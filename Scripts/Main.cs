using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Othello;

/// <summary>
/// 対局画面、入力、CPU思考、演出、リザルトを統括するGodotのメインノードです。
/// ルール判定自体はBoardStateへ、思考処理はCpuPlayerへ委譲します。
/// </summary>
public partial class Main : Control
{
    // 仕様書で定めた、装飾を抑えた盤面・背景・強調枠の配色です。
    private readonly Color _boardColor = new("#178447");
	private readonly Color _gridColor = new("#082f1c");
	private readonly Color _backgroundColor = new("#101820");
	private readonly Color _lastMoveColor = new("#ffd54f");

    // 対局状態とCPU思考器です。CPUは黒、人間は白で固定します。
    private BoardState _board = new();
	private readonly CpuPlayer _cpu = new();
	private Label _turnLabel = null!;
	private Label _scoreLabel = null!;
	private Label _messageLabel = null!;
	private ColorRect _resultOverlay = null!;
	private Label _resultLabel = null!;
	private Button _replayButton = null!;
	private Button _quitButton = null!;
	private Rect2 _boardRect;
	private float _cellSize;
	private Task<BoardPosition?>? _cpuTask;
	private double _cpuStartDelay;
	private double _messageRemaining;
	private bool _inputLocked;
	private bool _gameOver;
    private readonly Dictionary<BoardPosition, FlipAnimation> _flips = new();

    /// <summary>
    /// 反転中の石について、反転前後の色と経過時間を保持します。
    /// </summary>
    private sealed class FlipAnimation
	{
		public Disc From { get; init; }
		public Disc To { get; init; }
		public double Elapsed { get; set; }
    }

    /// <summary>
    /// 起動時に自己テストモードを判定し、通常時はUI生成後に新しい対局を開始します。
    /// </summary>
    public override void _Ready()
	{
		if (OS.GetCmdlineUserArgs().Contains("--self-test"))
		{
			try
			{
				RulesSelfTest.RunAll();
				GetTree().Quit(0);
			}
			catch (Exception exception)
			{
				GD.PrintErr(exception);
				GetTree().Quit(1);
			}
			return;
		}

		SetProcess(true);
		MouseFilter = MouseFilterEnum.Stop;
		CreateInterface();
		StartNewGame();

		if (OS.GetCmdlineUserArgs().Contains("--capture-preview"))
			CapturePreview();
    }

    /// <summary>
    /// 画面確認用のプレビュー画像を保存して終了します。
    /// 通常プレイでは呼び出されません。
    /// </summary>
    private async void CapturePreview()
	{
		for (var i = 0; i < 90; i++)
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		var image = GetViewport().GetTexture().GetImage();
		var result = image.SavePng("res://preview.png");
		if (result != Error.Ok)
			GD.PrintErr($"プレビュー画像の保存に失敗しました: {result}");
		GetTree().Quit(result == Error.Ok ? 0 : 1);
    }

    /// <summary>
    /// 手番・石数・メッセージ・リザルトのUI部品をコードから生成します。
    /// </summary>
    private void CreateInterface()
	{
		_turnLabel = CreateLabel(26, HorizontalAlignment.Center);
		_scoreLabel = CreateLabel(24, HorizontalAlignment.Center);
		_messageLabel = CreateLabel(22, HorizontalAlignment.Center);
		_messageLabel.Modulate = new Color("#ffe082");

		_resultOverlay = new ColorRect
		{
			Color = new Color(0.02f, 0.03f, 0.04f, 0.94f),
			Visible = false,
			MouseFilter = MouseFilterEnum.Stop
		};
		AddChild(_resultOverlay);

		_resultLabel = CreateLabel(36, HorizontalAlignment.Center, _resultOverlay);
		_replayButton = CreateButton("もう一度遊ぶ", _resultOverlay);
		_quitButton = CreateButton("ゲームを終了", _resultOverlay);
		_replayButton.Pressed += StartNewGame;
		_quitButton.Pressed += () => GetTree().Quit();

		Resized += LayoutInterface;
		LayoutInterface();
    }

    /// <summary>
    /// 共通設定を持つラベルを生成し、指定された親ノードへ追加します。
    /// </summary>
    private Label CreateLabel(int fontSize, HorizontalAlignment alignment, Control? parent = null)
	{
		var label = new Label
		{
			HorizontalAlignment = alignment,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = MouseFilterEnum.Ignore
		};
		label.AddThemeFontSizeOverride("font_size", fontSize);
		(parent ?? this).AddChild(label);
		return label;
    }

    /// <summary>
    /// リザルト画面で使用するボタンを生成します。
    /// </summary>
    private static Button CreateButton(string text, Control parent)
	{
		var button = new Button { Text = text };
		button.AddThemeFontSizeOverride("font_size", 22);
		parent.AddChild(button);
		return button;
    }

