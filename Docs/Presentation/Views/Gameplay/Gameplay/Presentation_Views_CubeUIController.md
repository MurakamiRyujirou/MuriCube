# Docs/Presentation/Views/Gameplay/Presentation_Views_CubeUIController.md

## 1. 概要

`CubeUIController` は `GameStateMachine` と `CubeUIView` を接続する `MonoBehaviour`。
`GameStateObservable` を購読し、`ActiveMino` のスポーン時に `CubeUIView.Build` を呼ぶ。
回転入力を受け取り `RotateMinoUseCase` → `CubeUIView.RotateAsync` → `Refresh` の順で処理する。

`CubeUIView` 自体は変更しない。このクラスが接続層として機能する。

## 2. 配置

| 項目 | 値 |
|------|-----|
| パス | `Assets/Scripts/Presentation/Views/Gameplay/CubeUIController.cs` |
| 名前空間 | `Presentation.Views.Gameplay` |
| 基底クラス | `MonoBehaviour` |

## 3. 構造（SerializedFields）

| フィールド名 | 型 | 説明 |
|-------------|-----|------|
| `_cubeUIView` | `CubeUIView` | キューブエリアの View |
| `_rotateDuration` | `float` | 回転アニメーション時間（デフォルト: 0.3f） |

## 4. 公開メソッド

### 4.1 Initialize(GameStateMachine stateMachine)

- `GameStateObservable` を購読する
- `ActiveMino` が変化したとき（前回と参照が異なる場合）に `CubeUIView.Build(activeMino.BlockGroup)` を呼ぶ
- `ActiveMino` が `null` になった場合（ロック後）は何もしない
- 初回購読時に現在の `GameState` で即座に `Build` を試みる

### 4.2 ExecuteRotateAsync(RotateAxis axis, CubeTurn turn)

回転入力を受け取り以下の順で処理する。

```
1. _cubeUIView.IsRotating が true なら早期リターン（多重入力防止）
2. GameState.ActiveMino が null なら早期リターン
3. ActiveMino.BlockGroup を Cube に変換する
4. cube.CanRotate(axis, turn, activeMino.Pivot) で衝突検証
5. affected = cube.GetAffectedBlocks(axis, turn, activeMino.Pivot)
6. await _cubeUIView.RotateAsync(axis, turn, activeMino.Pivot, _rotateDuration, affected)
7. positionMap = cube.GetPositionMap(axis, turn, activeMino.Pivot)
8. rotatedCube = cube.Rotate(axis, turn, activeMino.Pivot)
9. _cubeUIView.Refresh(rotatedCube, positionMap)
10. RotateMinoUseCase.Execute(gameState, axis, turn) で GameState を更新する
    → GameStateMachine 経由で GameState を更新する手段が必要（§5参照）
```

## 5. GameState の更新方法

`ExecuteRotateAsync` 内で `RotateMinoUseCase.Execute` を呼んだ結果を `GameStateMachine` に反映する必要がある。

現状の `GameStateMachine` は `OnUpdate(deltaTime)` でのみ状態を更新する設計のため、
外部からユースケース結果を注入する口として `GameStateMachine.ApplyGameState(GameState)` を追加する。

```csharp
// GameStateMachine に追加するメソッド
public void ApplyGameState(GameState newGameState)
{
    _gameState.Value = newGameState;
}
```

## 6. OnDestroy

購読を解除する。

## 7. 設計指針

- **CubeUIView は変更しない**: 既存の `CubeUIView` はそのまま使用する。
- **GameState の単一管理**: 回転後の `GameState` は必ず `GameStateMachine` 経由で更新し、`FieldUIView` にも通知が届くようにする。
- **UniTask**: `ExecuteRotateAsync` は `async UniTaskVoid` で実装する。