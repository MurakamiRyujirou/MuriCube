# Docs/Application/Application_Overview.md

## 1. 概要

Application 層は、Domain 層のエンティティ（`Field`、`ActiveMino`、`Cube` など）を orchestrate し、
「ゲームとして動く」一連のユースケースを提供する。
Unity や Presentation 層との直接依存を持たず、入出力は純粋な C# の値として扱う。

## 2. 層の責務と境界

| 層 | 責務 | 依存先 |
|----|------|--------|
| **Domain** | ビジネスルール（回転・消去判定・衝突判定） | `Domain_Common` を基準。`Tetris` は `PivotPosition` 等で `Cube` 名前空間を一方向参照 |
| **Application** | ユースケースの実行・状態管理 | Domain のみ |
| **Presentation** | 入力受付・View への反映 | Application（GameState の購読） |

- Application 層は `UnityEngine` に依存しない純粋な C# とする。
- Presentation 層は `GameStateMachine.GameStateObservable` を R3 で購読し、View に反映する。
- Application 層は Presentation 層を直接参照しない。

### 2.1 回転操作と Presentation の適用順

回転は `RotateMinoUseCase` が `IsColliding` により**採択／却下**を決める。Presentation の `CubeUIController` は、この判定を **`CubeUIView` の回転アニメーションより前**に行い、却下時はビューを進めない。そうしないと見た目だけが進み、`GameState` と不整合になる（スクランブル再生も同一経路）。設計の詳細は `Docs/Presentation/Views/Gameplay/Gameplay/Presentation_Views_CubeUIController.md` §4.2・§8 を参照。

## 3. フォルダ構成

```
Assets/Scripts/Application/
├── GameState.cs                ← ゲーム全体のデータを保持する不変レコード
├── GameStateMachine.cs         ← フェーズの保持・遷移・通知を一元管理
├── ScramblingMove.cs           ← スクランブル1手分（Spawn 時に列挙）
├── MinoFactory.cs              ← MinoType → ActiveMino の生成
├── PhaseStates/
│   ├── IGamePhaseState.cs      ← フェーズ状態のインターフェース
│   ├── GamePhase.cs            ← フェーズ識別用列挙型
│   ├── SpawningState.cs
│   ├── ScramblingState.cs
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

補助型: `UseCases/DropType.cs`, `UseCases/MoveDirection.cs`。`IsExternalInit.cs` はソース生成用の属性。

対応するドキュメントは `Docs/Application/` 以下に配置する。

```
Docs/Application/
├── Application_Overview.md         ← 本ドキュメント
├── Application_GameState.md
├── Application_GamePhaseState.md   ← Stateパターン・GameStateMachineの設計
├── Application_GamePhaseState_Scrambling.md ← スクランブル演出と ScramblingMoves
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
| `GameStateMachine` | クラス | フェーズの保持・OnUpdate の実行・変化の通知・`ApplyGameState` |

呼び出しの流れは以下の通り。

```
Presentation層（例: GameLoopDebugRunner）
    └── GameStateMachine.OnUpdate(deltaTime)
            └── IGamePhaseState.Execute(gameState, deltaTime)
                    └── (nextState, nextGameState) を返す
                            └── ReactiveProperty が View に通知
```

回転確定やスクランブル完了など、**1フレームのフェーズ tic 以外**で `GameState` を差し替える場合は `GameStateMachine.ApplyGameState` を使う。

詳細は `Application_GamePhaseState.md` を参照。

## 5. GameState（データの単一管理）

ゲームのすべてのデータは `GameState` という不変レコードに集約する。
各ユースケースは現在の `GameState` を受け取り、新しい `GameState` を返す純粋関数として実装する。

```
GameState
├── Field              : 接地済みブロックの配置
├── ActiveMino         : 操作中のミノ（未生成は null。型は実装上 ActiveMino）
├── Score              : スコア
├── Level              : レベル
├── ClearedLineCount   : 累積消去ライン数
├── IsGameOver         : ゲームオーバーフラグ
└── ScramblingMoves    : スポーン直後のスクランブル手順（空で通常プレイ）
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

## 9. Presentation との結線（参照用）

Application 層は Presentation を参照しない。シーン側で `GameStateMachine` を生成し `OnUpdate` する典型例は次のとおり。

| コンポーネント（例） | パス | 役割 |
|---------------------|------|------|
| `GameLoopDebugRunner` | `Assets/Scripts/Presentation/GameLoopDebugRunner.cs` | デバッグ用エントリ。`GameStateMachine` 生成・購読・各 View / Detector の `Initialize`・`Update` で `OnUpdate` |
| `FieldUIView` | `Presentation/Views/Gameplay/FieldUIView.cs` | フィールド表示 |
| `CubeUIView` / `CubeUIController` | 同上 | キューブエリア表示・回転 UI・スクランブル再生 |
| `BlockUIView` | 同上 | ブロック表示 |
| `TetrisInputView` | 同上 | テトリス操作ボタン |
| `ScoreUIView` | 同上 | スコア表示 |
| `FieldBorderView` | 同上 | フィールド枠など |
| `KeyboardInputDetector` / `GamepadInputDetector` / `CubeInputDetector` | `Presentation/InputDetectors/` | 入力 |
| `CubeViewTestController` | `Presentation/Views/Gameplay/` | 開発・検証用（製品フロー外想定） |

レイアウト・カメラは `Docs/Presentation/UI_Layout.md`、スクランブルは `Application_GamePhaseState_Scrambling.md`。