    /// <summary>
    /// ウィンドウサイズから盤面とUIの位置・大きさを再計算します。
    /// 盤面は常に正方形を維持します。
    /// </summary>
    private void LayoutInterface()
	{
		var size = Size;
		var availableHeight = Math.Max(320f, size.Y - 170f);
		var availableWidth = Math.Max(320f, size.X - 80f);
		_cellSize = MathF.Floor(MathF.Min(availableWidth / 8f, availableHeight / 8f));
		var boardPixels = _cellSize * 8f;
		_boardRect = new Rect2((size.X - boardPixels) / 2f, 105f, boardPixels, boardPixels);

		_turnLabel.Position = new Vector2(20, 14);
		_turnLabel.Size = new Vector2(size.X - 40, 38);
		_scoreLabel.Position = new Vector2(20, 52);
		_scoreLabel.Size = new Vector2(size.X - 40, 34);
		_messageLabel.Position = new Vector2(20, Math.Min(size.Y - 55f, _boardRect.End.Y + 8f));
		_messageLabel.Size = new Vector2(size.X - 40, 40);

		var overlayWidth = Math.Min(520f, size.X - 40f);
		var overlayHeight = 310f;
		_resultOverlay.Position = new Vector2((size.X - overlayWidth) / 2f, (size.Y - overlayHeight) / 2f);
		_resultOverlay.Size = new Vector2(overlayWidth, overlayHeight);
		_resultLabel.Position = new Vector2(20, 25);
		_resultLabel.Size = new Vector2(overlayWidth - 40, 130);
		_replayButton.Position = new Vector2(60, 180);
		_replayButton.Size = new Vector2(overlayWidth - 120, 48);
		_quitButton.Position = new Vector2(60, 240);
		_quitButton.Size = new Vector2(overlayWidth - 120, 48);
		QueueRedraw();
    }

    /// <summary>
    /// 盤面と一時状態を初期化し、黒CPUの初手から新しい対局を始めます。
    /// </summary>
    private void StartNewGame()
	{
		_board = new BoardState();
		_flips.Clear();
		_cpuTask = null;
		_cpuStartDelay = 0.35;
		_messageRemaining = 0;
		_inputLocked = true;
		_gameOver = false;
		_resultOverlay.Visible = false;
		_messageLabel.Text = "";
		UpdateStatus();
		QueueRedraw();
    }

    /// <summary>
    /// メッセージ表示、反転演出、非同期CPU探索をフレームごとに進めます。
    /// </summary>
    public override void _Process(double delta)
	{
		if (_messageRemaining > 0)
		{
			_messageRemaining -= delta;
			if (_messageRemaining <= 0)
				_messageLabel.Text = "";
		}

		UpdateFlipAnimations(delta);

		if (_gameOver || _flips.Count > 0)
			return;

		if (_board.CurrentPlayer == Disc.Black)
		{
			_inputLocked = true;
			if (_cpuStartDelay > 0)
			{
				_cpuStartDelay -= delta;
				return;
			}

            if (_cpuTask is null)
            {
                // 画面を止めないよう、CPU探索は盤面の複製を使って別スレッドで実行します。
                var snapshot = _board.Clone();
				_cpuTask = Task.Run(() => _cpu.ChooseMove(snapshot, Disc.Black));
			}
			else if (_cpuTask.IsCompleted)
			{
				var move = _cpuTask.Result;
				_cpuTask = null;
				if (move.HasValue)
					ApplyMove(move.Value, Disc.Black);
				else
					HandlePass(Disc.Black);
			}
		}
		else
		{
			_inputLocked = false;
		}
    }

    /// <summary>
    /// 0.2秒の石反転アニメーションを更新し、完了後に次の手番処理へ進みます。
    /// </summary>
    private void UpdateFlipAnimations(double delta)
	{
		if (_flips.Count == 0)
			return;

		foreach (var animation in _flips.Values)
			animation.Elapsed += delta;

		var completed = _flips.Where(pair => pair.Value.Elapsed >= 0.2).Select(pair => pair.Key).ToArray();
		foreach (var position in completed)
			_flips.Remove(position);

		QueueRedraw();

		if (_flips.Count == 0)
			ContinueTurnFlow();
    }

