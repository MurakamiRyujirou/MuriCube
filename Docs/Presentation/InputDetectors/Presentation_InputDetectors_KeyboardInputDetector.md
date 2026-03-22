# Docs/Presentation/InputDetectors/Presentation_InputDetectors_KeyboardInputDetector.md

## 1. 概要

`KeyboardInputDetector` はキーボード入力を検知し、回転・移動・落下操作を各ユースケースに橋渡しする `MonoBehaviour`。
ゲームロジックを持たず、入力を検知して対応するユースケースを呼ぶことに専念する。

## 2. 配置

| 項目 | 値 |
|------|-----|
| パス | `Assets/Scripts/Presentation/InputDetectors/KeyboardInputDetector.cs` |
| 名前空間 | `Presentation.InputDetectors` |
| 基底クラス | `MonoBehaviour` |

## 3. 依存関係（SerializedFields）

| フィールド名 | 型 | 説明 |
|-------------|-----|------|
| `_cubeUIController` | `CubeUIController` | 回転操作の委譲先 |
| `_stateMachine` | `GameStateMachine` | 移動・落下後の GameState 反映先 |

## 4. キー割り当て

### 4.1 回転操作（ルービックキューブ）

`UI_Layout.md` §6.2 に基づく。

| キー | 操作 | RotateAxis | CubeTurn |
|------|------|-----------|---------|
| R | R 回転 | X | Clockwise |
| Shift + R | R' 回転 | X | CounterClockwise |
| U | U 回転 | Y | Clockwise |
| Shift + U | U' 回転 | Y | CounterClockwise |
| F | F 回転 | Z | Clockwise |
| Shift + F | F' 回転 | Z | CounterClockwise |
| L | L 回転 | X | CounterClockwise |
| Shift + L | L' 回転 | X | Clockwise |
| D | D 回転 | Y | CounterClockwise |
| Shift + D | D' 回転 | Y | Clockwise |
| B | B 回転 | Z | CounterClockwise |
| Shift + B | B' 回転 | Z | Clockwise |

回転操作は `CubeUIController.ExecuteRotateAsync(axis, turn).Forget()` で呼ぶ。

### 4.2 テトリス操作（移動・落下）

| キー | 操作 | 処理 |
|------|------|------|
| ← | 左移動 | `MoveMinoUseCase.Execute(gameState, MoveDirection.Left)` → `ApplyGameState` |
| → | 右移動 | `MoveMinoUseCase.Execute(gameState, MoveDirection.Right)` → `ApplyGameState` |
| ↓ | ソフトドロップ | `DropMinoUseCase.Execute(gameState, DropType.Soft)` → `ApplyGameState` |
| Space | ハードドロップ | `DropMinoUseCase.Execute(gameState, DropType.Hard)` → `ApplyGameState` |

テトリス操作は `_stateMachine.GameStateObservable.CurrentValue` から現在の `GameState` を取得し、ユースケースを呼んだ結果を `_stateMachine.ApplyGameState` で反映する。

## 5. Update での処理

`Update()` 内で `Input.GetKeyDown` を使用する。
Shift の判定は `Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)` で行う。

```csharp
private void Update()
{
    var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    // 回転操作
    if (Input.GetKeyDown(KeyCode.R)) Rotate(RotateAxis.X, shift ? CubeTurn.CounterClockwise : CubeTurn.Clockwise);
    if (Input.GetKeyDown(KeyCode.U)) Rotate(RotateAxis.Y, shift ? CubeTurn.CounterClockwise : CubeTurn.Clockwise);
    if (Input.GetKeyDown(KeyCode.F)) Rotate(RotateAxis.Z, shift ? CubeTurn.CounterClockwise : CubeTurn.Clockwise);
    if (Input.GetKeyDown(KeyCode.L)) Rotate(RotateAxis.X, shift ? CubeTurn.Clockwise : CubeTurn.CounterClockwise);
    if (Input.GetKeyDown(KeyCode.D)) Rotate(RotateAxis.Y, shift ? CubeTurn.Clockwise : CubeTurn.CounterClockwise);
    if (Input.GetKeyDown(KeyCode.B)) Rotate(RotateAxis.Z, shift ? CubeTurn.Clockwise : CubeTurn.CounterClockwise);

    // テトリス操作
    if (Input.GetKeyDown(KeyCode.LeftArrow))  Move(MoveDirection.Left);
    if (Input.GetKeyDown(KeyCode.RightArrow)) Move(MoveDirection.Right);
    if (Input.GetKeyDown(KeyCode.DownArrow))  Drop(DropType.Soft);
    if (Input.GetKeyDown(KeyCode.Space))      Drop(DropType.Hard);
}
```

## 6. 設計指針

- **ゲームロジックを持たない**: 入力を検知して対応するユースケースを呼ぶだけ。判定ロジックはユースケース側が担う。
- **IsGameOver チェック**: `_stateMachine.GameStateObservable.CurrentValue.IsGameOver` が `true` の場合は入力を無視する。
- **UniTask**: 回転操作は `async UniTaskVoid` の `ExecuteRotateAsync` を呼ぶため `.Forget()` を使用する。