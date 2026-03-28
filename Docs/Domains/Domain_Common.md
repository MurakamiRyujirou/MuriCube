# Docs/Domain_Common.md

## 1. 概要
本プロジェクトの各ドメインで共有される基底型、列挙型、およびインターフェースを定義する。
本ドメインは他のドメインに依存せず、各ドメイン間の疎結合を実現するための接合部として機能する。

## 2. 共通型定義
以下の定義は、Domain_Cube および Domain_Tetris で共通して使用される。

### 2.1 BlockColor (Enum)
- White, Yellow, Red, Orange, Blue, Green

### 2.2 BlockFace (Enum)
- Up, Down, Left, Right, Front, Back

### 2.3 BlockPosition (Value Object)
- Unity非依存の 3D 座標 (float X, float Y, float Z)。回転軸が 0.5 刻みの場合は非整数を許容する。

## 3. インターフェース

### 3.1 IBlock
- **メソッド**:
    - `BlockColor GetColor(BlockFace face)` : 指定した面の色を返す。

### 3.2 IBlockGroup
- **プロパティ**:
    - `IReadOnlyDictionary<BlockPosition, IBlock> Blocks { get; }` : ブロックの配置集合。

## 4. 設計指針
- **参照の一方向性**: `Domain_Cube` は本ドキュメントのみ参照する。`Domain_Tetris` は本ドキュメントを主に参照しつつ、回転中心型として `Domain_Cube.PivotPosition` 等を参照しうる（`Domain_Cube` から `Domain_Tetris` への参照はない）。
- **具象の隠蔽**: テトリス側は IBlockGroup を通じて操作を行い、Cube 側の内部ロジック（面のスワップ等）には依存しない。