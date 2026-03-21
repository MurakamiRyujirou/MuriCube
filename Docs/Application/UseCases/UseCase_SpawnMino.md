# Docs/Application/UseCases/UseCase_SpawnMino.md

## 1. 概要

`SpawnMinoUseCase` は新しいミノを生成し、フィールド上部中央に配置する。
生成直後に衝突が発生する場合はゲームオーバーとして `IsGameOver = true` の `GameState` を返す。

## 2. 配置

| 項目 | 値 |
|------|-----|
| ソース | `Assets/Scripts/Application/UseCases/SpawnMinoUseCase.cs` |
| 名前空間 | `Application.UseCases` |
| 型 | `static class SpawnMinoUseCase` |

## 3. 公開API

```csharp
public static class SpawnMinoUseCase
{
    // ランダムな MinoType を選択し、ランダム回転を加えてフィールド上部中央に配置する。
    // 衝突が発生した場合は IsGameOver = true の GameState を返す。
    public static GameState Execute(GameState gameState, System.Random random);
}
```

## 4. 処理フロー

```
1. MinoType をランダムに選択する（random.Next を使用）
2. MinoFactory.Create(type) で固定配色・固定形状の ActiveMino を生成する
3. BlockGroup を Cube に変換し、20回のランダム回転を加える
4. 回転後の Cube を IBlockGroup として WithBlockGroup で ActiveMino に反映する
5. スポーン位置（フィールド上部中央）のオフセットを計算し WithOffset で設定する
6. ActiveMino.IsColliding(field) で衝突判定する
7a. 衝突あり → gameState with { IsGameOver = true } を返す
7b. 衝突なし → gameState with { ActiveMino = mino } を返す
```

## 5. スポーン位置

Tetris Guideline の「スポーンは上部付近の基準列・基準行」「ただしフィールド外には出さない（必要なら位置を調整する）」に合わせる。

### 5.1 目標（基準グリッド）

ランダム回転**後**、`ActiveMino` のオフセットが `(0,0,0)` のとき `AbsolutePositions()` で得られる各セル（丸め後ローカルを整数化したもの）の **最小 X・最小 Y・最小 Z** を `minX` / `minY` / `minZ` とする。

ミノのオフセット `(spawnX, spawnY, spawnZ)` は、まず次の「目標」で求める。

| 軸 | 目標 |
|----|------|
| X | `spawnX = 3 - minX`（横幅10の中央左寄り: Guideline の基準列 3） |
| Y | `spawnY = 18 - minY`（上部スポーン行の基準: y=18 をバウンディング下端に合わせる） |
| Z | `spawnZ = 0 - minZ`（Front レイヤー z=0 を基準） |

### 5.2 ウェル内への Clamp

回転により `minX` 等が変わるため、上記だけでは X/Y/Z が `Field` の境界を超えることがある。
そのため **配置後**の全セルが `Field.Contains` を満たす範囲で、各軸を次の区間に Clamp する。

- `spawnX ∈ [Field.MinX - minX, Field.MaxX - maxX]`
- `spawnY ∈ [Field.MinY - minY, Field.MaxY - maxY]`
- `spawnZ ∈ [Field.MinZ - minZ, Field.MaxZ - maxZ]`

`minAllowed > maxAllowed` の退化時（形状がウェル幅に収まらない等）は `minAllowed` を採用する。

### 5.3 実装メモ

- スポーンオフセットは **固定定数 1 組ではなく**、回転後の形状に対して上記で **毎回計算**する。
- `MinoFactory` のオフセットは引き続き `(0,0,0)`。本ユースケースが `WithOffset` で最終位置を決める。

## 6. ランダム回転

`GameDesign.md` §3.2「生成時、ランダムな軸で90度回転を複数回実行した状態で提示する」に準拠する。

回転回数はWCA公式スクランブルの手数（20手）を参考に **20回** とする。
これはルービックキューブのすべての状態が20手以内で解けるという「神の数字」に基づく値であり、
20回の回転で十分なランダム性が確保できる。

- **回転軸**: `RotateAxis.X` / `RotateAxis.Y` / `RotateAxis.Z` からランダムに選択
- **回転回数**: 20回（固定）
- **回転方向**: 常に `CubeTurn.Clockwise`（回数でランダム性を確保するため方向は固定）
- **Pivot**: `ActiveMino.Pivot` をそのまま使用する
- **実装**: `BlockGroup` を `Cube` でラップし、`Cube.Rotate` を20回チェーンする

## 7. Cube への変換

`MinoFactory` が返す `ActiveMino` の `BlockGroup` は `IBlockGroup` だが、
ランダム回転には `Cube.Rotate` が必要なため、以下の手順で変換する。

```csharp
// IBlockGroup → Cube への変換
var cube = new Cube(new BlockGroup(mino.BlockGroup.Blocks));
```

回転後は `Cube`（`IBlockGroup` として扱える）を `WithBlockGroup` に渡す。

## 8. 設計指針

- **UnityEngine 非依存**: `System.Random` を引数で受け取ることで、Unity の `Random` に依存しない。
- **純粋関数**: 引数の `GameState` を変更せず、新しい `GameState` を返す。
- **ゲームオーバー判定**: 衝突時は `ActiveMino` を `GameState` に反映せず、`IsGameOver = true` のみ設定する。