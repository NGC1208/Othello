using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Othello;

/// <summary>
/// α-β枝刈り付きミニマックス法でCPUの着手を選びます。
/// 深さ1から3まで反復深化し、制限時間内に完了した最深探索の結果を採用します。
/// </summary>
public sealed class CpuPlayer
{
    // 評価関数で特に価値が高い盤面四隅の座標です。
    private static readonly BoardPosition[] Corners =
    {
        new(0, 0), new(7, 0), new(0, 7), new(7, 7)
    };

    // 同評価の候補から手を選ぶための乱数です。テスト時はシードを固定できます。
    private readonly Random _random;
    private readonly TimeSpan _timeLimit;
    private readonly int _maxDepth;
    private Stopwatch _stopwatch = new();
    private bool _timedOut;

    /// <summary>
    /// CPU探索器を生成します。
    /// </summary>
    /// <param name="randomSeed">再現可能なテストで使用する乱数シードです。</param>
    /// <param name="timeLimitSeconds">1着手あたりの思考時間上限です。</param>
    /// <param name="maxDepth">CPUと人間を合わせて何着手先まで読むかを指定します。</param>
    public CpuPlayer(int? randomSeed = null, double timeLimitSeconds = 0.5, int maxDepth = 3)
    {
        _random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
        _timeLimit = TimeSpan.FromSeconds(timeLimitSeconds);
        _maxDepth = maxDepth;
    }

    /// <summary>
    /// 指定盤面からCPUの最善手を選びます。
    /// 合法手がない場合はnullを返します。
    /// </summary>
    public BoardPosition? ChooseMove(BoardState board, Disc cpuDisc)
    {
        var legalMoves = board.GetLegalMoves(cpuDisc);
        if (legalMoves.Count == 0)
            return null;

        var fallback = legalMoves[0];
        var bestMoves = new List<BoardPosition> { fallback };
        _stopwatch = Stopwatch.StartNew();

        // 浅い探索を必ず先に完了させることで、途中で時間切れになっても有効な手を返せます。
        for (var depth = 1; depth <= _maxDepth; depth++)
        {
            _timedOut = false;
            var depthBest = SearchRoot(board, cpuDisc, depth);
            if (_timedOut)
                break;

            if (depthBest.Count > 0)
                bestMoves = depthBest;
        }

        _stopwatch.Stop();
        return bestMoves[_random.Next(bestMoves.Count)];
    }

    /// <summary>
    /// 現在の探索深度における最善の着手候補をすべて返します。
    /// </summary>
    private List<BoardPosition> SearchRoot(BoardState board, Disc cpuDisc, int depth)
    {
        var bestScore = int.MinValue;
        var bestMoves = new List<BoardPosition>();

        foreach (var move in board.GetLegalMoves(cpuDisc))
        {
            if (HasTimedOut())
                break;

            // 元の盤面を壊さないよう、候補ごとに盤面を複製して探索します。
            var child = board.Clone();
            child.TryApplyMove(move, cpuDisc);
            var score = Minimax(child, BoardState.Opponent(cpuDisc), cpuDisc, depth - 1, int.MinValue + 1, int.MaxValue);

            if (_timedOut)
                break;

            if (score > bestScore)
            {
                bestScore = score;
                bestMoves.Clear();
                bestMoves.Add(move);
            }
            else if (score == bestScore)
            {
                bestMoves.Add(move);
            }
        }

        return bestMoves;
    }

