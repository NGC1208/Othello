# オセロプロジェクト Vault

このVaultは、オセロゲームの企画・仕様・技術設計・制作状況を管理する正本です。

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

## 最初に読む文書

- [[01_Vision/game_vision|ゲームビジョン]]
- [[01_Vision/design_pillars|デザイン原則]]
- [[02_Game_Design/core_loop|コアループ]]
- [[02_Game_Design/game_rules|ゲームルール]]
- [[02_Game_Design/cpu_ai|CPU AI仕様]]
- [[02_Game_Design/ui_ux|UI / UX仕様]]
- [[03_Technical/architecture|技術設計]]
- [[03_Technical/testing_strategy|テスト戦略]]
- [[04_Production/roadmap|ロードマップ]]
- [[04_Production/definition_of_done|完成条件]]
- [[04_Production/open_questions|未確定事項]]
- [[05_Decisions/README|Decision Records]]
- [[05_Decisions/Design/DDR-001_CPU難易度方針|DDR-001 CPU難易度方針]]
- [[05_Decisions/Architecture/ADR-001_CPU難易度実装方式|ADR-001 CPU難易度実装方式]]
- [[06_Tasks/TASK-002_CPU難易度選択|CPU難易度選択タスク]]

## 運用ルール

- 確定した仕様とアイデアを区別する。
- 同じ仕様を複数箇所に重複して書かない。
- 重要な企画判断は `05_Decisions/Design` のDDRへ記録する。
- DDRを実現する重要な技術判断は `05_Decisions/Architecture` のADRへ記録する。
- 開発は `DDR → 必要ならADR → Task → Code / Tests` の順で進める。
- 実装作業は `06_Tasks` のタスク単位で行う。
- 未確定事項には「未定」と記載する。
