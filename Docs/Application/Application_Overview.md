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
- Presentation 層は `GameStateMachine.GameStateObservable` を R3 で購読し、View に反映する。
- Application 層は Presentation 層を直接参照しない。

## 3. フォルダ構成

```
Assets/Scripts/Application/
├── GameState.cs                ← ゲーム全体のデータを保持する不変レコード
├── GameStateMachine.cs         ← フェーズの保持・遷移・通知を一元管理
├── MinoFactory.cs              ← MinoType → ActiveMino の生成
├── PhaseStates/
│   ├── IGamePhaseState.cs      ← フェーズ状態のインターフェース
│   ├── GamePhase.cs            ← フェーズ識別用列挙型
│   ├── SpawningState.cs
│   ├── FallingState.cs
│   ├── LockDownState.cs
│   ├── ClearingState.cs
│   └── GameOverState.cs
└── UseCases/
    ├── SpawnMinoUseCase.cs
    ├── MoveMinoUseCase.cs
    ├── RotateMinoUseCase.cs
    ├── DropMinoUseCase.cs
    ├── LockMinoUseCase.cs
    └── LineClearUseCase.cs
```

対応するドキュメントは `Docs/Application/` 以下に配置する。

```
Docs/Application/
├── Application_Overview.md         ← 本ドキュメント
├── Application_GameState.md
├── Application_GamePhaseState.md   ← Stateパターン・GameStateMachineの設計
├── Application_MinoFactory.md
└── UseCases/
    ├── UseCase_SpawnMino.md
    ├── UseCase_MoveMino.md
    ├── UseCase_RotateMino.md
    ├── UseCase_DropMino.md
    ├── UseCase_LockMino.md
    └── UseCase_LineClear.md
```

## 4. 状態管理の設計（State パターン + 不変レコード）

ゲームの状態管理は以下の 3 つに分離する。

| クラス | 種別 | 役割 |
|--------|------|------|
| `GameState` | 不変レコード | ゲームデータの保持（Field・Score など） |
| `IGamePhaseState` | インターフェース | フェーズごとのロジックと遷移 |
| `GameStateMachine` | クラス | フェーズの保持・OnUpdate の実行・変化の通知 |

呼び出しの流れは以下の通り。

```
Presentation層（将来の GameController）
    └── GameStateMachine.OnUpdate(deltaTime)
            └── IGamePhaseState.Execute(gameState, deltaTime)
                    └── (nextState, nextGameState) を返す
                            └── ReactiveProperty が View に通知
```

詳細は `Application_GamePhaseState.md` を参照。

## 5. GameState（データの単一管理）

ゲームのすべてのデータは `GameState` という不変レコードに集約する。
各ユースケースは現在の `GameState` を受け取り、新しい `GameState` を返す純粋関数として実装する。

```
GameState
├── Field           : 接地済みブロックの配置
├── ActiveMino?     : 現在操作中のミノ（null = 未生成）
├── Score           : スコア
├── Level           : レベル
├── ClearedLineCount: 累積消去ライン数
└── IsGameOver      : ゲームオーバーフラグ
```

詳細は `Application_GameState.md` を参照。

## 6. ユースケースの設計方針

- **純粋関数**: 各ユースケースは `Execute(GameState, ...) → GameState` の形を基本とする。
- **副作用なし**: ログ・サウンド・アニメーション等は Presentation 層が `GameState` の変化を購読して行う。
- **衝突検証は Domain に委譲**: `ActiveMino.IsColliding(Field)` を呼ぶだけで判定する。
- **ゲームオーバー判定は SpawnMino 時**: 新しいミノを生成した直後に衝突していれば `IsGameOver = true`。

## 7. 名前空間

```
Application
Application.PhaseStates
Application.UseCases
```

## 8. 外部依存ライブラリ

Application 層が使用してよいライブラリ（`ProjectContext.md` 準拠）:

- **R3**: `GameStateMachine` の `ReactiveProperty<GameState>` に使用
- **UniTask**: 非同期ユースケースが必要な場合に限定使用
- **UnityEngine**: **使用禁止**（純粋 C# を維持する）