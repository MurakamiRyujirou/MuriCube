# Docs/TechSpecs.md

## 1. ドメイン構成と依存関係
本プロジェクトは、機能の独立性を担保するため以下の3つのレイヤーでドメインを構成する。

- **Domain_Common**: ブロックの属性、共通インターフェース、列挙型を定義する。他ドメインから参照されるが、自身は他を参照しない。
- **Domain_Cube**: 3D幾何学的な「回転」と「配色スワップ」の具象ロジックを担う。
- **Domain_Tetris**: フィールド管理、落下、接地、消去判定などの「パズルルール」を担う。

※ **形状データの疎結合**: テトリスはブロック集合を `Domain_Common` の `IBlockGroup` / `IBlock` 越しに扱い、`Cube` の回転アルゴリズム具象には依存しない。

※ **幾何の共有**: `Domain_Tetris` の `ActiveMino` は回転中心として `Domain.Cube.PivotPosition` を保持するため、`Domain_Cube` 名前空間へ **一方向参照**する。`Domain_Cube` 側は `Domain_Tetris` を参照しない。

## 2. 座標系とデータ構造の定義

### 2.1 グリッド座標系
- 単位: 1.0 単位の整数グリッド（Unityの座標系に準拠）。
- 原点 (World Origin): フィールドの「左下手前」を (0, 0, 0) とする。
- 軸の定義:
    - x軸: 右方向がプラス (0 〜 9)
    - y軸: 上方向がプラス (0 〜 19)
    - z軸: 奥方向がプラス (0: Frontレイヤー, 1: Backレイヤー)

### 2.2 ブロックデータの属性定義 (Domain_Common)
各ブロックは、以下の6方向に対応する配色情報を保持する。

- 方向定義 (BlockFace):
    - Up, Down, Left, Right, Front (z=0方向), Back (z=1方向)
- 配色定義 (BlockColor):
    - Red, Blue, White, Yellow, Green, Orange
    - (Empty: ブロックが存在しない状態)

### 2.3 抽象インターフェース
- **IBlock**: `GetColor(BlockFace face)` で各面の配色を取得する（`Domain_Common.md` 参照）。
- **IBlockGroup**: ブロックの集合体。テトリスドメインからは「形状を持つオブジェクト」として扱われ、中身がルービックキューブであるか等の具象実装を隠蔽する。

## 3. 回転ロジック (Rotation Logic)

### 3.1 90度単位の座標置換（空間回転）
回転軸（Pivot）を中心とした座標置換を行う。Pivotは整数格子点（格子回転）または 0.5単位の実数（中心回転）を許容する。

- **X軸回転**: (y, z) -> (z, -y) 方向の成分置換。
- **Y軸回転**: (x, z) -> (-z, x) 方向の成分置換。
- **Z軸回転**: (x, y) -> (-y, x) 方向の成分置換。

### 3.2 ブロック内配色データのスワップ
位置の移動と同時に、ブロック自体が持つ6面の配色情報を以下の通り更新する。

- **X軸回転**: Front -> Up, Up -> Back, Back -> Down, Down -> Front (L/R不変)
- **Y軸回転**: Front -> Left, Left -> Back, Back -> Right, Right -> Front (U/D不変)
- **Z軸回転**: Up -> Right, Right -> Down, Down -> Left, Left -> Up (F/B不変)

### 3.3 ルービックキューブ記法との対応

R/U/F/L/D/B の記法は「どの層を動かすか」と「どの方向に回すか」を組み合わせて定義される。
コード上は `Domain.Cube.Enums.CubeOperation` がこの組み合わせをそのまま列挙し、内部で `CubeOperationRotation` が `RotateAxis` と `CubeTurn` に落とす。
理論的な対応表は次のとおり（L/R 等は **Pivot** と **向き**の組で表す）。

| 記法 | 対象層 | RotateAxis | CubeTurn |
|------|--------|-----------|---------|
| R    | Pivot より右側（X大）の層 | X | Clockwise |
| L    | Pivot より左側（X小）の層 | X | CounterClockwise |
| U    | Pivot より上側（Y大）の層 | Y | Clockwise |
| D    | Pivot より下側（Y小）の層 | Y | CounterClockwise |
| F    | Pivot より手前側（Z小）の層 | Z | Clockwise |
| B    | Pivot より奥側（Z大）の層 | Z | CounterClockwise |

`'`（プライム）は逆回転を表す（例: R' = X軸 CounterClockwise）。

**なぜ L が CounterClockwise か**: X軸時計回り（Clockwise）はPivotより右側（X大）のブロックを動かす定義である。
左側（X小）のブロックを「左面が時計回りに見える向きで」動かすには、X軸の逆方向（CounterClockwise）として表現することで整合が取れる。
Y軸・Z軸も同様の原則に従う。

## 4. 回転軸の動的制御 (Dynamic Pivot)

- **軸の選択**: プレイヤーの入力により、x軸上の隣り合うグリッド境界を回転軸として切り替える。
- **回転対象**: 選択された軸および Pivot 周辺に存在する全てのブロック。

## 5. 消去判定と更新処理

`Domain_Tetris.Field` / `LineClearUseCase` の実装に合わせる（詳細は `Domain_Tetris.md` §4）。

1. **行走査**: `y = MinY .. MaxY` を走査し、消去可能な行を列挙する。
2. **充填・配色条件（Z=Front のみ）**: 対象行 `y` について、`z = Field.MinZ`（プレイ面）の `x = MinX .. MaxX` がすべて埋まり、かつ `IBlock.GetColor(Front)` が 10 マス同一であること（`z = MaxZ` 側は判定に使わない）。
3. **消去実行**:
    - 条件を満たした **Y 行に存在する全セル**（`MinZ`〜`MaxZ` の両レイヤーを含む）をフィールドから除去する。
    - 残ブロックを、自分より下側に消えた行の本数だけ `Y` を減らす（`Z` は不変）。複数行同時消去にも対応する。
4. **接地（ロック）**: `LockMinoUseCase` は落下ミノのうち **絶対座標 `Z = Field.MinZ` のセルだけ**をフィールドに書き込む（奥行き側のセルはテトリス衝突・固定の対象外）。

## 6. 実装上の注意
- **Unity非依存の維持**: ドメインロジック（回転・消去判定等）は UnityEngine 名前空間に依存させないこと。
- **計算精度**: 独自の 3D座標クラス（BlockPosition, PivotPosition）を使用し、不適切な浮動小数点演算を排除する。
- **不変性の維持**: 回転操作時は新しい状態を生成して返す不変（Immutable）操作を基本とする。
- **プレイ面のみの判定**: 落下ミノの衝突判定・ライン消去の条件判定は **`Z = Field.MinZ`（Front）** のみを対象とする。ルービック回転でウェル外の Z にブロックがあってもテトリス判定からは除外する。