# Docs/Application/Application_Overview.md

## 1. 概要

Application 層は、Domain 層のエンティティ（`Field`、`ActiveMino`、`Cube` など）を orchestrate し、  
「ゲームとして動く」一連のユースケースを提供する。  
Unity や Presentation 層との直接依存を持たず、入出力は純粋な C# の値として扱う。

## 2. 層の責務と境界

| 層 | 責務 | 依存先 |
|----|------|--------|
| **Domain** | ビジネスルール（回転・消去判定・衝突判定） | なし |
| **Application** | ユースケースの実行・状態管理 | Domain のみ |
| **Presentation** | 入力受付・View への反映 | Application（GameState の購読） |

- Application 層は `UnityEngine` に依存しない純粋な C# とする。
- Presentation 層は Application 層の `GameState` を R3 の Observable で購読し、View に反映する。
- Application 層は Presentation 層を直接参照しない。

## 3. フォルダ構成

```
Assets/Scripts/Application/
├── GameState.cs           ← ゲーム全体の状態を保持する不変レコード
├── MinoFactory.cs         ← MinoType → ActiveMino の生成
└── UseCases/
    ├── SpawnMinoUseCase.cs   ← ミノ生成・初期配置
    ├── MoveMinoUseCase.cs    ← 左右移動
    ├── RotateMinoUseCase.cs  ← 回転
    ├── DropMinoUseCase.cs    ← 落下（自然・ソフト・ハード）
    ├── LockMinoUseCase.cs    ← 接地固定
    └── LineClearUseCase.cs   ← ライン消去
```

対応するドキュメントは `Docs/Application/` 以下に配置する。

```
Docs/Application/
├── Application_Overview.md       ← 本ドキュメント
├── Application_GameState.md
├── Application_MinoFactory.md
└── UseCases/
    ├── UseCase_SpawnMino.md
    ├── UseCase_MoveMino.md
    ├── UseCase_RotateMino.md
    ├── UseCase_DropMino.md
    ├── UseCase_LockMino.md
    └── UseCase_LineClear.md
```

## 4. GameState（状態の単一管理）

ゲームのすべての状態は `GameState` という不変レコードに集約する。  
各ユースケースは現在の `GameState` を受け取り、新しい `GameState` を返す純粋関数として実装する。

```
GameState
├── Field           : 接地済みブロックの配置
├── ActiveMino?     : 現在操作中のミノ（null = 未生成）
├── Score           : スコア
├── Level           : レベル
├── Phase           : ゲームフェーズ（列挙型）
└── IsGameOver      : ゲームオーバーフラグ
```

詳細は `Application_GameState.md` を参照。

## 5. ユースケースの設計方針

- **純粋関数**: 各ユースケースは `Execute(GameState) → GameState` の形を基本とする。
- **副作用なし**: ログ・サウンド・アニメーション等は Presentation 層が `GameState` の変化を購読して行う。
- **衝突検証は Domain に委譲**: `ActiveMino.IsColliding(Field)` を呼ぶだけで判定する。
- **ゲームオーバー判定は SpawnMino 時**: 新しいミノを生成した直後に衝突していれば `IsGameOver = true`。

## 6. ゲームフェーズ（Phase）

```
Spawning  → ミノを生成中
Falling   → ミノが落下・操作受付中
LockDown  → 接地後のロック猶予中
Clearing  → ライン消去アニメーション中
GameOver  → ゲームオーバー
```

フェーズ遷移のオーケストレーションは Presentation 層の Controller が担う。  
Application 層は「このユースケースを実行した結果の GameState」を返すにとどまる。

## 7. 名前空間

```
Application
Application.UseCases
```

## 8. 外部依存ライブラリ

Application 層が使用してよいライブラリ（`ProjectContext.md` 準拠）:

- **R3**: `GameState` の変更通知に使用（`ReactiveProperty<GameState>`）
- **UniTask**: 非同期ユースケース（ハードドロップ待機など）が必要な場合に限定使用
- **UnityEngine**: **使用禁止**（純粋 C# を維持する）