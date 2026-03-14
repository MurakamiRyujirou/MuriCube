# Docs/Domain_Cube.md

## 1. 概要
3D空間における「ブロックの集合体」の移動・回転・配色管理を定義する。
特定の形状（テトリミノ等）に依存せず、任意の中心座標（Pivot）に基づいた空間回転をサポートする。

## 2. 基底型（Enums & Value Objects）

### 2.1 CubeAxis
- X, Y, Z

### 2.2 CubeTurn
- Clockwise (90°), CounterClockwise (-90°), HalfTurn (180°)

### 2.3 PivotPosition (Value Object)
- 回転の軸となる空間上の絶対位置を示す座標 (float X, float Y, float Z)。
- ブロックの空間的な範囲を $BlockPosition \sim BlockPosition + 1.0$ と定義した際の座標を扱う。
  - 例: $3 \times 3 \times 3$ の中心回転なら $(1.5, 1.5, 1.5)$。
  - 例: $2 \times 2 \times 2$ の格子点回転なら $(1.0, 1.0, 1.0)$。

## 3. 主要エンティティ

### 3.1 Block (Value Object) : IBlock 実装
- **データ構造**: 各 `BlockFace` に対応する `BlockColor` を保持。
- **責務**: 指定された `CubeAxis` と `CubeTurn` に基づく「自転（面のスワップ）」を行い、新しい `Block` インスタンスを返す。
- **インターフェース実装**: `GetColor(BlockFace face)` により指定面の配色を返す。

### 3.2 BlockGroup (Entity) : IBlockGroup 実装
複数の `Block` の集合と、その配置構造を管理する。
- **データ構造**:
    - `Dictionary<BlockPosition, Block>` : 座標とブロック実体のマッピング。
- **インターフェースプロパティ**:
    - `IReadOnlyDictionary<BlockPosition, IBlock> Blocks` : 上記マッピングを IBlock として公開。
- **回転ロジック**:
    - `Rotate(CubeAxis axis, CubeTurn turn, PivotPosition pivot)`
    - **公転**: `pivot` を中心として、配下にある各ブロックの `BlockPosition` を幾何学的に置換する。
    - **自転**: 各ブロックに対して、同じ `axis` と `turn` に応じた自転を命令する。
- **不変性**: 回転や移動の操作は、常に新しい `BlockGroup` インスタンスを生成して返す。

## 4. 回転の数学的定義（汎用）

任意の回転平面において、中心 $(p_1, p_2)$ を基準とした座標 $(a, b)$ の 90度時計回り（Clockwise）回転は以下の通り定義する。

$$a' = p_1 + (b - p_2)$$
$$b' = p_2 - (a - p_1)$$

※ `CounterClockwise` および `HalfTurn` は上記を応用、または合成して定義する。

## 5. 設計指針
- **エンジンの非依存性**: `UnityEngine` を含む外部ライブラリに依存せず、純粋な C# の論理演算として実装する。
- **形状の自由度**: ブロックがどのように連結されていても、指定された `pivot` を基準に整合性を保って回転可能とする。
- **副作用の排除**: 状態の上書きを禁止し、常に新しいインスタンスを構築することで、データの整合性を担保する。