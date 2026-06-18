using System;
using System.Collections.Generic;

namespace Othello;

/// <summary>
/// プレイヤーが選択できるCPU難易度です。
/// </summary>
public enum CpuDifficulty
{
    Beginner,
    Intermediate,
    Advanced
}

/// <summary>
/// 難易度に応じた評価関数とα-β枝刈り付きミニマックス法でCPUの着手を選びます。
/// </summary>
public sealed class CpuPlayer
{
    // 評価関数で特に価値が高い盤面四隅の座標です。
    private static readonly BoardPosition[] Corners =
    {
        new(0, 0), new(7, 0), new(0, 7), new(7, 7)
    };

    /// <summary>現在選択されている難易度です。</summary>
    public CpuDifficulty Difficulty { get; }

    /// <summary>難易度に対応する最大探索深度です。</summary>
    public int MaxDepth => Difficulty switch
    {
        CpuDifficulty.Beginner => 1,
        CpuDifficulty.Intermediate => 2,
        _ => 3
    };

    /// <summary>
    /// 指定難易度のCPU探索器を生成します。
    /// </summary>
    public CpuPlayer(CpuDifficulty difficulty)
    {
        Difficulty = difficulty;
    }

    /// <summary>
    /// 指定盤面からCPUの最善手を選びます。
    /// 合法手がない場合はnullを返します。
    /// 同評価の場合は左上から右下への座標順で最初の手を選びます。
    /// </summary>
    public BoardPosition? ChooseMove(BoardState board, Disc cpuDisc)
    {
        var legalMoves = board.GetLegalMoves(cpuDisc);
        if (legalMoves.Count == 0)
            return null;

        SortByCoordinate(legalMoves);
        // 時間依存で探索深度が変わらないよう、難易度ごとの固定深度を必ず探索します。
        return SearchRoot(board, cpuDisc, MaxDepth) ?? legalMoves[0];
    }

    /// <summary>
    /// 現在の探索深度で最も評価が高い着手を返します。
    /// </summary>
    private BoardPosition? SearchRoot(BoardState board, Disc cpuDisc, int depth)
    {
        var bestScore = int.MinValue;
        BoardPosition? bestMove = null;
        var moves = board.GetLegalMoves(cpuDisc);
        SortByCoordinate(moves);

        foreach (var move in moves)
        {
            var child = board.Clone();
            var result = child.TryApplyMove(move, cpuDisc);
            if (result is null)
                continue;

            int score;
            if (Difficulty == CpuDifficulty.Beginner)
            {
                // 初級は相手の応手を読まず、着手直後の分かりやすい成果を評価します。
                score = EvaluateBeginnerMove(child, move, result.Flipped.Count, cpuDisc);
            }
            else
            {
                score = Minimax(
                    child,
                    BoardState.Opponent(cpuDisc),
                    cpuDisc,
                    depth - 1,
                    int.MinValue + 1,
                    int.MaxValue);
            }

            // 同評価では先に調べた座標順の手を維持します。
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    /// <summary>
    /// CPUは評価値を最大化し、人間は評価値を最小化すると仮定してゲーム木を探索します。
    /// </summary>
    private int Minimax(BoardState board, Disc player, Disc cpuDisc, int depth, int alpha, int beta)
    {
        if (board.IsGameOver() || depth <= 0)
            return EvaluateStrategic(board, cpuDisc);

        var moves = board.GetLegalMoves(player);
        if (moves.Count == 0)
            // パスは着手ではないため探索深度を消費しません。
            return Minimax(board, BoardState.Opponent(player), cpuDisc, depth, alpha, beta);

        SortByCoordinate(moves);
        var maximizing = player == cpuDisc;
        var best = maximizing ? int.MinValue : int.MaxValue;

        foreach (var move in moves)
        {
            var child = board.Clone();
            child.TryApplyMove(move, player);
            var score = Minimax(child, BoardState.Opponent(player), cpuDisc, depth - 1, alpha, beta);

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
                break;
        }

        return best;
    }

    /// <summary>
    /// 初心者が選びそうな、目先の成果を重視した着手評価を計算します。
    /// </summary>
    private static int EvaluateBeginnerMove(
        BoardState boardAfterMove,
        BoardPosition move,
        int flippedCount,
        Disc cpuDisc)
    {
        var opponent = BoardState.Opponent(cpuDisc);
        var discDifference = boardAfterMove.Count(cpuDisc) - boardAfterMove.Count(opponent);
        var score = flippedCount * 20 + discDifference * 5;

        if (IsCorner(move))
            score += 100;
        if (IsEdge(move))
            score += 15;
        if (IsAdjacentToEmptyCorner(boardAfterMove, move))
            score -= 5;

        return score;
    }

    /// <summary>
    /// 中級・上級用に、局面全体の有利・不利を評価します。
    /// </summary>
    private static int EvaluateStrategic(BoardState board, Disc cpuDisc)
    {
        var opponent = BoardState.Opponent(cpuDisc);
        var discDifference = board.Count(cpuDisc) - board.Count(opponent);

        if (board.IsGameOver())
        {
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

    private static void SortByCoordinate(List<BoardPosition> moves)
    {
        moves.Sort((left, right) =>
        {
            var yComparison = left.Y.CompareTo(right.Y);
            return yComparison != 0 ? yComparison : left.X.CompareTo(right.X);
        });
    }

    private static bool IsCorner(BoardPosition position)
    {
        return (position.X == 0 || position.X == 7)
            && (position.Y == 0 || position.Y == 7);
    }

    private static bool IsEdge(BoardPosition position)
    {
        return position.X == 0 || position.X == 7 || position.Y == 0 || position.Y == 7;
    }

    /// <summary>
    /// 着手位置が、まだ空いている角の隣接3マスに含まれるか判定します。
    /// </summary>
    private static bool IsAdjacentToEmptyCorner(BoardState board, BoardPosition position)
    {
        foreach (var corner in Corners)
        {
            if (board.GetCell(corner.X, corner.Y) != Disc.Empty)
                continue;

            if (Math.Abs(position.X - corner.X) <= 1
                && Math.Abs(position.Y - corner.Y) <= 1
                && position != corner)
                return true;
        }

        return false;
    }

    private static int CountCorners(BoardState board, Disc player)
    {
        var count = 0;
        foreach (var corner in Corners)
            if (board.GetCell(corner.X, corner.Y) == player)
                count++;
        return count;
    }

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
