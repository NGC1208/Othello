using System;
using System.Collections.Generic;

namespace Othello;

/// <summary>
/// 外部テストフレームワークを使わず、Godotのヘッドレス実行から呼び出せる
/// 最小限のルール・CPU自己テストです。
/// </summary>
public static class RulesSelfTest
{
    /// <summary>
    /// 登録済みの自己テストを順番に実行し、すべて成功した場合だけ完了メッセージを出力します。
    /// </summary>
    public static void RunAll()
    {
        TestInitialBoard();
        TestInitialLegalMoves();
        TestSingleDirectionFlip();
        TestMultipleDirectionFlip();
        TestCpuDifficulties();
        TestCpuIsDeterministic();
        Console.WriteLine("SELF_TEST_OK");
    }

    /// <summary>標準ルールどおり中央4マスが初期化されることを確認します。</summary>
    private static void TestInitialBoard()
    {
        var board = new BoardState();
        Assert(board.Count(Disc.Black) == 2, "初期黒石数");
        Assert(board.Count(Disc.White) == 2, "初期白石数");
        Assert(board.GetCell(3, 3) == Disc.White, "D4は白");
        Assert(board.GetCell(4, 3) == Disc.Black, "E4は黒");
        Assert(board.GetCell(3, 4) == Disc.Black, "D5は黒");
        Assert(board.GetCell(4, 4) == Disc.White, "E5は白");
    }

    /// <summary>初期盤面で黒の合法手が正しい4マスになることを確認します。</summary>
    private static void TestInitialLegalMoves()
    {
        var board = new BoardState();
        var expected = new HashSet<BoardPosition>
        {
            new(2, 3), new(3, 2), new(4, 5), new(5, 4)
        };
        var actual = board.GetLegalMoves(Disc.Black);
        Assert(actual.Count == 4 && expected.SetEquals(actual), "黒の初期合法手");
    }

    /// <summary>1方向で挟んだ石が正しく反転することを確認します。</summary>
    private static void TestSingleDirectionFlip()
    {
        var board = new BoardState();
        var result = board.TryApplyMove(new BoardPosition(2, 3), Disc.Black);
        Assert(result is not null, "C4へ着手可能");
        Assert(result!.Flipped.Count == 1, "1枚反転");
        Assert(board.GetCell(3, 3) == Disc.Black, "D4が黒へ反転");
    }

    /// <summary>複数着手後も合法手判定が継続して機能することを確認します。</summary>
    private static void TestMultipleDirectionFlip()
    {
        var cells = new Disc[BoardState.Size, BoardState.Size];

        // D4へ黒を置くと、上下左右の白石を同時に反転する配置です。
        cells[3, 1] = Disc.Black;
        cells[3, 2] = Disc.White;
        cells[1, 3] = Disc.Black;
        cells[2, 3] = Disc.White;
        cells[4, 3] = Disc.White;
        cells[5, 3] = Disc.Black;
        cells[3, 4] = Disc.White;
        cells[3, 5] = Disc.Black;

        var board = BoardState.CreateForTesting(cells, Disc.Black);
        var result = board.TryApplyMove(new BoardPosition(3, 3), Disc.Black);

        Assert(result is not null, "D4へ複数方向の着手が成立");
        Assert(result!.Flipped.Count == 4, "上下左右の4枚を同時に反転");
        Assert(board.GetCell(3, 2) == Disc.Black, "上方向を反転");
        Assert(board.GetCell(2, 3) == Disc.Black, "左方向を反転");
        Assert(board.GetCell(4, 3) == Disc.Black, "右方向を反転");
        Assert(board.GetCell(3, 4) == Disc.Black, "下方向を反転");
    }

    /// <summary>全難易度の探索深度と合法手選択を確認します。</summary>
    private static void TestCpuDifficulties()
    {
        var board = new BoardState();
        var cases = new[]
        {
            (CpuDifficulty.Beginner, 1),
            (CpuDifficulty.Intermediate, 2),
            (CpuDifficulty.Advanced, 3)
        };

        foreach (var testCase in cases)
        {
            var cpu = new CpuPlayer(testCase.Item1);
            var move = cpu.ChooseMove(board, Disc.Black);
            Assert(cpu.MaxDepth == testCase.Item2, $"{testCase.Item1}の探索深度");
            Assert(move.HasValue && board.IsLegalMove(move.Value, Disc.Black), $"{testCase.Item1}は合法手を返す");
        }
    }

    /// <summary>同じ盤面と難易度なら同じ手を選ぶことを確認します。</summary>
    private static void TestCpuIsDeterministic()
    {
        var board = new BoardState();
        foreach (var difficulty in Enum.GetValues<CpuDifficulty>())
        {
            var first = new CpuPlayer(difficulty).ChooseMove(board, Disc.Black);
            var second = new CpuPlayer(difficulty).ChooseMove(board, Disc.Black);
            Assert(first == second, $"{difficulty}の着手は再現可能");
        }
    }

    /// <summary>
    /// 条件が偽ならテスト名を含む例外を送出します。
    /// </summary>
    private static void Assert(bool condition, string name)
    {
        if (!condition)
            throw new InvalidOperationException($"自己テスト失敗: {name}");
    }
}
