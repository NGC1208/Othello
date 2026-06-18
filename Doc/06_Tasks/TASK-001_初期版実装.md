---
id: TASK-001
status: 完了
priority: 高
depends_on: []
related_docs:
  - 02_Game_Design/game_rules.md
  - 02_Game_Design/cpu_ai.md
  - 02_Game_Design/ui_ux.md
  - 03_Technical/architecture.md
---

# プレイヤー対CPUオセロ初期版

## 目的

仕様書に従い、起動直後から最後まで遊べるWindows向けオセロを実装する。

## 実装内容

- 標準的な8×8オセロのルール
- CPU黒、人間白
- α-β枝刈り付きミニマックスCPU
- 手番、石数、最後の着手位置
- パス、終局、リザルト、再対局、終了
- 0.2秒の石反転演出
- サイズ変更可能な1280×720基準ウィンドウ

## 検証

- `dotnet build`: 成功、警告0、エラー0
- ルール自己テスト: `SELF_TEST_OK`
- Godotヘッドレス起動: 成功
