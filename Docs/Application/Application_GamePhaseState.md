# Docs/Application/Application_GamePhaseState.md

## 1. 概要

ゲームのフェーズ管理を State パターンで実装する。
各フェーズを `IGamePhaseState` を実装したクラスとして定義し、
`GameStateMachine` がフェーズの保持・遷移・GameState の更新を一元管理する。

フェーズが持つのは**ロジックのみ**。ゲームデータは `GameState` が保持する（責務の分離）。

## 2. インターフェース定義

### IGamePhaseState


| 項目   | 値                                                           |
| ---- | ----------------------------------------------------------- |
| ソース  | `Assets/Scripts/Application/PhaseStates/IGamePhaseState.cs` |
| 名前空間 | `Application.PhaseStates`                                   |


```csharp
public interface IGamePhaseState
{
    // このフェーズの識別子（ログ・デバッグ用）
    GamePhase Phase { get; }

    // 1 ティック分の処理を実行する。
    // deltaTime: 前フレームからの経過時間（秒）。タイマー管理に使用する。
    // 戻り値: (次のフェーズState, 更新後のGameState)
    // フェーズ遷移しない場合は this を返す。
    (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState, float deltaTime);
}
```

### GamePhase（識別用列挙型）

- **配置**: `Assets/Scripts/Application/PhaseStates/GamePhase.cs`
- **名前空間**: `Application.PhaseStates`
- ロジックを持たず、識別・ログ目的にのみ使用する。

```csharp
public enum GamePhase
{
    Spawning,
    Falling,
    LockDown,
    Clearing,
    GameOver
}
```

## 3. 各フェーズの責務

### SpawningState

- `MinoFactory` で新しい `ActiveMino` を生成し、`GameState.ActiveMino` にセットする。
- 生成直後に `ActiveMino.IsColliding(field)` が `true` なら `IsGameOver = true` にして `GameOverState` へ遷移。
- 正常生成なら即座に `FallingState` へ遷移する。
- `deltaTime` は使用しない（即時遷移のため）。
- `System.Random` をコンストラクタで受け取り、`SpawnMinoUseCase.Execute` に渡す。

### FallingState

- 自然落下タイマーを管理し、落下間隔に達したら `DropMinoUseCase.Execute(gameState, DropType.Soft)` を呼ぶ。
- 落下後に `GameState` が変化しなかった（接地）場合は `LockDownState` へ遷移。
- 落下後に変化があった場合は `(this, newGameState)` を返す。
- 落下間隔は `Level` に応じて変化する。


| Level | 落下間隔      |
| ----- | --------- |
| 0     | 1.0秒      |
| 1     | 0.9秒      |
| 2     | 0.8秒      |
| …     | …         |
| 9以上   | 0.1秒（最小値） |


### LockDownState

- 猶予時間（0.5秒）を計測する。
- 猶予時間が切れたら `LockMinoUseCase.Execute` → `LineClearUseCase.Execute` を順に呼び `SpawningState` へ遷移。
- 猶予時間内に移動・回転があった場合はタイマーをリセットし `FallingState` へ戻る。
- `System.Random` を外部から受け取る（`SpawningState` に引き継ぐため）。

### ClearingState

- `System.Random` をコンストラクタで受け取り、即座に `new SpawningState(random)` へ遷移する。
- 将来アニメーション待機が必要になった場合に `deltaTime` タイマーを追加する。

### GameOverState

- 終端状態。`Execute` は何もせず `(this, gameState)` を返す。
- Presentation 層がこのフェーズを検知してゲームオーバー画面を表示する。

## 4. GameStateMachine

### 配置


| 項目   | 値                                                |
| ---- | ------------------------------------------------ |
| ソース  | `Assets/Scripts/Application/GameStateMachine.cs` |
| 名前空間 | `Application`                                    |
| 型    | `sealed class GameStateMachine`                  |


### 責務

- `IGamePhaseState` の現在インスタンスを保持する。
- `OnUpdate(float deltaTime)` を呼ばれたら現在フェーズの `Execute` を呼び、次の State と `GameState` を受け取って更新する。
- `ReactiveProperty<GameState>` を公開し、`GameState` の変化を Presentation 層に通知する。

### 公開API

```csharp
public sealed class GameStateMachine
{
    // 現在の GameState を購読可能な形で公開
    public ReadOnlyReactiveProperty<GameState> GameStateObservable { get; }

    // 現在のフェーズ識別子（デバッグ・UI表示用）
    public GamePhase CurrentPhase => _currentState.Phase;

    // 1ティック分の更新。Presentation 層の Update / FixedUpdate から呼ぶ。
    public void OnUpdate(float deltaTime);

    // ゲームを初期状態にリセットする。
    public void Reset();
}
```

### Presentation 層との連携

```
GameController.Update(Time.deltaTime)
    └── GameStateMachine.OnUpdate(deltaTime)
            └── IGamePhaseState.Execute(gameState, deltaTime)
                    └── 次のState + 新GameState を返す
                            └── ReactiveProperty が購読者（View）に通知
```

## 5. 設計指針

- **フェーズはロジックのみ**: データは `GameState` に置き、フェーズクラスはタイマー以外のフィールドを持たない。
- **タイマーの扱い**: `FallingState` と `LockDownState` のタイマーはフェーズクラス内のフィールドとして保持する。`Execute` の引数 `deltaTime` で更新する。
- **UnityEngine 非依存の維持**: `IGamePhaseState` および `GameStateMachine` は `UnityEngine` を参照しない。`deltaTime` は引数で受け取る形にし、依存を外部に押し出す。
- **拡張性**: 将来フェーズを追加する場合は `IGamePhaseState` を実装した新クラスを追加するだけでよい。

