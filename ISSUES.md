# MuriCube Development Issues

## [Task 001] Domain_Common の型定義 [x]
- **ステータス**: 完了 ✅
- **優先度**: 最高
- **概要**: `TechSpecs.md` に基づき、全レイヤーで共通利用する列挙型、バリューオブジェクト、インターフェースを定義する。
- **実装対象**:
    - `BlockColor`: キューブの6色 + Empty
    - `BlockFace`: Up/Down/Left/Right/Front/Back
    - `BlockPosition`: (x, y, z) の整数座標を扱う構造体
    - `IBlock`: 単一ブロックの正面の色を公開するインターフェース
    - `IBlockGroup`: ブロック集合体の抽象
- **完了条件**: 
    - `UnityEngine` に依存しない純粋な C# コードであること。
    - `Docs/Standards/CodingGuidelines.md` の命名規則を遵守していること。

## [Task 002] Domain 用 Assembly Definition の作成 [x]
- **ステータス**: 完了 ✅
- **概要**: `Assets/Scripts/Domain/` に `Domain.asmdef` を作成し、`No Engine References` を有効にする。
- **目的**: Domain層の独立性と純粋性をシステムレベルで強制し、コンパイル時間を最適化する。

## [Task 003] Domain_Cube の回転ロジック実装 [ ]
- **ステータス**: 待機中
- **概要**: 3D座標の回転および配色スワップの実装。
- **実装対象**:
    - `Cube`: IBlock を継承したクラスの実装。
    - `Rotate`: 軸指定による面の配色スワップアルゴリズム。