    /// <summary>
    /// CPUは評価値を最大化し、人間は評価値を最小化すると仮定してゲーム木を探索します。
    /// alphaとbetaにより、最終結果へ影響しない枝を省略します。
    /// </summary>
    private int Minimax(BoardState board, Disc player, Disc cpuDisc, int depth, int alpha, int beta)
    {
        if (HasTimedOut())
            return Evaluate(board, cpuDisc);

        if (board.IsGameOver() || depth <= 0)
            return Evaluate(board, cpuDisc);

        var moves = board.GetLegalMoves(player);
        if (moves.Count == 0)
            // パスは着手ではないため探索深度を消費せず、相手へ手番だけ渡します。
            return Minimax(board, BoardState.Opponent(player), cpuDisc, depth, alpha, beta);

        var maximizing = player == cpuDisc;
        var best = maximizing ? int.MinValue : int.MaxValue;

        foreach (var move in moves)
        {
            var child = board.Clone();
            child.TryApplyMove(move, player);
            var score = Minimax(child, BoardState.Opponent(player), cpuDisc, depth - 1, alpha, beta);

            if (_timedOut)
                return score;

            if (maximizing)
            {
                best = Math.Max(best, score);
                alpha = Math.Max(alpha, best);
            }
            else
            {
                best = Math.Min(best, score);
                beta = Math.Min(beta, best);
            }

            if (beta <= alpha)
                // この先を読んでも親ノードの選択が変わらないため枝刈りします。
                break;
        }

        return best;
    }

    /// <summary>
    /// 思考時間の上限を確認し、超過していれば探索中断フラグを立てます。
    /// </summary>
    private bool HasTimedOut()
    {
        if (_stopwatch.Elapsed < _timeLimit)
            return false;

        _timedOut = true;
        return true;
    }

    /// <summary>
    /// CPUから見た局面の有利・不利を整数値で評価します。
    /// 序盤・中盤・終盤で合法手、角、危険マス、石数の重みを切り替えます。
    /// </summary>
    private static int Evaluate(BoardState board, Disc cpuDisc)
    {
        var opponent = BoardState.Opponent(cpuDisc);
        var discDifference = board.Count(cpuDisc) - board.Count(opponent);

        if (board.IsGameOver())
        {
            // 終局時の勝敗は通常の局面評価より常に優先される大きな値を使用します。
            var winner = board.GetWinner();
            if (winner == Disc.Empty)
                return 0;
            return winner == cpuDisc ? 1_000_000 + discDifference : -1_000_000 + discDifference;
        }

        var empty = board.Count(Disc.Empty);
        var mobilityDifference = board.GetLegalMoves(cpuDisc).Count - board.GetLegalMoves(opponent).Count;
        var cornerDifference = CountCorners(board, cpuDisc) - CountCorners(board, opponent);
        var dangerDifference = CountDangerSquares(board, cpuDisc) - CountDangerSquares(board, opponent);

        if (empty >= 41)
            return mobilityDifference * 20 + cornerDifference * 100 - dangerDifference * 30 + discDifference;
        if (empty >= 13)
            return mobilityDifference * 15 + cornerDifference * 120 - dangerDifference * 25 + discDifference * 3;
        return mobilityDifference * 5 + cornerDifference * 120 - dangerDifference * 10 + discDifference * 20;
    }

    /// <summary>
    /// 指定プレイヤーが所有する角の数を返します。
    /// </summary>
    private static int CountCorners(BoardState board, Disc player)
    {
        var count = 0;
        foreach (var corner in Corners)
            if (board.GetCell(corner.X, corner.Y) == player)
                count++;
        return count;
    }

    /// <summary>
    /// まだ取られていない角に隣接する危険マスを何個所有しているか数えます。
    /// 角が取得済みなら、その角の周囲は危険マスとして数えません。
    /// </summary>
    private static int CountDangerSquares(BoardState board, Disc player)
    {
        var count = 0;

        foreach (var corner in Corners)
        {
            if (board.GetCell(corner.X, corner.Y) != Disc.Empty)
                continue;

            var dx = corner.X == 0 ? 1 : -1;
            var dy = corner.Y == 0 ? 1 : -1;
            var adjacent = new[]
            {
                new BoardPosition(corner.X + dx, corner.Y),
                new BoardPosition(corner.X, corner.Y + dy),
                new BoardPosition(corner.X + dx, corner.Y + dy)
            };

            foreach (var position in adjacent)
                if (board.GetCell(position.X, position.Y) == player)
                    count++;
        }

        return count;
    }
}
