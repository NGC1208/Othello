using System;
using System.Collections.Generic;

namespace Othello;

/// <summary>
/// 盤面の1マスに置かれている石の状態を表します。
/// </summary>
public enum Disc
{
    Empty = 0,
    Black = 1,
    White = 2
}

/// <summary>
/// 盤面上の0始まりの座標を表します。
/// Xは左から右、Yは上から下へ増加します。
/// </summary>
public readonly record struct BoardPosition(int X, int Y);

/// <summary>
/// 1回の着手によって置いた石と反転した石の情報を保持します。
/// UIの反転アニメーションにも利用します。
/// </summary>
public sealed class MoveResult
{
    public BoardPosition Position { get; }
    public Disc Player { get; }
    public IReadOnlyList<BoardPosition> Flipped { get; }

    /// <summary>
    /// 着手結果を生成します。
    /// </summary>
    public MoveResult(BoardPosition position, Disc player, IReadOnlyList<BoardPosition> flipped)
    {
        Position = position;
        Player = player;
        Flipped = flipped;
    }
}

/// <summary>
/// オセロの盤面状態とルール判定を管理します。
/// Godotの画面ノードには依存せず、CPU探索や自動テストから直接利用できます。
/// </summary>
public sealed class BoardState
{
    /// <summary>盤面の一辺のマス数です。</summary>
    public const int Size = 8;

    // 左上から時計回りに、縦・横・斜めの8方向を表します。
    private static readonly (int X, int Y)[] Directions =
    {
        (-1, -1), (0, -1), (1, -1),
        (-1, 0),            (1, 0),
        (-1, 1),  (0, 1),   (1, 1)
    };

    // 配列の添字は [X, Y] の順で使用します。
    private readonly Disc[,] _cells;

    /// <summary>次に着手するプレイヤーです。</summary>
    public Disc CurrentPlayer { get; private set; }

    /// <summary>最後に石を置いた座標です。初期状態ではnullです。</summary>
    public BoardPosition? LastMove { get; private set; }

    /// <summary>
    /// 標準の初期配置を持つ盤面を生成します。
    /// </summary>
    public BoardState()
    {
        _cells = new Disc[Size, Size];
        Reset();
    }

    // CPU探索用の複製を生成するときだけ使用する内部コンストラクターです。
    private BoardState(Disc[,] cells, Disc currentPlayer, BoardPosition? lastMove)
    {
        _cells = cells;
        CurrentPlayer = currentPlayer;
        LastMove = lastMove;
    }

    /// <summary>
    /// 盤面を標準の初期配置へ戻し、黒を先手に設定します。
    /// </summary>
    public void Reset()
    {
        Array.Clear(_cells);
        _cells[3, 3] = Disc.White; // D4
        _cells[4, 3] = Disc.Black; // E4
        _cells[3, 4] = Disc.Black; // D5
        _cells[4, 4] = Disc.White; // E5
        CurrentPlayer = Disc.Black;
        LastMove = null;
    }

    /// <summary>
    /// 現在の盤面を独立した配列として複製します。
    /// CPUは複製へ仮想着手するため、実際の対局盤面を変更しません。
    /// </summary>
    public BoardState Clone()
    {
        return new BoardState((Disc[,])_cells.Clone(), CurrentPlayer, LastMove);
    }

    /// <summary>
    /// 指定座標の状態を返します。盤外は空きマスとして扱います。
    /// </summary>
    public Disc GetCell(int x, int y)
    {
        return IsInside(x, y) ? _cells[x, y] : Disc.Empty;
    }

    /// <summary>
    /// パス処理などで次の手番を明示的に設定します。
    /// </summary>
    public void SetCurrentPlayer(Disc player)
    {
        if (player == Disc.Empty)
            throw new ArgumentException("手番に空きマスは指定できません。", nameof(player));

        CurrentPlayer = player;
    }

    /// <summary>
    /// 指定した色の相手色を返します。
    /// </summary>
    public static Disc Opponent(Disc player)
    {
        return player == Disc.Black ? Disc.White : Disc.Black;
    }

