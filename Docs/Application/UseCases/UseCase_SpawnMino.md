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
    // ランダムな MinoType を選択し、スクランブル手順と最終形状を求めつつ、
    // ActiveMino は整列形状のままスポーンし ScramblingMoves に手順を載せる。
    // 衝突が発生した場合は IsGameOver = true の GameState を返す。
    public static GameState Execute(GameState gameState, System.Random random);
}
```

## 4. 処理フロー

```
1. MinoType をランダムに選択する
2. MinoFactory.Create(type) で整列・固定配色の ActiveMino（alignedMino）を得る
3. その BlockGroup のコピーから Cube を構築し、最大 20 回のランダム試行で回転する
   - 各試行で軸 X/Y/Z をランダム選択し、対応する CubeOperation（R / U / F）を決める
   - CanRotate(op, pivot) が true のときだけ ScramblingMove(op) を列に追加し Cube.Rotate(op, pivot) を適用
4. 得られた Cube を WithBlockGroup したミノにスポーンオフセット（§5）を適用し、衝突判定用の形状 rotatedMino を得る
5. rotatedMino.IsColliding(field) で判定
6a. 衝突あり → gameState with { IsGameOver = true }（ActiveMino・ScramblingMoves は変えない）
6b. 衝突なし → alignedMino に同じオフセットだけ適用した activeAlignedAtSpawn を返す:
        gameState with { ActiveMino = activeAlignedAtSpawn, ScramblingMoves = moves }
```

`ActiveMino.BlockGroup` は **常に整列状態**。ユーザーが見るランダム形状は `CubeUIView` が `ScramblingMoves` を順に再生して `GameState` を更新することで一致する。詳細は `Application_GamePhaseState_Scrambling.md`。

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

## 6. ランダム回転と ScramblingMoves

`GameDesign.md` §3.2「生成時、ランダムな軸で90度回転を複数回実行した状態で提示する」に準拠する。

回転試行回数は WCA の 20 手を参考に **20 回**（必ず 20 手採用するのではなく、最大 20 回試行し `CanRotate` を通過した分だけ列に載せる）。

- **軸のランダム化**: 各試行で `RotateAxis.X` / `Y` / `Z` を等確率で選ぶ
- **操作の対応**: X → `CubeOperation.R`、Y → `CubeOperation.U`、Z → `CubeOperation.F`（いずれも公式記法におけるその軸の正方向「ベース」操作。逆回転はスクランブル生成では使わない）
- **Pivot**: `alignedMino.Pivot` をそのまま使う
- **実装**: `Cube.CanRotate(op, pivot)` / `Cube.Rotate(op, pivot)` を採用可能な手だけ繰り返す。採用された `op` は `ScramblingMove` に逐次格納する

## 7. Cube への変換

`MinoFactory` が返す `ActiveMino` のブロック集合を **ディープコピー**した `BlockGroup` から `Cube` を作り、上記ループで変形する（元の `alignedMino` の `IBlockGroup` は汚さない）。

```csharp
var blockGroup = new BlockGroup(alignedMino.BlockGroup.Blocks);
var cube = new Cube(blockGroup);
```

衝突判定とオフセット計算には、ループ終了後の `cube` を `alignedMino.WithBlockGroup(cube)` に載せ替えたミノを用いる。返却する `ActiveMino` の `BlockGroup` は整列のままである点に注意（§4）。

## 8. 設計指針

- **UnityEngine 非依存**: `System.Random` を引数で受け取ることで、Unity の `Random` に依存しない。
- **純粋関数**: 引数の `GameState` を変更せず、新しい `GameState` を返す。
- **ゲームオーバー判定**: 衝突時は `ActiveMino` を `GameState` に反映せず、`IsGameOver = true` のみ設定する。