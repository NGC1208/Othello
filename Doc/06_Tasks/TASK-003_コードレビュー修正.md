---
id: TASK-003
status: 完了
priority: 高
depends_on:
  - TASK-002
related_docs:
  - 02_Game_Design/cpu_ai.md
  - 02_Game_Design/ui_ux.md
  - 03_Technical/architecture.md
  - 03_Technical/testing_strategy.md
---

# コードレビュー指摘の修正

## 目的

現行コードレビューで発見したCPU再現性、パス通知、CPU例外処理、複数方向反転テストの問題を修正する。

## 実装内容

- CPU探索を時間制限付き反復深化から難易度別の固定深度探索へ変更した。
- パスメッセージ表示中は入力と次のCPU思考を停止し、約1秒後に手番を移すよう変更した。
- CPU思考タスクが失敗・キャンセルされた場合、現在盤面の先頭合法手へ復旧する処理を追加した。
- 上下左右4方向を1着手で同時反転する固定盤面テストを追加した。
- 任意の固定盤面を生成するテスト専用の内部ファクトリを`BoardState`へ追加した。

## 受け入れ条件

- [x] 同一盤面・同一難易度で探索深度が実行時間に左右されない。
- [x] CPUパス通知中に人間が盤面を操作できない。
- [x] CPU思考例外時にゲームを停止せず合法手へ復旧できる。
- [x] 複数方向反転テストが反転枚数と各方向の石を検証する。
- [x] ビルドが警告0・エラー0で成功する。
- [x] 自己テストが成功する。

## 完了記録

- 変更ファイル: `Scripts/BoardState.cs`、`Scripts/CpuPlayer.cs`、`Scripts/Main.cs`、`Scripts/RulesSelfTest.cs`
- 検証結果: ビルド成功、`SELF_TEST_OK`
- 既知の問題: パス遷移とCPU例外復旧の自動UIテストは今後追加する。
