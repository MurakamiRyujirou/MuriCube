# Docs/Application/UseCases/UseCase_LockMino.md

## 1. 概要

`LockMinoUseCase` は接地したミノをフィールドに固定する。
- z=0 のセルのみ `Field` に配置する（z=1 は破棄）
- 配置後に `ActiveMino` を `null` にクリアする

## 2. 配置

| 項目 | 値 |
|------|-----|
| ソース | `Assets/Scripts/Application/UseCases/LockMinoUseCase.cs` |
| 名前空間 | `Application.UseCases` |
| 型 | `static class LockMinoUseCase` |

## 3. 公開API

```csharp
public static class LockMinoUseCase
{
    // 接地した ActiveMino をフィールドに固定し、ActiveMino を null にクリアする。
    // ActiveMino が null の場合は元の GameState を返す。
    public static GameState Execute(GameState gameState);
}
```

## 4. 処理フロー

```
1. gameState.ActiveMino が null なら gameState をそのまま返す
2. ActiveMino.AbsolutePositions() と BlockGroup.Blocks を対応づけて各セルを取得する
3. 各セルについて絶対座標の Z を確認する
   - Z == 0（Front レイヤー）→ Field.WithCell で配置する
   - Z == 1（Back レイヤー）→ 破棄する（配置しない）
4. gameState with { Field = newField, ActiveMino = null } を返す
```

## 5. z=1 破棄の仕様

`GameDesign.md` §2「消滅レイヤー」に基づく。

> 奥の面（z=1）は、ブロックが固定された瞬間にその座標にあるセルデータが破棄される。積み上がることはない。

これにより、フィールドに積み上がるのは z=0 のブロックのみとなる。

## 6. AbsolutePositions と BlockGroup.Blocks の対応

`ActiveMino.AbsolutePositions()` は絶対座標の列挙を返すが、対応するブロック（`IBlock`）は
`BlockGroup.Blocks`（相対座標→IBlock）から取得する必要がある。

具体的には以下の手順で対応づける。

```csharp
foreach (var kv in mino.BlockGroup.Blocks)
{
    var absolutePos = Combine(mino.Offset, kv.Key);  // 相対→絶対変換
    var block = kv.Value;

    if (absolutePos.Z != Field.MinZ) continue;  // z=1 は破棄

    field = field.WithCell(absolutePos, block);
}
```

`Combine` は `ActiveMino` 内の `ToGrid`（`MidpointRounding.AwayFromZero`）と同じ丸め処理で実装する。

## 7. 設計指針

- **純粋関数**: 引数の `GameState` を変更せず、新しい `GameState` を返す。
- **UnityEngine 非依存**: 純粋な C# とする。
- **null安全**: `ActiveMino` が `null` の場合は早期リターンする。
- **ライン消去は担わない**: 固定後のライン消去は `LineClearUseCase` が担う。呼び出し元（`LockDownState`）が `LockMinoUseCase` → `LineClearUseCase` の順で呼ぶ。