# Docs/Application/Application_GamePhaseState_Scrambling.md

## 1. 概要

スクランブル演出フェーズの設計。ミノスポーン時に「整列状態で表示 → 高速スクランブル回転アニメーション → 落下開始」という演出を実現する。

Application 層と Presentation 層の責務を明確に分離するため、スクランブル手順を `GameState.ScramblingMoves` に持たせ、Presentation 層が購読して演出を再生する。

## 2. 変更・追加するクラス

| クラス | 変更種別 | 内容 |
|--------|---------|------|
| `ScramblingMove` | 新規 | 1手分の `Domain.Cube.Enums.CubeOperation` を表す readonly struct |
| `GameState` | 修正 | `ScramblingMoves` プロパティを追加 |
| `SpawnMinoUseCase` | 修正 | スクランブル手順を `GameState.ScramblingMoves` に記録する |
| `SpawningState` | 修正 | スポーン後に `ScramblingState` へ遷移する |
| `ScramblingState` | 新規 | スクランブル完了を待機し `FallingState` へ遷移する |
| `GamePhase` | 修正 | `Scrambling` を追加 |
| `CubeUIController` | 修正 | `ScramblingMoves` を購読し順番に `ExecuteRotateAsync` を呼ぶ |

## 3. ScramblingMove

`SpawnMinoUseCase` が試行した各 `Cube.Rotate` と同じ `CubeOperation` を列挙する。Presentation はこれを `CubeUIController.ExecuteRotateAsync(CubeOperation, ...)` に渡す。

```csharp
// Assets/Scripts/Application/ScramblingMove.cs
using Domain.Cube.Enums;

namespace Application
{
    public readonly struct ScramblingMove
    {
        public CubeOperation Operation { get; }

        public ScramblingMove(CubeOperation operation)
        {
            Operation = operation;
        }
    }
}
```

## 4. GameState の変更

`ScramblingMoves` プロパティを追加する。

```csharp
public sealed record GameState
{
    // 既存プロパティ...
    public IReadOnlyList<ScramblingMove> ScramblingMoves { get; init; }
        = Array.Empty<ScramblingMove>();
}
```

- スクランブル中は `ScramblingMoves` に回転リストが入る
- スクランブル完了後（`CubeUIController` が再生完了）は `ApplyGameState` で空リストに戻す
- `GameState.Initial` では空リスト

## 5. SpawnMinoUseCase の変更

現在のランダム回転ループを以下のように変更する。

```
1. 整列状態のミノを生成する（MinoFactory.Create のまま）
2. CanRotate チェック付きでランダム回転を最大 20 回試みる
3. 各回転を ScramblingMove のリストに記録する
4. 回転後の BlockGroup を持つミノをスポーン位置に配置する
5. gameState with { ActiveMino = mino, ScramblingMoves = moves } を返す
```

スポーン時の `ActiveMino.BlockGroup` は**整列状態のまま**にする。スクランブル後の形状ではなく、整列状態で `CubeUIView` に `Build` させる。スクランブルは `CubeUIController` がアニメーションで再生する。

## 6. SpawningState の変更

```csharp
public (IGamePhaseState, GameState) Execute(GameState gameState, float deltaTime)
{
    var newGameState = SpawnMinoUseCase.Execute(gameState, _random);
    if (newGameState.IsGameOver)
        return (new GameOverState(), newGameState);
    return (new ScramblingState(_random), newGameState);  // FallingState → ScramblingState
}
```

## 7. ScramblingState

```csharp
// Assets/Scripts/Application/PhaseStates/ScramblingState.cs
public sealed class ScramblingState : IGamePhaseState
{
    private readonly Random _random;

    public ScramblingState(Random random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public GamePhase Phase => GamePhase.Scrambling;

    public (IGamePhaseState, GameState) Execute(GameState gameState, float deltaTime)
    {
        // ScramblingMoves が空になったら FallingState へ遷移
        // CubeUIController が演出完了後に ApplyGameState で空リストにする
        if (gameState.ScramblingMoves.Count == 0)
            return (new FallingState(_random), gameState);

        // まだ演出中 → このフェーズに留まる
        return (this, gameState);
    }
}
```

## 8. CubeUIController の変更

`Initialize` 内の購読処理を以下のように拡張する。

```
GameState が変化したとき:
    1. ActiveMino が切り替わった → CubeUIView.Build（整列状態で表示）
    2. ScramblingMoves が空でない かつ 前回は空だった → PlayScramblingAsync を開始
```

### PlayScramblingAsync

```
1. ScramblingMoves を順番にイテレートする
2. 各 `ScramblingMove.Operation` を `ExecuteRotateAsync(operation, durationOverride)` で再生する（スクランブル用は 0.15 秒/回）
3. 貯めた手順をすべて処理したあと、gameState with { ScramblingMoves = [] } を ApplyGameState する
```

**回転1回あたりの適用順**は `CubeUIController.ExecuteRotateCoreAsync` に従う。**`RotateMinoUseCase`（ドメイン採否）をアニメより先**に実行し、フィールド衝突で却下された手順では**その回はアニメ・Refresh を行わない**。意図と手順は `Docs/Presentation/Views/Gameplay/Gameplay/Presentation_Views_CubeUIController.md` §4.2・§8、`ISSUES.md` [Task 047] を参照。

## 9. スクランブル中の入力無効

`ScramblingMoves.Count > 0` のあいだはゲームプレイ入力を受け付けない。実装では次が同条件を参照する。

- `KeyboardInputDetector.TryConsumeGameplayInput`
- `GamepadInputDetector`（同等のガード）
- `CubeInputDetector`（演出中は操作不可）
- `TetrisInputView`（移動・落下ボタン）

例（キーボード側）:

```csharp
if (_stateMachine.GameStateObservable.CurrentValue.ScramblingMoves.Count > 0)
    return false;
```

## 10. アニメーション時間

- スクランブル1回転あたり: **0.15秒**
- `CubeUIController` の `_scramblingRotateDuration` として `[SerializeField]` で持つ（デフォルト: 0.15f）