# AI開発ルール

## 仕様の正本

ゲーム仕様は `Doc` 以下のMarkdownを正本とする。

優先順位：

1. `Doc/06_Tasks` の承認済みタスク
2. `Doc/05_Decisions`
3. `Doc/03_Technical`
4. `Doc/02_Game_Design`
5. `Doc/01_Vision`
6. 既存コード

矛盾を見つけた場合は推測で製品仕様を変更せず、問題を報告する。

## 実装原則

- ルール処理とGodot UIを分離する。
- CPU AIをGodotの画面ノードへ依存させない。
- 初期版の対象外機能を無断で追加しない。
- 外部素材や依存ライブラリを追加する場合は、商用利用条件を確認する。

## 変更後の確認

1. `dotnet build Othello.csproj`
2. Godotで `--headless --path <project> -- --self-test`
3. メインシーンのヘッドレス起動
4. 関連仕様と受け入れ条件の確認
