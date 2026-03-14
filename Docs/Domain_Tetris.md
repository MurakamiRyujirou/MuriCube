# Docs/Domain_Tetris.md

## 1. 概要
本ドキュメントは、MuriCubeにおける「パズルゲーム（テトリス風パズル）」としてのドメインモデルを定義する。
`Domain_Cube.md` で定義されたブロック集合をフィールド上に配置し、落下・接地・消去のライフサイクルを管理する。

## 2. 基底型（Value Objects / Enums）

### 2.1 CubePosition (Value Object)
- フィールド上の絶対座標を示す 3D 整数座標 (int X, int Y, int Z)。
- フィールドサイズ: X: 0～9, Y: 0～19, Z: 0～1（0: Front, 1: Back）。

### 2.2 MinoType (Enum)
落下するブロック集合の形状タイプ。
- I, O, S, Z, J, L, T

## 3. 主要エンティティ

### 3.1 PlayField (Entity)
接地が確定したブロックを静的に管理する空間。
- **データ構造**: `Dictionary<CubePosition, IBlock>`
- **責務**: 
    - 接地した `IBlock` 群を `CubePosition` に紐づけて永続化する。
    - 指定した行（Y座標）のブロック存在確認と消去判定。
    - ライン消去後の上部ブロックの落下（Y軸マイナス方向へのシフト）処理。

### 3.2 ActiveMino (Entity)
プレイヤーが現在操作している、落下中のオブジェクト。
- **データ構造**:
    - `MinoType` : 自身の形状タイプ。
    - `IBlockGroup` : 立体としての形状と配色データの抽象。
    - `CubePosition` : フィールド上での「基準位置（オフセット）」。
- **責務**:
    - **位置計算**: 自身の `CubePosition` と、内包する `IBlockGroup` の `BlockPosition` を加算してフィールド上の絶対座標を算出する。
    - **移動・回転命令の委譲**: プレイヤー入力に応じ、新しい `CubePosition` や、回転後の `IBlockGroup` を持つ自身の新しいインスタンスを生成する。
    - **衝突判定**: 移動・回転後の絶対座標がフィールド境界外であったり、`PlayField` 上の既存ブロックと重ならないかを検証する。

## 4. ゲーム固有ロジック：消去判定（Clear Condition）

ライン消去は、対象となる特定の Y 行において、以下の **2つの条件が両立（AND）** した時に実行される。

1. **充填条件（Filling Condition）**: 
   - $Z=0$ (Frontレイヤー) において、対象行の $X=0 \dots 9$ すべての座標に `IBlock` が存在すること。
2. **配色条件（Color Consistency）**:
   - 上記10個のブロックの **`IBlock.GetColor(BlockFace.Front)`** が、すべて同一であること。

※ $Z=1$ (Backレイヤー) のブロックは、消去判定には一切関与しない。ただし、消去後の落下処理には追従する。

## 5. 設計指針
- **Unityからの分離**: 座標計算、衝突判定、消去アルゴリズムはすべて純粋な C# ロジックとして記述する。
- **不変性の維持**: `ActiveMino` も `PlayField` も、状態更新時は常に新しいインスタンスを返す。
- **具象への非依存**: `ActiveMino` は `IBlockGroup` を通じて操作を行い、`Domain_Cube` の具象クラスを直接参照しない。