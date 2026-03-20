# MuriCube Development Issues

最終更新: 2026-03-20（実装状況に同期）

| Task | 題目 | 状態 |
|------|------|------|
| 001 | Domain_Common | ✅ |
| 002 | Domain.asmdef | ✅ |
| 003 | Block（回転スワップ） | ✅ |
| 004 | Domain.Tests・Block 回転テスト | ✅ |
| 005 | BlockGroup / Cube | ✅ |
| 006 | Domain_Tetris（Field ほか） | ✅ |
| 007 | Field ユニットテスト | ✅ |
| 008 | Cube 回転ユニットテスト（CubeTest） | ✅ |

---

## [Task 001] Domain_Common の型定義 [x]
- **ステータス**: 完了 ✅
- **優先度**: 最高
- **概要**: `Docs/Domains/TechSpecs.md` およびアーキテクチャに基づき、全レイヤーで共通利用する列挙型、バリューオブジェクト、インターフェースを定義する。
- **実装対象**:
    - `BlockColor`: 6色（Red, Blue, White, Yellow, Green, Orange）※ Empty は現状未追加
    - `BlockFace`: Up/Down/Left/Right/Front/Back
    - `BlockPosition`: (x, y, z) float（回転後の非整数を許容）
    - `IBlock`: `GetColor(BlockFace)` で面ごとの色を返す
    - `IBlockGroup`: ブロック集合体の抽象（`Blocks`）
- **完了条件**:
    - `UnityEngine` に依存しない純粋な C# であること。
    - `Docs/Standards/CodingGuidelines.md` に準拠していること。

## [Task 002] Domain 用 Assembly Definition の作成 [x]
- **ステータス**: 完了 ✅
- **概要**: `Assets/Scripts/Domain/Domain.asmdef` を作成し、`noEngineReferences` を有効にする。
- **目的**: Domain 層の独立性と純粋性をコンパイル時に強制する。

## [Task 003] Domain_Cube の回転ロジック実装 [x]
- **ステータス**: 完了 ✅
- **概要**: 3D 座標の公転（座標置換）と `Block` の面スワップ（自転）。Enums は `Cube/Enums/` に配置。
- **実装対象**:
    - `Block`: `IBlock` 実装、`Rotate(RotateAxis)` による 90° スワップ
    - `PivotPosition`, `RotateAxis`, `CubeTurn` など

## [Task 004] Block の回転ユニットテスト [x]
- **ステータス**: 完了 ✅
- **概要**: `Assets/Tests/Domain/Domain.Tests.asmdef` と `BlockTest.cs` により、`Block` の X/Y/Z 軸回転の配色スワップを NUnit で検証する。
- **完了条件**:
    - 標準6面配色での `BlockTest` 通過
    - Cube 实体パッケージ名の `Block` への整理（旧 Cube 単体ブロックの置き換え）

## [Task 005] BlockGroup と Cube の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: ブロック集合 `BlockGroup` と、回転・衝突検証・位置マップを担う `Cube`。
- **実装対象**:
    - `BlockGroup`: `IBlockGroup`。内部 `Dictionary<BlockPosition, IBlock>` を `Blocks` で公開。`IReadOnlyDictionary<BlockPosition, IBlock>` / `Block` 用コンストラクタ
    - `Cube`: `IBlockGroup` 実装（`Blocks` は `BlockGroup` に委譲）。`Rotate` → 新 `Cube`。`GetPositionMap` / `GetAffectedBlocks` / `CanRotate`
- **完了条件**:
    - View が `IBlockGroup` として `Cube` を受け取れること
    - `GetPositionMap` は **Rotate 前**の `Cube` で取得し、回転後のグループと `Refresh` で併用できること
    - `UnityEngine` 非依存。`Domain_Cube.md` に沿った境界レイヤー・Z 軸自転の `InvertTurn` など

## [Task 006] Domain_Tetris（Field / CubePosition）[x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `Docs/Domains/Domain_Tetris.md` に基づくテトリス風フィールドのドメイン。`Assets/Scripts/Domain/Tetris/`。
- **実装対象**:
    - `CubePosition`: フィールド絶対座標 `readonly struct`
    - `Field`: 接地セル管理、`Contains`（ウェル形状）、`WithCell` / `WithoutCell`、`IsLineClearable`、`ClearCompletedLines`
- **参照仕様**: `Domain_Tetris.md`（消去条件・Front のみ判定・同一 Y の Back も消去実行時に除去・落下シフト）

## [Task 007] Field ユニットテスト [x]
- **ステータス**: 完了 ✅
- **概要**: `Assets/Tests/Domain/FieldTest.cs` と `StubBlock.cs` により占有・消去判定・複数行落下を検証。
- **完了条件**: `FieldTest` が NUnit でオールグリーンであること

## [Task 008] Cube 回転ユニットテスト（CubeTest）[x]
- **ステータス**: 完了 ✅
- **概要**: `Assets/Tests/Domain/CubeTest.cs` により `BlockGroup` の公開形状、`Cube.Rotate` の X/Y/Z 公転・自転、4 回転復元、`GetAffectedBlocks` / ピボット挙動を検証。
- **備考**: Z 軸は `Domain_Cube.md`・実装（公転で `dz` 不変、自転 `InvertTurn`）にテスト期待値を合わせている

---

## 進行メモ（未イシュー化の候補）

- `ActiveMino`、`MinoType`（`Domain_Tetris.md` 3.2）
- Application 層ユースケース（ミノ生成・ロック・ライン処理のオーケストレーション）
- TechSpecs `BlockColor.Empty` の要否と `IBlock` 仕様の一本化
