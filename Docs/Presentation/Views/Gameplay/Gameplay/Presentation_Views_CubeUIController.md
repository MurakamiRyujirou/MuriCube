# Docs/Presentation/Views/Gameplay/Presentation_Views_CubeUIController.md

## 1. 概要

`CubeUIController` は `GameStateMachine` と `CubeUIView` を接続する `MonoBehaviour`。
`GameStateObservable` を購読し、`ActiveMino` のスポーン時に `CubeUIView.Build` を呼ぶ。
回転入力を受け取り、**ドメイン採否（`RotateMinoUseCase`）を先に実行**したうえで、採択時のみ `CubeUIView.RotateAsync` → `Refresh` → `ApplyGameState` とする（§4.2・§8）。

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

### 4.2 ExecuteRotateAsync / ExecuteRotateCoreAsync（`CubeOperation`）

公開 API は `ExecuteRotateAsync(CubeOperation operation, float? durationOverride)`。内部の `ExecuteRotateCoreAsync` が実処理を担い、**スクランブル再生**（短い `durationOverride`）でも同一経路を通る。

#### 4.2.1 処理順（設計／Task 047 対象）

**意図**: フィールド衝突などで回転が **キャンセル**される場合、`RotateAsync` や `Refresh` でビューを進めない。そうしないと `GameState.ActiveMino` は元のままなのに `CubeUIView` だけ回転後形状になり、スクランブル完了時の `Build` や初回プレイヤー回転で**見た目とモデルが食い違う**。

```
1. _cubeUIView.IsRotating が true なら早期リターン（多重入力防止）
2. GameState.ActiveMino が null なら早期リターン
3. current = GameStateMachine.GameStateObservable.CurrentValue をスナップショット
4. ActiveMino（= current.ActiveMino）の BlockGroup を Cube に変換
5. cube.CanRotate(operation, pivot) が false なら早期リターン（内部オーバーラップ）
6. newGameState = RotateMinoUseCase.Execute(current, operation)   ← ドメイン先行
7. ReferenceEquals(newGameState, current) なら早期リターン（回転キャンセル・演出なし）
8. (axis, turn) = CubeOperationRotation.ToAxisAndTurn(operation)
9. affected = cube.GetAffectedBlocks(operation, pivot)
10. await _cubeUIView.RotateAsync(axis, turn, pivot, duration, affected)
11. positionMap = cube.GetPositionMap(operation, pivot)
12. rotatedCube = cube.Rotate(operation, pivot)
13. _cubeUIView.Refresh(rotatedCube, positionMap)
14. _lastActiveMinoRef = newGameState.ActiveMino
15. ApplyGameState(newGameState)
```

- **6〜7**: `RotateMinoUseCase` は衝突時に **入力の `GameState` と同一参照**を返し、採択時は `with` による新インスタンスを返す。この契約に基づき、**必ず Execute に渡した `current` と戻り値**を `ReferenceEquals` する（`Execute` 後の `CurrentValue` との比較は避ける）。
- **10 以降**: `GameStateObservable` はまだ `current` のままなので、`cube` は**未回転の形状**のまま。アニメはこの状態から開始できる。

#### 4.2.2 変更前の順序（参考・不具合の原因）

アニメ → `Refresh` → `RotateMinoUseCase` → `ApplyGameState` としていた期間は、7 でキャンセルされても 10〜13 が完了しており、**ビューだけが進む**ことがあった。

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
- **UniTask**: `ExecuteRotateAsync` は `async UniTask` で実装する。

## 8. 実行順序の意図（まとめ）

| 観点 | 内容 |
|------|------|
| **なぜドメイン先行か** | `RotateMinoUseCase` は `IsColliding` により回転を**採択／却下**する。却下のときにビューを進めると、モデルと `CubeUIView` が一致しなくなる。 |
| **なぜ参照一致か** | 却下時は同一 `GameState` 参照を返す。採択時は新レコード。`ReferenceEquals(new, current)` が最も単純で、ユースケース契約と一致する。 |
| **スクランブルとの関係** | `PlayScramblingAsync` も `ExecuteRotateCoreAsync` を繰り返す。手順の一部が却下された場合、**その回はアニメしない**ため、不要な見た目の積み上げを抑える。 |
| **残る課題** | `SpawnMinoUseCase` がスクランブル手順を `CanRotate` のみで記録している場合、`IsColliding` と食い違う手順は引き続き却下されうる（モデル上は回転が進まない）。表示の破綻は抑えられるが、手順リストとドメインの完全一致には別タスクが必要になりうる。 |

関連: `ISSUES.md` [Task 047]。