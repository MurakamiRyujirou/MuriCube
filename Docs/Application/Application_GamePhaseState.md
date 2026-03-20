# Docs/Application/Application_GamePhaseState.md

## 1. 概要

ゲームのフェーズ管理を State パターンで実装する。
各フェーズを `IGamePhaseState` を実装したクラスとして定義し、
`GameStateMachine` がフェーズの保持・遷移・GameState の更新を一元管理する。

フェーズが持つのは**ロジックのみ**。ゲームデータは `GameState` が保持する（責務の分離）。

## 2. インターフェース定義

### IGamePhaseState

| 項目 | 値 |
|------|-----|
| ソース | `Assets/Scripts/Application/PhaseStates/IGamePhaseState.cs` |
| 名前空間 | `Application.PhaseStates` |

```csharp
public interface IGamePhaseState
{
    // このフェーズの識別子（ログ・デバッグ用）
    GamePhase Phase { get; }

    // 1 ティック分の処理を実行する。
    // 戻り値: (次のフェーズState, 更新後のGameState)
    // フェーズ遷移しない場合は this を返す。
    (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState);
}
```

**備考**: タイマー（自然落下・LockDown 猶予）が必要になる段階で、
引数に `float deltaTime` を追加することを検討する。現時点では保留。

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

### FallingState
- プレイヤー入力（移動・回転・ソフトドロップ）を受け付け、対応するユースケースを呼ぶ。
- 自然落下タイマーを管理し、1 段落下を試みる。落下不可（接地）なら `LockDownState` へ遷移。
- ハードドロップ入力で即着地し、`LockDownState` へ遷移。

### LockDownState
- 猶予時間（デフォルト 0.5 秒）を計測する。
- 猶予時間内に移動・回転があった場合はタイマーをリセットし `FallingState` へ戻る。
- 猶予時間が切れたら `LockMinoUseCase` を実行して `ClearingState` へ遷移。

### ClearingState
- `LineClearUseCase` を実行し、消去ライン数に応じてスコア・レベルを更新する。
- 消去処理完了後、`SpawningState` へ遷移する。
- 消去対象ラインがゼロの場合も `SpawningState` へ遷移する（スキップ不可）。

### GameOverState
- 終端状態。`Execute` は何もせず `(this, gameState)` を返す。
- Presentation 層がこのフェーズを検知してゲームオーバー画面を表示する。

## 4. GameStateMachine

### 配置

| 項目 | 値 |
|------|-----|
| ソース | `Assets/Scripts/Application/GameStateMachine.cs` |
| 名前空間 | `Application` |
| 型 | `sealed class GameStateMachine` |

### 責務

- `IGamePhaseState` の現在インスタンスを保持する。
- `OnUpdate()` を呼ばれたら現在フェーズの `Execute` を呼び、次の State と `GameState` を受け取って更新する。
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
    public void OnUpdate();

    // ゲームを初期状態にリセットする。
    public void Reset();
}
```

### Presentation 層との連携

将来の `GameController`（MonoBehaviour）は以下のような薄い層になる。

```
GameController.Update()
    └── GameStateMachine.OnUpdate()
            └── IGamePhaseState.Execute(gameState)
                    └── 次のState + 新GameState を返す
                            └── ReactiveProperty が購読者（View）に通知
```

`GameController` は入力を受け取って `OnUpdate()` を呼ぶだけに専念し、
ゲームロジックは `GameStateMachine` と各 `IGamePhaseState` が担う。

## 5. 設計指針

- **フェーズはロジックのみ**: データは `GameState` に置き、フェーズクラスはフィールドを持たない（タイマーを除く）。
- **タイマーの扱い**: `LockDownState` と `FallingState` のタイマーはフェーズクラス内のフィールドとして保持してよい。タイマーが必要になった段階で `Execute` の引数に `float deltaTime` を追加することを検討する。
- **UnityEngine 非依存の維持**: `IGamePhaseState` および `GameStateMachine` は `UnityEngine` を参照しない。`deltaTime` が必要な場合は引数で受け取る形にし、依存を外部に押し出す。
- **拡張性**: 将来フェーズを追加する場合は `IGamePhaseState` を実装した新クラスを追加するだけでよい。既存クラスの変更は不要。