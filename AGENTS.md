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
5. 依頼に関連するDDR、ゲーム仕様、ADR、技術仕様を読む。
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

企画意図から実装へ、次の順序で具体化する：

1. `Doc/01_Vision`: VisionとDesign Pillars
2. `Doc/05_Decisions/Design`: 承認済みDDR
3. `Doc/02_Game_Design`: ゲームデザイン仕様
4. `Doc/05_Decisions/Architecture`: 必要な承認済みADR
5. `Doc/03_Technical`: 技術仕様
6. `Doc/06_Tasks`: 承認済みTask
7. Code / Tests

## DDR / Game Design / ADR 作成ルール

文書を作成・更新するときは、次の責務分離を厳守する。

開発の基本フロー：

```text
DDR → Game Design → 必要ならADR → Task → Code / Tests
```

ADRはGame Designを変更してはならない。TaskはDDR、Game Design、ADRの内容を独断で変更してはならない。

下流で上流の決定を実現できない場合は、推測で妥協せず上流へ判断を戻す。

### DDR（Design Decision Record）

DDRはゲームデザインの目的と方針を記録する。

主に記載する内容：

- なぜその体験が必要なのか
- プレイヤーへどのような価値を提供するのか
- どのような問題を解決するのか
- 検討したデザイン方針
- 採用したデザイン方針
- 採用しなかった方針と理由
- 期待するプレイヤー体験への影響

DDRは「Why」と「Design Policy」を記録する。

DDRには、アルゴリズム、クラス構成、データ構造、ライブラリなどの実装方式を書かない。

個別の数値調整や小さな仕様変更だけではDDRを作成しない。デザインの目的または方針を変更するときに作成・更新する。

### Game Design

Game Design文書は、プレイヤーから見えるゲーム仕様の正本とする。

主に記載する内容：

- プレイヤーが体験する振る舞い
- ゲームルール
- ゲームモード
- バランス
- 数値パラメータ
- 状態遷移
- CPUの外部的な挙動
- UI / UX仕様
- 受け入れ条件

Game Designは「What」を記録する。

探索深度、評価式、評価の重みなど、プレイヤー体験やゲームバランスを規定する値はGame Designへ記載する。

Game Designには、クラス構成、具体的なデータ構造、スレッド方式など、プレイヤーから見えない実装詳細を書かない。

### ADR（Architecture Decision Record）

ADRは、Game Designを実現するための重要な技術判断を記録する。

主に記載する内容：

- 技術的課題
- 技術上の要件と制約
- 検討した実装方式
- 採用した実装方式
- 採用理由
- 技術的なトレードオフ
- 保守性、性能、互換性への影響
- 将来の変更へ与える影響

ADRは「How」を記録する。

ADRにはゲーム仕様の正本を記載しない。DDRやGame Designの内容を必要以上に複製せず、関連文書へのリンクを記載する。

ADRでDDRまたはGame Designの意図を変更してはならない。技術的に実現できない場合は、独断で仕様を変更せず企画判断へ戻す。

すべてのDDRにADRが必要なわけではない。次のいずれかに該当する場合にADRを作成する。

- 複数の有力な実装方式がある
- 複数機能へ影響する
- 後から変更するコストが高い
- 性能、互換性、依存関係へ影響する
- 採用理由を将来確認する必要がある

### Task

Taskは、承認済みのDDR、Game Design、ADRを、今回実装する作業範囲へ具体化する。

Taskには次を記載する。

- 目的
- 作業範囲
- 作業対象外
- 関連するDDR、Game Design、ADR
- 受け入れ条件
- テスト計画
- 完了記録

Taskは上流文書の決定を独断で変更しない。

### 判断に迷った場合

変更内容を次の基準で分類する。

- デザインの目的・方針が変わる → DDR
- プレイヤーから見える仕様・数値が変わる → Game Design
- プレイヤーから見えない実装方式だけが変わる → ADR
- 承認済み内容を実装単位へ分解する → Task
- Taskと受け入れ条件を実現する → Code / Tests

複数に該当する場合は上流から順に更新する。

```text
DDR → Game Design → ADR → Task → Code / Tests
```

主な入口：

- `Doc/00_HOME.md`
- `Doc/02_Game_Design/game_rules.md`
- `Doc/02_Game_Design/cpu_ai.md`
- `Doc/02_Game_Design/ui_ux.md`
- `Doc/03_Technical/architecture.md`
- `Doc/03_Technical/testing_strategy.md`
- `Doc/04_Production/roadmap.md`
- `Doc/04_Production/open_questions.md`
- `Doc/05_Decisions/README.md`
- `Doc/05_Decisions/Design/DDR-001_CPU難易度方針.md`
- `Doc/05_Decisions/Architecture/ADR-001_CPU難易度実装方式.md`

DDR、ADR、仕様、Task、コードの矛盾を発見した場合、推測で製品仕様を変更しない。矛盾箇所、影響、選択肢を報告する。明らかな誤字や、承認済み判断を別文書へ反映するだけの整合修正は行ってよい。

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
- 新しい企画判断を行った場合はDDRを作成または更新する。
- DDRの実現に重要な技術判断が必要な場合はADRを作成または更新する。
- 小さく自明な実装で技術的トレードオフがない場合、ADRは省略してよい。
- 実装作業は `Doc/06_Tasks` に記録し、完了時に検証結果を残す。

## Gitと未コミット変更

- 作業開始時に必ずGit状態を確認する。
- 未コミット変更を勝手に破棄、上書き、リセットしない。
- `git reset --hard` や `git checkout --` を使用しない。
- コミット・プッシュはユーザーが依頼した場合に行う。
- コミット前に対象差分と生成物の混入を確認する。
- `.godot`、`bin`、`obj`、プレビュー画像などの生成物はコミットしない。

### 2026-06-19時点の引き継ぎ

- `TASK-002_CPU難易度選択`は実装・検証済み。
- `TASK-003_コードレビュー修正`は実装・検証済み。
- `TASK-004_AI評価定数化とエラー表示`は実装・検証済み。
- 初級・中級・上級CPUと難易度選択UIが実装済み。
- Decision RecordsをDDRとADRへ分離済み。
- CPU難易度の企画判断は`DDR-001`、技術判断は`ADR-001`を正本とする。
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
