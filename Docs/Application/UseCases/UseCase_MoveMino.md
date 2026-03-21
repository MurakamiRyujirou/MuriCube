# Docs/Application/UseCases/UseCase_MoveMino.md

## 1. 概要

`MoveMinoUseCase` はプレイヤー入力を受け取り、`ActiveMino` を左・右・下方向に1セル移動する。
移動後に衝突が発生する場合は元の `GameState` をそのまま返す（移動キャンセル）。
接地判定（下移動の失敗からロックダウンへの遷移）は呼び出し元のフェーズが担う。

## 2. 配置

| 項目 | 値 |
|------|-----|
| ソース（列挙型） | `Assets/Scripts/Application/UseCases/MoveDirection.cs` |
| ソース（ユースケース） | `Assets/Scripts/Application/UseCases/MoveMinoUseCase.cs` |
| 名前空間 | `Application.UseCases` |
| 型 | `static class MoveMinoUseCase` |

## 3. MoveDirection（列挙型）

```csharp
public enum MoveDirection
{
    Left,   // X - 1
    Right,  // X + 1
    Down,   // Y - 1（上がプラスのため下はマイナス）
}
```

## 4. 公開API

```csharp
public static class MoveMinoUseCase
{
    // ActiveMino を指定方向に 1 セル移動する。
    // ActiveMino が null または移動後に衝突する場合は元の GameState を返す。
    public static GameState Execute(GameState gameState, MoveDirection direction);
}
```

## 5. 処理フロー

```
1. gameState.ActiveMino が null なら gameState をそのまま返す
2. direction に応じてオフセットの差分 (dx, dy, dz) を決定する
      Left  → (-1,  0, 0)
      Right → (+1,  0, 0)
      Down  → ( 0, -1, 0)
3. 新しいオフセット = 現在のオフセット + 差分
4. mino.WithOffset(newOffset) で移動後の ActiveMino を生成する
5. movedMino.IsColliding(gameState.Field) で衝突判定する
6a. 衝突あり → gameState をそのまま返す（移動キャンセル）
6b. 衝突なし → gameState with { ActiveMino = movedMino } を返す
```

## 6. 接地判定との分担

- `MoveMinoUseCase` は Down 方向への移動失敗を**検知しない**。
- 呼び出し元（`FallingState`）が `Execute` の戻り値を見て「`ActiveMino` が変化しなかった＝接地」と判断し、`LockDownState` へ遷移する責務を持つ。
- これにより `MoveMinoUseCase` は純粋な移動処理に専念できる。

## 7. 設計指針

- **純粋関数**: 引数の `GameState` を変更せず、新しい `GameState` を返す。
- **UnityEngine 非依存**: 純粋な C# とする。
- **null安全**: `ActiveMino` が `null` の場合は早期リターンする。