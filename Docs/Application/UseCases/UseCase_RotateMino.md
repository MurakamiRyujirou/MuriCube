# Docs/Application/UseCases/UseCase_RotateMino.md

## 1. 概要

`RotateMinoUseCase` はプレイヤー入力を受け取り、`ActiveMino` を指定の軸・方向で90度回転する。
回転後に衝突が発生する場合は元の `GameState` をそのまま返す（回転キャンセル）。

## 2. 配置

| 項目 | 値 |
|------|-----|
| ソース | `Assets/Scripts/Application/UseCases/RotateMinoUseCase.cs` |
| 名前空間 | `Application.UseCases` |
| 型 | `static class RotateMinoUseCase` |

## 3. 公開API

```csharp
public static class RotateMinoUseCase
{
    // ActiveMino を指定の軸・方向で 90度回転する。
    // ActiveMino が null または回転後に衝突する場合は元の GameState を返す。
    public static GameState Execute(GameState gameState, RotateAxis axis, CubeTurn turn);
}
```

## 4. 処理フロー

```
1. gameState.ActiveMino が null なら gameState をそのまま返す
2. ActiveMino.BlockGroup を Cube に変換する
3. Cube.Rotate(axis, turn, ActiveMino.Pivot) で回転後の Cube を得る
4. ActiveMino.WithBlockGroup(rotatedCube) で回転後の ActiveMino を生成する
5. rotatedMino.IsColliding(gameState.Field) で衝突判定する
6a. 衝突あり → gameState をそのまま返す（回転キャンセル）
6b. 衝突なし → gameState with { ActiveMino = rotatedMino } を返す
```

## 5. Cube への変換

`ActiveMino.BlockGroup` は `IBlockGroup` だが、回転には `Cube.Rotate` が必要なため変換する。

```csharp
var cube = new Cube(new BlockGroup(mino.BlockGroup.Blocks));
```

回転後の `Cube` は `IBlockGroup` として `WithBlockGroup` に渡す。
これは `SpawnMinoUseCase` のランダム回転と同じパターンである。

## 6. Presentation 層との対応

`UI_Layout.md` §5 で定義した入力と `RotateAxis` / `CubeTurn` の対応は以下の通り。

| 操作 | RotateAxis | CubeTurn |
|------|-----------|---------|
| R 回転 | X | Clockwise |
| R' 回転 | X | CounterClockwise |
| U 回転 | Y | Clockwise |
| U' 回転 | Y | CounterClockwise |
| F 回転 | Z | Clockwise |
| F' 回転 | Z | CounterClockwise |
| L 回転 | X | CounterClockwise |
| L' 回転 | X | Clockwise |
| D 回転 | Y | CounterClockwise |
| D' 回転 | Y | Clockwise |
| B 回転 | Z | CounterClockwise |
| B' 回転 | Z | Clockwise |

この変換は Presentation 層の InputDetector が担い、`RotateMinoUseCase` は `RotateAxis` と `CubeTurn` のみを受け取る。

## 7. 設計指針

- **純粋関数**: 引数の `GameState` を変更せず、新しい `GameState` を返す。
- **UnityEngine 非依存**: 純粋な C# とする。
- **`CanRotate` は使用しない**: ミノ内部のブロック同士の重なりは現状の形状定義では発生しない。フィールドとの衝突判定（`IsColliding`）のみで十分。
- **null安全**: `ActiveMino` が `null` の場合は早期リターンする。