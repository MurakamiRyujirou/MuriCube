# Docs/Domain_Tetris.md

## 1. 概要
本ドキュメントは、MuriCubeにおける「パズルゲーム（テトリス風パズル）」としてのドメインモデルを定義する。
`Domain_Cube.md` で定義されたブロック集合をフィールド上に配置し、落下・接地・消去のライフサイクルを管理する。

## 2. 基底型（Value Objects / Enums）

### 2.1 CubePosition (Value Object)
- フィールド上の絶対座標を示す 3D 整数座標 (int X, int Y, int Z)。
- フィールドサイズ（**ウェルの有効範囲**）: X: 0～9, Y: 0～19, Z: 0～1（0: Front, 1: Back）。
- **実装**: `Domain.Tetris.CubePosition`（`readonly struct`）。保持する値自体には範囲制約を課さず、落下中のミノなど範囲外の座標も表現しうる。ウェル内かどうかは `Field.Contains` で判定する。

### 2.2 MinoType (Enum)
落下するブロック集合の形状タイプ。
- I, O, S, Z, J, L, T
- **実装**: `Domain.Tetris.MinoType`

## 3. 主要エンティティ

### 3.1 Field (Entity)
接地が確定したブロックを静的に管理する空間。
- **実装**: `Domain.Tetris.Field`（`Assets/Scripts/Domain/Tetris/Field.cs`）。
- **データ構造**: `Dictionary<CubePosition, IBlock>`（外部には `IReadOnlyDictionary<CubePosition, IBlock> Blocks` で公開）。
- **ウェル境界**: `MinX`～`MaxX`、`MinY`～`MaxY`、`MinZ`～`MaxZ` の `const` で定義。`static bool Contains(CubePosition)` は**ウェルの形状（静的ルール）**に座標が収まるかのみを判定する。**セルが埋まっているか**はインスタンスの `TryGetBlock` 等で問い合わせる。`ActiveMino` の衝突判定でもこの分担（境界は `Contains`、占有はフィールド状態）に合わせる。
- **不変更新**:
    - `WithCell` : ウェル内かつ非 null の `IBlock` を配置した新しい `Field` を返す。範囲外・`null` は例外。
    - `WithoutCell` : 指定座標のセルを除いた新しい `Field` を返す（該当セルが空でも可）。
    - `ClearCompletedLines` : 下記「消去の実行」を適用した新しい `Field` を返す。消去対象が一つもない場合は実装として同一インスタンスを返してよい。
- **責務（補足）**: 
    - 接地した `IBlock` 群を `CubePosition` に紐づけて永続化する。
    - 指定した **Y 行**の消去可否判定（`IsLineClearable`）。仕様は §4。
    - ライン消去の**実行**と、その後の**落下**（Y 軸マイナス方向へのシフト）。複数の Y が同時に消去対象なら、一度にまとめて適用する。消去された行のリストは `MinY`→`MaxY` の順で走査して構築し、**昇順**であることを利用して落下量（より下の消去行の本数）を計算する。

### 3.2 ActiveMino (Entity)
プレイヤーが現在操作している、落下中のオブジェクト。
詳細は `Docs/Domains/Domain_Tetris_ActiveMino.md` を参照。
- **データ構造**:
    - `MinoType` : 自身の形状タイプ。
    - `IBlockGroup` : 立体としての形状と配色データの抽象。
    - `CubePosition` : フィールド上での「基準位置（オフセット）」。
- **責務**:
    - **位置計算**: 自身の `CubePosition` と、内包する `IBlockGroup` の `BlockPosition` を加算してフィールド上の絶対座標を算出する。
    - **移動・回転命令の委譲**: プレイヤー入力に応じ、新しい `CubePosition` や、回転後の `IBlockGroup` を持つ自身の新しいインスタンスを生成する。
    - **衝突判定**: 移動・回転後の絶対座標がフィールド境界外であったり、`Field` 上の既存ブロックと重ならないかを検証する。

## 4. ゲーム固有ロジック：消去判定（Clear Condition）

ライン消去は、対象となる特定の Y 行において、以下の **2つの条件が両立（AND）** した時に実行される。

1. **充填条件（Filling Condition）**: 
   - $Z=0$ (Frontレイヤー) において、対象行の $X=0 \dots 9$ すべての座標に `IBlock` が存在すること。
2. **配色条件（Color Consistency）**:
   - 上記10個のブロックの **`IBlock.GetColor(BlockFace.Front)`** が、すべて同一であること。

※ $Z=1$ (Backレイヤー) のブロックは、消去判定には一切関与しない。

### 4.1 消去の実行と落下（実装確定）
- **除くセル**: ある Y が §4 の条件で消去対象になったとき、その **Y 行に存在する全セル**（すべての $X$ および $Z=0$・$Z=1$）をフィールドから取り除く。判定に使うのはあくまで $Z=0$ の10マスのみだが、**実行時は Back も同じ行ごと除去**する。
- **落下**: 残った各ブロックについて、「自分より**狭義に下**にある（$Y$ が小さい）消去済み行」の本数だけ $Y$ を減らす。$Z$ は変えない。これにより Back のブロックも上面と同じ段数だけ下に追従する。

## 5. 設計指針
- **Unityからの分離**: 座標計算、衝突判定、消去アルゴリズムはすべて純粋な C# ロジックとして記述する。
- **不変性の維持**: `ActiveMino` も `Field` も、状態更新時は新しいインスタンスを返すことを原則とする。変更がない場合に同一インスタンスを返してよい操作は §3.1 に従う。
- **具象への非依存**: `ActiveMino` は `IBlockGroup` を通じて操作を行い、`Domain_Cube` の具象クラスを直接参照しない。