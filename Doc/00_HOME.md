# オセロプロジェクト Vault

このVaultは、オセロゲームの企画・仕様・技術設計・制作状況を管理する正本です。

## プロジェクトの場所

- プロジェクトルート: `D:\GodotProjects\Othello`
- 仕様Vault: `D:\GodotProjects\Othello\Doc`
- Godotプロジェクト: `D:\GodotProjects\Othello\project.godot`
- C#プロジェクト: `D:\GodotProjects\Othello\Othello.csproj`
- ソースコード: `D:\GodotProjects\Othello\Scripts`
- GitHub: `https://github.com/NGC1208/Othello`
- 標準ブランチ: `main`

## プロジェクト状況

- フェーズ: 初期版実装済み・検証中
- ゲームエンジン: Godot 4.6.3 stable Mono
- 対応プラットフォーム: Windows
- リリース形態: 未定

## 現在の実装範囲

- プレイヤー対CPU
- 起動時の難易度選択（初級・中級・上級）
- 標準的な8×8オセロルール
- パス、終局、勝敗、再対局
- マウス操作
- BGM・効果音・セーブ機能なし

## 現在の実装構成

- `Main.tscn`: メインシーン
- `Scripts/Main.cs`: UI、入力、ゲーム進行、難易度選択、演出、リザルト
- `Scripts/BoardState.cs`: 盤面状態、合法手、反転、パス、終局、勝敗
- `Scripts/CpuPlayer.cs`: 難易度別評価、固定深度ミニマックス、α-β枝刈り
- `Scripts/RulesSelfTest.cs`: ヘッドレス自己テスト

## 最初に読む文書

- [[01_Vision/game_vision|ゲームビジョン]]
- [[01_Vision/design_pillars|デザイン原則]]
- [[02_Game_Design/core_loop|コアループ]]
- [[02_Game_Design/game_rules|ゲームルール]]
- [[02_Game_Design/cpu_ai|CPU AI仕様]]
- [[02_Game_Design/ui_ux|UI / UX仕様]]
- [[03_Technical/architecture|技術設計]]
- [[03_Technical/development_environment|開発環境]]
- [[03_Technical/testing_strategy|テスト戦略]]
- [[04_Production/roadmap|ロードマップ]]
- [[04_Production/definition_of_done|完成条件]]
- [[04_Production/open_questions|未確定事項]]
- [[05_Decisions/README|Decision Records]]
- [[05_Decisions/Design/DDR-001_CPU難易度方針|DDR-001 CPU難易度方針]]
- [[05_Decisions/Architecture/ADR-001_CPU難易度実装方式|ADR-001 CPU難易度実装方式]]
- [[06_Tasks/TASK-002_CPU難易度選択|CPU難易度選択タスク]]

## 現在のDecisionとTask

- Design Decision: [[05_Decisions/Design/DDR-001_CPU難易度方針|DDR-001 CPU難易度方針]]
- Architecture Decision: [[05_Decisions/Architecture/ADR-001_CPU難易度実装方式|ADR-001 CPU難易度実装方式]]
- 完了Task:
  - [[06_Tasks/TASK-001_初期版実装|TASK-001 初期版実装]]
  - [[06_Tasks/TASK-002_CPU難易度選択|TASK-002 CPU難易度選択]]
  - [[06_Tasks/TASK-003_コードレビュー修正|TASK-003 コードレビュー修正]]
  - [[06_Tasks/TASK-004_AI評価定数化とエラー表示|TASK-004 AI評価定数化とエラー表示]]
- 未確定事項: [[04_Production/open_questions|未確定事項]]

## 運用ルール

- 確定した仕様とアイデアを区別する。
- 同じ仕様を複数箇所に重複して書かない。
- 重要な企画判断は `05_Decisions/Design` のDDRへ記録する。
- DDRを実現する重要な技術判断は `05_Decisions/Architecture` のADRへ記録する。
- 開発は `DDR → 必要ならADR → Task → Code / Tests` の順で進める。
- 実装作業は `06_Tasks` のタスク単位で行う。
- 未確定事項には「未定」と記載する。

## Gitと生成物

- `.godot`、`bin`、`obj`、プレビュー画像などの生成物はコミットしない。
- `Doc/.obsidian/graph.json`や`workspace.json`など、個人のObsidian表示状態は機能変更へ含めない。
- 作業開始時に`git status -sb`を実行し、既存の未コミット変更を保護する。

## 現在の引き継ぎ

- 初級・中級・上級CPUと難易度選択UIは実装済み。
- CPU難易度の企画判断は`DDR-001`、技術判断は`ADR-001`を正本とする。
- `TASK-001`から`TASK-004`まで実装・検証済み。
- 現在状態はGitとTask文書を優先し、この節が古くなった場合は更新する。
