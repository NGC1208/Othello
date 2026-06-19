# 開発環境

## 使用環境

- Godot: `4.6.3 stable Mono`
- 使用言語: C#
- .NETターゲット: `net8.0`
- 対応プラットフォーム: Windows

## 実行ファイル

- Godot GUI:
  `D:\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64.exe`
- Godot Console:
  `D:\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64_console.exe`

## 標準検証コマンド

作業コマンドは原則として`D:\GodotProjects\Othello`をカレントディレクトリにして実行する。

### ビルド

```powershell
dotnet build "D:\GodotProjects\Othello\Othello.csproj"
```

期待結果：警告0、エラー0。

### 自己テスト

```powershell
& "D:\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64_console.exe" --headless --path "D:\GodotProjects\Othello" -- --self-test
```

期待結果：`SELF_TEST_OK`。

### メインシーンのヘッドレス起動

```powershell
& "D:\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64_console.exe" --headless --path "D:\GodotProjects\Othello" --quit-after 180
```

### 難易度別起動確認

```powershell
& "D:\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64_console.exe" --headless --path "D:\GodotProjects\Othello" --quit-after 180 -- --difficulty=beginner
& "D:\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64_console.exe" --headless --path "D:\GodotProjects\Othello" --quit-after 180 -- --difficulty=intermediate
& "D:\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64_console.exe" --headless --path "D:\GodotProjects\Othello" --quit-after 180 -- --difficulty=advanced
```

## 関連文書

- [[architecture|技術設計]]
- [[testing_strategy|テスト戦略]]

