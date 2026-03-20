# MuriCube Development Issues

## [Task 001] Domain_Common の型定義 [x]
- **ステータス**: 完了 ✅
- **優先度**: 最高
- **概要**: `TechSpecs.md` に基づき、全レイヤーで共通利用する列挙型、バリューオブジェクト、インターフェースを定義する。
- **実装対象**:
    - `BlockColor`: キューブの6色 + Empty
    - `BlockFace`: Up/Down/Left/Right/Front/Back
    - `BlockPosition`: (x, y, z) の float 座標を扱う構造体（回転後は非整数を許容）
    - `IBlock`: 単一ブロックの正面の色を公開するインターフェース
    - `IBlockGroup`: ブロック集合体の抽象
- **完了条件**: 
    - `UnityEngine` に依存しない純粋な C# コードであること。
    - `Docs/Standards/CodingGuidelines.md` の命名規則を遵守していること。

## [Task 002] Domain 用 Assembly Definition の作成 [x]
- **ステータス**: 完了 ✅
- **概要**: `Assets/Scripts/Domain/` に `Domain.asmdef` を作成し、`No Engine References` を有効にする。
- **目的**: Domain層の独立性と純粋性をシステムレベルで強制し、コンパイル時間を最適化する。

## [Task 003] Domain_Cube の回転ロジック実装 [x]
- **ステータス**: 完了 ✅
- **概要**: 3D座標の回転および配色スワップの実装。Enums をサブフォルダに整理するリファクタリングも完了。
- **実装対象**:
    - `Block`: IBlock を継承したクラスの実装。
    - `Rotate`: 軸指定による面の配色スワップアルゴリズム。

## [Task 004] Block の回転ユニットテスト実装 [x]
- **ステータス**: 完了 ✅
- **概要**: Domain.Tests アセンブリを作成し、Block の X/Y/Z 軸回転が TechSpecs.md 通りに色スワップを行うか NUnit で検証する。
- **完了条件**:
    - 世界標準配色（BOY配色）でのテスト通過。
    - Cube から Block へのリネーム完了。

## [Task 005] BlockGroup と Cube の実装 [ ]
- **ステータス**: 未着手
- **優先度**: 高
- **概要**: Block の集合を管理する BlockGroup と、それを保持して回転を担う Cube を実装する。
- **実装対象**:
    - `BlockGroup`: IBlockGroup を継承したクラス。Block の集合の管理に徹し、座標（BlockPosition）と Block の対応を保持する。Rotate は持たない。
    - `Cube`: BlockGroup を保持するクラス。Rotate を実装する（座標の回転行列変換と、各 Block の向き更新の組み合わせ）。
- **完了条件**:
    - BlockGroup が Block の集合として IBlockGroup を満たすこと。
    - Cube の Rotate により、複数の Block が相対位置を保ったまま 90度回転すること。
    - 回転後の各 Block の面の色（向き）が正しく更新されていること。