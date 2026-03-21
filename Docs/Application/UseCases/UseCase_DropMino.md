# Docs/Application/UseCases/UseCase_DropMino.md

## 1. 概要

`DropMinoUseCase` はソフトドロップ・ハードドロップを処理する。
- **ソフトドロップ**: Y-1 の移動を1回試みる。失敗した場合は元の `GameState` を返す。
- **ハードドロップ**: 衝突しない限り Y-1 を繰り返し、最下段まで即座に落下させる。

接地判定（ハードドロップ後のフィールドへの固定）は `LockMinoUseCase` が担う。

## 2. 配置

| 項目 | 値 |
|------|-----|
| ソース（列挙型） | `Assets/Scripts/Application/UseCases/DropType.cs` |
| ソース（ユースケース） | `Assets/Scripts/Application/UseCases/DropMinoUseCase.cs` |
| 名前空間 | `Application.UseCases` |
| 型 | `static class DropMinoUseCase` |

## 3. DropType（列挙型）

```csharp
public enum DropType
{
    Soft,   // Y - 1 を1回試みる
    Hard,   // 最下段まで即座に落下
}
```

## 4. 公開API

```csharp
public static class DropMinoUseCase
{
    // ActiveMino をソフトドロップまたはハードドロップさせる。
    // ActiveMino が null の場合は元の GameState を返す。
    public static GameState Execute(GameState gameState, DropType dropType);
}
```

## 5. 処理フロー

### 5.1 ソフトドロップ

```
1. gameState.ActiveMino が null なら gameState をそのまま返す
2. オフセット Y-1 の新しい ActiveMino を生成する
3. movedMino.IsColliding(gameState.Field) で衝突判定する
4a. 衝突あり → gameState をそのまま返す（接地。LockMinoUseCase が後続で処理する）
4b. 衝突なし → gameState with { ActiveMino = movedMino } を返す
```

### 5.2 ハードドロップ

```
1. gameState.ActiveMino が null なら gameState をそのまま返す
2. 現在の ActiveMino を作業用変数 current に代入する
3. current のオフセット Y-1 の新しい ActiveMino を生成する
4. 衝突しない限り current を更新しながら 3 を繰り返す
5. 衝突した時点でループを終了し、直前の current が最終位置
6. gameState with { ActiveMino = current } を返す
```

ハードドロップ後の `LockMinoUseCase` 呼び出しは呼び出し元のフェーズ（`LockDownState`）が担う。

## 6. `MoveMinoUseCase` との関係

ソフトドロップは `MoveMinoUseCase.Execute(gameState, MoveDirection.Down)` と等価だが、
`DropMinoUseCase` として独立させることで以下のメリットがある。

- ハードドロップとソフトドロップを同一ユースケースで管理できる
- 将来ソフトドロップにスコアボーナスを加算する際、`DropMinoUseCase` だけを変更すれば済む

## 7. 設計指針

- **純粋関数**: 引数の `GameState` を変更せず、新しい `GameState` を返す。
- **UnityEngine 非依存**: 純粋な C# とする。
- **null安全**: `ActiveMino` が `null` の場合は早期リターンする。
- **接地後の処理は担わない**: ソフトドロップ・ハードドロップともに、固定処理は `LockMinoUseCase` に委ねる。