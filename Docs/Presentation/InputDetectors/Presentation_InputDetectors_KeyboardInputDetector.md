# Docs/Presentation/InputDetectors/Presentation_InputDetectors_KeyboardInputDetector.md

## 1. 概要

`KeyboardInputDetector` は **Unity Input System** の `InputAction`（`InputActionReference` 経由）を購読し、回転・移動・落下操作をユースケース／`CubeUIController` に橋渡しする `MonoBehaviour`。
ゲームロジックは持たず、`Initialize(GameStateMachine)` 後にアクションの `performed` で反応する。

## 2. 配置

| 項目 | 値 |
|------|-----|
| パス | `Assets/Scripts/Presentation/InputDetectors/KeyboardInputDetector.cs` |
| 名前空間 | `Presentation.InputDetectors` |
| 基底クラス | `MonoBehaviour` |

## 3. 依存関係

### 3.1 SerializeField

| フィールド名 | 型 | 説明 |
|-------------|-----|------|
| `_cubeUIController` | `CubeUIController` | 回転の委譲先 |
| `_rotateUAction` … `_rotateBAction` | `InputActionReference` | R/U/F/L/D/B 各 1 手（時計回り側の操作にバインド想定） |
| `_counterClockwiseModifierAction` | `InputActionReference` | 押下中は各回転をプライム（逆回転）として解釈 |
| `_moveLeftAction` … `_hardDropAction` | `InputActionReference` | テトリス操作 |

### 3.2 実行時注入

- `Initialize(GameStateMachine stateMachine)` で `_stateMachine` を渡す。未初期化時は購読しない。

実際にバインドするキー／ゲームパッドは Input Actions アセット側の設定に従う。論理割り当ての例は `UI_Layout.md` §6.2 を参照。

## 4. 回転と CubeOperation

修飾キー `_counterClockwiseModifierAction` がアクティブなとき、各面操作は **プライム（インバース）** の `CubeOperation` を選ぶ。

| 面操作（ベース） | 修飾なし | 修飾あり（逆回転） |
|------------------|----------|---------------------|
| U | `CubeOperation.U` | `CubeOperation.Ui` |
| R | `CubeOperation.R` | `CubeOperation.Ri` |
| F | `CubeOperation.F` | `CubeOperation.Fi` |
| L | `CubeOperation.L` | `CubeOperation.Li` |
| D | `CubeOperation.D` | `CubeOperation.Di` |
| B | `CubeOperation.B` | `CubeOperation.Bi` |

`HandleRotate` は `TryConsumeGameplayInput()` が `true` のときだけ `_cubeUIController.ExecuteRotateAsync(operation).Forget()` を呼ぶ。

## 5. テトリス操作（移動・落下）

`TryConsumeGameplayInput()` が `true` のとき、現在の `GameState` を `GameStateObservable.CurrentValue` から取り、結果を `ApplyGameState` で反映する。

| 処理 | 呼び出し |
|------|----------|
| 左・右 | `MoveMinoUseCase.Execute(..., MoveDirection.Left \| Right)` |
| ソフト／ハード | `DropMinoUseCase.Execute(..., DropType.Soft \| Hard)` |

## 6. 入力の抑止条件（TryConsumeGameplayInput）

次のいずれかなら **すべてのゲームプレイ入力を消費しない**（`false`）。

- `GameStateMachine` 未設定
- `CurrentValue.IsGameOver`
- `CurrentValue.ScramblingMoves.Count > 0`（スクランブル再生中。詳細は `Application_GamePhaseState_Scrambling.md`）

## 7. ライフサイクル

- `OnEnable` / `Initialize` で `SubscribeInputActions`（`performed` にハンドラ、`counterClockwise` は `performed` / `canceled` でフラグ更新）。
- `OnDisable` で購読解除。

## 8. 設計指針

- **ゲームロジックを持たない**: 回転は `CubeUIController`（内部で `RotateMinoUseCase` とアニメ）、移動落下は各ユースケースへ委譲。
- **UniTask**: `ExecuteRotateAsync` は `Cysharp.Threading.Tasks` のため `.Forget()` で発火。
- 他デバイス（`GamepadInputDetector`、`CubeInputDetector`、`TetrisInputView`）も同様に `ScramblingMoves` または専用ガードでスクランブル中をブロックする。