    /// <summary>
    /// 指定プレイヤーが現在着手できるすべての座標を返します。
    /// </summary>
    public List<BoardPosition> GetLegalMoves(Disc player)
    {
        var moves = new List<BoardPosition>();

        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                if (_cells[x, y] == Disc.Empty && GetFlips(x, y, player).Count > 0)
                    moves.Add(new BoardPosition(x, y));
            }
        }

        return moves;
    }

    /// <summary>
    /// 指定座標が指定プレイヤーの合法手か判定します。
    /// </summary>
    public bool IsLegalMove(BoardPosition position, Disc player)
    {
        return IsInside(position.X, position.Y)
            && _cells[position.X, position.Y] == Disc.Empty
            && GetFlips(position.X, position.Y, player).Count > 0;
    }

    /// <summary>
    /// 現在の手番プレイヤーとして着手を試みます。
    /// 非合法手の場合は盤面を変更せずnullを返します。
    /// </summary>
    public MoveResult? TryApplyMove(BoardPosition position)
    {
        return TryApplyMove(position, CurrentPlayer);
    }

    /// <summary>
    /// 指定プレイヤーとして着手し、成立した全方向の相手石を反転します。
    /// CPU探索ではCurrentPlayerにかかわらず仮想着手するため、このオーバーロードを使用します。
    /// </summary>
    public MoveResult? TryApplyMove(BoardPosition position, Disc player)
    {
        if (!IsInside(position.X, position.Y) || _cells[position.X, position.Y] != Disc.Empty)
            return null;

        var flips = GetFlips(position.X, position.Y, player);
        if (flips.Count == 0)
            return null;

        _cells[position.X, position.Y] = player;
        foreach (var flip in flips)
            _cells[flip.X, flip.Y] = player;

        LastMove = position;
        CurrentPlayer = Opponent(player);
        return new MoveResult(position, player, flips);
    }

    /// <summary>
    /// 指定プレイヤーに合法手がなく、パスが必要か判定します。
    /// </summary>
    public bool MustPass(Disc player)
    {
        return GetLegalMoves(player).Count == 0;
    }

    /// <summary>
    /// 空きマスがない、または両者とも合法手がない場合にtrueを返します。
    /// </summary>
    public bool IsGameOver()
    {
        return Count(Disc.Empty) == 0
            || (MustPass(Disc.Black) && MustPass(Disc.White));
    }

    /// <summary>
    /// 指定した状態のマス数を数えます。
    /// </summary>
    public int Count(Disc disc)
    {
        var count = 0;
        for (var y = 0; y < Size; y++)
        for (var x = 0; x < Size; x++)
            if (_cells[x, y] == disc)
                count++;
        return count;
    }

    /// <summary>
    /// 現在の石数から勝者を返します。同数の場合はDisc.Emptyです。
    /// </summary>
    public Disc GetWinner()
    {
        var black = Count(Disc.Black);
        var white = Count(Disc.White);
        return black == white ? Disc.Empty : black > white ? Disc.Black : Disc.White;
    }

    /// <summary>
    /// 指定位置へ石を置いた場合に反転する全座標を収集します。
    /// 各方向について、連続する相手石の先に自分の石がある場合だけ成立します。
    /// </summary>
    private List<BoardPosition> GetFlips(int x, int y, Disc player)
    {
        var result = new List<BoardPosition>();
        if (!IsInside(x, y) || _cells[x, y] != Disc.Empty || player == Disc.Empty)
            return result;

        var opponent = Opponent(player);

        foreach (var direction in Directions)
        {
            // 1方向分の相手石を一時保存し、最後に自分の石へ到達した場合だけ採用します。
            var line = new List<BoardPosition>();
            var nx = x + direction.X;
            var ny = y + direction.Y;

            while (IsInside(nx, ny) && _cells[nx, ny] == opponent)
            {
                line.Add(new BoardPosition(nx, ny));
                nx += direction.X;
                ny += direction.Y;
            }

            if (line.Count > 0 && IsInside(nx, ny) && _cells[nx, ny] == player)
                result.AddRange(line);
        }

        return result;
    }

    /// <summary>
    /// 座標が8×8の盤面内にあるか判定します。
    /// </summary>
    private static bool IsInside(int x, int y)
    {
        return x >= 0 && x < Size && y >= 0 && y < Size;
    }
}