    /// <summary>
    /// 人間の左クリックを盤面座標へ変換し、合法手の場合だけ着手します。
    /// 非合法手は仕様どおり無反応です。
    /// </summary>
    public override void _GuiInput(InputEvent @event)
	{
		if (_inputLocked || _gameOver || _board.CurrentPlayer != Disc.White)
			return;

		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } mouse)
			return;

		if (!_boardRect.HasPoint(mouse.Position))
			return;

		var x = (int)((mouse.Position.X - _boardRect.Position.X) / _cellSize);
		var y = (int)((mouse.Position.Y - _boardRect.Position.Y) / _cellSize);
		var position = new BoardPosition(x, y);

		if (_board.IsLegalMove(position, Disc.White))
			ApplyMove(position, Disc.White);
    }

    /// <summary>
    /// 盤面へ着手を反映し、反転対象のアニメーションを開始します。
    /// </summary>
    private void ApplyMove(BoardPosition position, Disc player)
	{
		var result = _board.TryApplyMove(position, player);
		if (result is null)
			return;

		_inputLocked = true;
		var from = BoardState.Opponent(player);
		foreach (var flipped in result.Flipped)
			_flips[flipped] = new FlipAnimation { From = from, To = player };

		UpdateStatus();
		QueueRedraw();

		if (_flips.Count == 0)
			ContinueTurnFlow();
    }

    /// <summary>
    /// 反転完了後に終局・パス・次の通常手番を判定します。
    /// </summary>
    private void ContinueTurnFlow()
	{
		if (_board.IsGameOver())
		{
			ShowResult();
			return;
		}

		var next = _board.CurrentPlayer;
		if (_board.MustPass(next))
		{
			HandlePass(next);
			return;
		}

		if (next == Disc.Black)
			_cpuStartDelay = 0.25;
		else
			_inputLocked = false;

		UpdateStatus();
    }

    /// <summary>
    /// パスメッセージを約1秒表示し、相手へ手番を渡します。
    /// 相手にも合法手がなければその場で終局します。
    /// </summary>
    private void HandlePass(Disc player)
	{
		_messageLabel.Text = player == Disc.White
			? "あなたは置ける場所がないためパスします"
			: "CPUは置ける場所がないためパスします";
		_messageRemaining = 1.0;

		var next = BoardState.Opponent(player);
		_board.SetCurrentPlayer(next);

		if (_board.MustPass(next))
		{
			ShowResult();
			return;
		}

		if (next == Disc.Black)
			_cpuStartDelay = 1.0;
		else
			_inputLocked = false;

		UpdateStatus();
    }

    /// <summary>
    /// 勝敗と最終石数をリザルト表示へ反映し、対局操作を停止します。
    /// </summary>
    private void ShowResult()
	{
		_gameOver = true;
		_inputLocked = true;
		var winner = _board.GetWinner();
		var result = winner == Disc.Empty ? "引き分け" : winner == Disc.White ? "あなたの勝ち" : "あなたの負け";
		_resultLabel.Text = $"{result}\n黒 {_board.Count(Disc.Black)} － {_board.Count(Disc.White)} 白";
		_resultOverlay.Visible = true;
		UpdateStatus();
    }

    /// <summary>
    /// 現在の手番と黒白の石数を画面上部へ反映します。
    /// </summary>
    private void UpdateStatus()
	{
		_turnLabel.Text = _gameOver
			? "対局終了"
			: _board.CurrentPlayer == Disc.Black ? "CPUの手番" : "あなたの手番";
		_scoreLabel.Text = $"黒（CPU） {_board.Count(Disc.Black)}　　白（あなた） {_board.Count(Disc.White)}";
    }

    /// <summary>
    /// 背景、盤面、格子、石、最後の着手枠を描画します。
    /// 外部画像素材は使用しません。
    /// </summary>
    public override void _Draw()
	{
		DrawRect(new Rect2(Vector2.Zero, Size), _backgroundColor);
		DrawRect(_boardRect, _boardColor);

		for (var i = 0; i <= BoardState.Size; i++)
		{
			var offset = i * _cellSize;
			DrawLine(_boardRect.Position + new Vector2(offset, 0), _boardRect.Position + new Vector2(offset, _boardRect.Size.Y), _gridColor, 2);
			DrawLine(_boardRect.Position + new Vector2(0, offset), _boardRect.Position + new Vector2(_boardRect.Size.X, offset), _gridColor, 2);
		}

		for (var y = 0; y < BoardState.Size; y++)
		for (var x = 0; x < BoardState.Size; x++)
			DrawDisc(x, y, _board.GetCell(x, y));

		if (_board.LastMove is { } last)
		{
			var rect = CellRect(last.X, last.Y).Grow(-4);
			DrawRect(rect, _lastMoveColor, false, 4);
		}
    }

    /// <summary>
    /// 指定マスの石を描画します。
    /// 反転中は横方向の縮小と色の切り替えで裏返る動きを表現します。
    /// </summary>
    private void DrawDisc(int x, int y, Disc disc)
	{
		if (disc == Disc.Empty)
			return;

		var position = new BoardPosition(x, y);
		var center = CellRect(x, y).GetCenter();
		var radius = _cellSize * 0.38f;
		var shownDisc = disc;
		var scaleX = 1f;

		if (_flips.TryGetValue(position, out var animation))
		{
			var progress = Math.Clamp(animation.Elapsed / 0.2, 0.0, 1.0);
			scaleX = (float)Math.Abs(Math.Cos(progress * Math.PI));
			shownDisc = progress < 0.5 ? animation.From : animation.To;
		}

		DrawSetTransform(center, 0, new Vector2(Math.Max(0.04f, scaleX), 1));
		DrawCircle(Vector2.Zero, radius, shownDisc == Disc.Black ? Colors.Black : new Color("#f4f4ef"));
		DrawCircle(Vector2.Zero, radius, new Color(0, 0, 0, 0.45f), false, 2);
		DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }

    /// <summary>
    /// 盤面座標から画面上のマス領域を計算します。
    /// </summary>
    private Rect2 CellRect(int x, int y)
	{
		return new Rect2(
			_boardRect.Position + new Vector2(x * _cellSize, y * _cellSize),
			new Vector2(_cellSize, _cellSize));
	}
}
