# Docs/Domain_Tetris_ActiveMino.md

`Domain_Tetris.md` §3.2 **ActiveMino** の実装仕様を定義する。概要・ゲーム上の責務は `Domain_Tetris.md` に従う。

## 1. 配置（クラスパス・名前空間）

| 項目 | 値 |
|------|-----|
| ソース | `Assets/Scripts/Domain/Tetris/ActiveMino.cs` |
| 名前空間 | `Domain.Tetris` |
| 型名 | `ActiveMino`（`sealed class`） |

## 2. データ構造（フィールド定義）

コンストラクタおよび不変更新で保持する論理データは次のとおり（実装では `readonly` フィールドに相当）。

| 論理名 | 型 | 意味 |
|--------|-----|------|
| 形状タイプ | `MinoType` | テトリミノの種類（I / O / S / Z / J / L / T） |
| ブロック集合 | `IBlockGroup` | 相対配置と各色を持つ立体。`Blocks` で `BlockPosition` → `IBlock` |
| オフセット | `CubePosition` | フィールド上の基準位置（ワールド／ウェル上の絶対グリッド原点に対する並進） |
| Pivot | `PivotPosition` | 回転の中心座標（ミノの形状に対して定まる初期値）。相対座標で保持する |

公開プロパティ: `MinoType`、`IBlockGroup BlockGroup`、`CubePosition Offset`、`PivotPosition Pivot`。

## 3. 各メソッドの仕様

### 3.1 `AbsolutePositions()`

- **シグネチャ**: `IEnumerable<CubePosition> AbsolutePositions()`
- **動作**: `BlockGroup.Blocks` の各エントリについて、相対座標 `BlockPosition` とオフセット `CubePosition` から **フィールド絶対座標** `CubePosition` を求め、列挙で返す。
- **座標変換**: §4 の丸め規則に従い、相対位置の float 成分を整数化してからオフセットの各軸に加算する。

### 3.2 `WithOffset(CubePosition offset)`

- **戻り値**: `ActiveMino`
- **動作**: `MinoType`・`IBlockGroup`・`Pivot` はそのまま、`CubePosition` オフセットだけを差し替えた **新しい** `ActiveMino` を返す（落下・水平移動用の不変更新）。

### 3.3 `WithBlockGroup(IBlockGroup blockGroup)`

- **戻り値**: `ActiveMino`
- **動作**: `MinoType`・オフセット・`Pivot` はそのまま、`IBlockGroup` だけを差し替えた **新しい** `ActiveMino` を返す（回転後の形状用の不変更新）。
- **制約**: `blockGroup` が `null` の場合は `ArgumentNullException` とする。

### 3.4 `WithPivot(PivotPosition pivot)`

- **戻り値**: `ActiveMino`
- **動作**: `MinoType`・`IBlockGroup`・オフセットはそのまま、`Pivot` だけを差し替えた **新しい** `ActiveMino` を返す（将来のプレイヤーによるPivot切り替え用の不変更新）。

### 3.5 `IsColliding(Field field)`

- **戻り値**: `bool`（衝突ありで `true`）
- **動作**: 衝突判定の対象は **絶対座標の Z が `Field.MinZ`（=0）と一致するセルのみ** とする。
- **Z=0 以外のセル**（z=-1・z=2 など、回転によりフィールド外の Z 座標になったセルを含む）は **判定をスキップ** する。
- これはテトリス世界が z=0 の平面のみで成立するためであり、ルービックキューブの回転によって z=0 範囲外に出たセルはテトリスの判定に影響しない。
- 上記の対象セルについて、次を順に判定する。
  1. `Field.Contains(p)` が `false` → ウェル外なので **衝突**（`true`）
  2. `field.TryGetBlock(p, out _)` が `true` → 占有セルと重なるので **衝突**（`true`）
  3. いずれにも該当しないセルのみなら **非衝突**（`false`）
- **制約**: `field` が `null` の場合は `ArgumentNullException` とする。
- **分担**: 境界は静的メソッド `Field.Contains`、占有は `Field` インスタンスの `TryGetBlock`（`Domain_Tetris.md` §3.1 と整合）。

## 4. `BlockPosition` → `CubePosition` 変換の丸め仕様

`BlockPosition` は `float` の (X, Y, Z) を持つ。絶対 `CubePosition` は `int` の (X, Y, Z) であるため、成分ごとに次で整数化する。

- **丸め**: `Math.Round(value, MidpointRounding.AwayFromZero)` の結果を `int` に変換する（実装では `(int)Math.Round(...)`）。
- **意味**: 中点（*.5）は **ゼロから離れる方向**に進める（例: 2.5 → 3、-1.5 → -2）。ピボット由来の 0.5 座標と整数グリッド `CubePosition` の整合に用いる。

絶対座標の各軸は次式。

\[
\text{absoluteAxis} = \text{offsetAxis} + \text{RoundAway}(\text{localAxis})
\]

## 5. 設計指針

- **不変性**: `WithOffset` / `WithBlockGroup` / `WithPivot` は常に新インスタンスを返す。ミュータブルな共有状態を持たない。
- **Pivot の拡張性**: 現時点では1ミノ1Pivot固定。将来「複数Pivot候補を持ちプレイヤーが切り替える」拡張が必要になった場合は `AvailablePivots` と `CurrentPivot` に分離する形で対応する（`WithPivot` のシグネチャは維持できる）。
- **具象非依存**: `IBlockGroup` の具象（例: `Domain.Cube.BlockGroup` や `Cube`）へは **参照しない**。コンストラクタ・`WithBlockGroup` の引数および `BlockGroup` プロパティは **`IBlockGroup` のみ**。`UnityEngine` に依存しない純粋な C# とする。