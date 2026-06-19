# Othello AI開発ガイド

このファイルは、新しいAIセッションがプロジェクトの場所と経緯を知らなくても、安全に開発を再開するための入口兼引き継ぎ書です。

## プロジェクトの場所

- プロジェクトルート: `D:\GodotProjects\Othello`
- 仕様Vault: `D:\GodotProjects\Othello\Doc`
- Godotプロジェクト: `D:\GodotProjects\Othello\project.godot`
- C#プロジェクト: `D:\GodotProjects\Othello\Othello.csproj`
- ソースコード: `D:\GodotProjects\Othello\Scripts`
- GitHub: `https://github.com/NGC1208/Othello`
- 標準ブランチ: `main`

作業コマンドは原則として `D:\GodotProjects\Othello` をカレントディレクトリにして実行する。

## 開発環境

- Godot: `4.6.3 stable Mono`
- Godot実行ファイル:
  - GUI: `D:\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64.exe`
  - Console: `D:\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64_console.exe`
- 使用言語: C#
- .NETターゲット: `net8.0`
- 対応プラットフォーム: Windows

## セッション開始時に必ず行うこと

1. この `AGENTS.md` を最後まで読む。
2. `Doc\00_HOME.md` を読む。
3. `Doc\04_Production\open_questions.md` を読む。
4. `Doc\06_Tasks` を確認し、承認済み・進行中・完了タスクを把握する。
5. 依頼に関連するゲーム仕様・技術仕様・設計判断記録を読む。
6. `git status -sb` と `git diff --stat` を実行する。
7. 既存の未コミット変更をユーザーの作業として保護する。
8. 関連するコードとテストを確認してから変更する。

## 現在のゲーム概要

- 標準的な8×8オセロ。
- CPUが黒の先手、人間が白の後手。
- 起動後、CPUが着手する前に難易度選択ダイアログを表示する。
- 難易度は初級・中級・上級。
  - 初級: 初心者評価で1手先を評価する。
  - 中級: 戦略評価で2手先を探索する。
  - 上級: 戦略評価で3手先を探索する。
- 完全なランダム着手は行わない。
- 同じ盤面と難易度では同じ手を選ぶ。
- 対局画面、難易度選択、リザルトだけを実装する。
- タイトル、モード選択、ポーズ、設定、セーブ、BGM、効果音は初期版の対象外。

詳細は必ずVaultを正本として確認すること。上記の要約とVaultが異なる場合はVaultを優先し、このファイルも更新する。

## 仕様の正本と優先順位

ゲーム仕様は `Doc` 以下のMarkdownを正本とする。

優先順位：

1. `Doc/06_Tasks` の承認済みタスク
2. `Doc/05_Decisions`
3. `Doc/03_Technical`
4. `Doc/02_Game_Design`
5. `Doc/01_Vision`
6. 既存コード

主な入口：

- `Doc/00_HOME.md`
- `Doc/02_Game_Design/game_rules.md`
- `Doc/02_Game_Design/cpu_ai.md`
- `Doc/02_Game_Design/ui_ux.md`
- `Doc/03_Technical/architecture.md`
- `Doc/03_Technical/testing_strategy.md`
- `Doc/04_Production/roadmap.md`
- `Doc/04_Production/open_questions.md`
- `Doc/05_Decisions/ADR-001_CPU難易度設計.md`

仕様とコードの矛盾を発見した場合、推測で製品仕様を変更しない。矛盾箇所、影響、選択肢を報告する。明らかな誤字や、承認済み仕様を別文書へ反映するだけの整合修正は行ってよい。

## 現在の実装構成

- `Main.tscn`: メインシーン
- `Scripts/Main.cs`: UI、入力、ゲーム進行、難易度選択、演出、リザルト
- `Scripts/BoardState.cs`: 盤面状態、合法手、反転、パス、終局、勝敗
- `Scripts/CpuPlayer.cs`: 難易度別評価、固定深度ミニマックス、α-β枝刈り
- `Scripts/RulesSelfTest.cs`: ヘッドレス自己テスト

## 実装原則

- ルール処理とGodot UIを分離する。
- `BoardState`をGodot APIへ依存させない。
- `CpuPlayer`をGodot APIや画面ノードへ依存させない。
- CPU探索は実対局の盤面を変更せず、複製した盤面を使用する。
- CPU思考でメインスレッドを停止させない。
- 初期版の対象外機能を無断で追加しない。
- 依頼されていないバランス変更や大規模リファクタリングを行わない。
- 外部素材・フォント・音源・依存ライブラリを追加する場合は、商用利用条件を確認する。
- 新しい仕様判断を行った場合は、関連仕様書と必要に応じて `Doc/05_Decisions` を更新する。
- 実装作業は `Doc/06_Tasks` に記録し、完了時に検証結果を残す。

## Gitと未コミット変更

- 作業開始時に必ずGit状態を確認する。
- 未コミット変更を勝手に破棄、上書き、リセットしない。
- `git reset --hard` や `git checkout --` を使用しない。
- コミット・プッシュはユーザーが依頼した場合に行う。
- コミット前に対象差分と生成物の混入を確認する。
- `.godot`、`bin`、`obj`、プレビュー画像などの生成物はコミットしない。

### 2026-06-18時点の引き継ぎ

- `TASK-002_CPU難易度選択`は実装・検証済み。
- 初級・中級・上級CPUと難易度選択UIが実装済み。
- 難易度仕様に合わせたVault全体の整合更新済み。
- これらは未コミットの可能性があるため、必ず現在の `git status` と差分を確認すること。
- この時点情報は将来古くなるため、Gitとタスク文書の現在状態を優先すること。

## 標準検証

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

## 作業完了時に行うこと

1. 関連するビルド・自己テスト・起動確認を実行する。
2. 受け入れ条件を一つずつ確認する。
3. 関連するVault文書を更新する。
4. `Doc/06_Tasks`の完了記録へ変更ファイル、検証結果、既知の問題を書く。
5. `git status -sb` と `git diff --check` を確認する。
6. 変更内容、検証結果、未解決事項をユーザーへ報告する。
