# MuriCube Development Issues

最終更新: 2026-03-21（Task 019 完了を反映）

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
| 009 | ActiveMino | ✅ |
| 010 | ActiveMino ユニットテスト | ✅ |
| 011 | Application 基盤（GameState / GamePhase / IGamePhaseState / GameStateMachine） | ✅ |
| 012 | MinoFactory | ✅ |
| 013 | MinoFactory ユニットテスト | ✅ |
| 014 | SpawnMinoUseCase | ✅ |
| 015 | SpawnMinoUseCase ユニットテスト | ✅ |
| 016 | MoveMinoUseCase | ✅ |
| 017 | MoveMinoUseCase ユニットテスト | ✅ |
| 018 | RotateMinoUseCase | ✅ |
| 019 | RotateMinoUseCase ユニットテスト | ✅ |

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

## [Task 009] ActiveMino の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: プレイヤーが操作する落下中のミノを表すエンティティ。`Docs/Domains/Domain_Tetris.md` §3.2 に基づく。
- **実装対象**:
    - `ActiveMino`: `MinoType`・`IBlockGroup`・`CubePosition`（オフセット）・`PivotPosition`（回転中心）を保持する `sealed class`
    - `Pivot`: 回転の中心座標を保持する `PivotPosition` プロパティ
    - `AbsolutePositions()`: 内包する `IBlockGroup.Blocks` の各 `BlockPosition` をオフセットに加算し、フィールド上の絶対座標（`CubePosition`）を列挙して返す
    - `WithOffset(CubePosition)`: オフセットを差し替えた新しい `ActiveMino` を返す（移動用・不変）
    - `WithBlockGroup(IBlockGroup)`: `IBlockGroup` を差し替えた新しい `ActiveMino` を返す（回転用・不変）
    - `WithPivot(PivotPosition)`: `Pivot` だけを差し替えた新しい `ActiveMino` を返す（不変）
    - `IsColliding(Field)`: `AbsolutePositions()` のいずれかが `Field.Contains` の範囲外、または `Field.TryGetBlock` で占有済みであれば `true`
- **完了条件**:
    - `UnityEngine` に依存しない純粋な C# であること
    - 不変設計（更新時は新インスタンスを返す）であること
    - `IBlockGroup` の具象（`Cube`）に依存せず、インターフェース経由のみで操作すること

## [Task 010] ActiveMino のユニットテスト [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `ActiveMino` の位置計算・移動・衝突判定を NUnit で検証する。
- **実装対象**: `Assets/Tests/Domain/ActiveMinoTest.cs`
- **テストケース**:
    - `AbsolutePositions_OffsetAdded`: オフセット加算で絶対座標が正しく計算されること
    - `WithOffset_ReturnsNewInstance`: 不変性の確認
    - `IsColliding_OutOfBounds_ReturnsTrue`: フィールド範囲外で `true` を返すこと
    - `IsColliding_OccupiedCell_ReturnsTrue`: 既存ブロックと重なると `true` を返すこと
    - `IsColliding_FreeCell_ReturnsFalse`: 空きセルでは `false` を返すこと
- **完了条件**: `ActiveMinoTest` が NUnit でオールグリーンであること

## [Task 011] Application 基盤の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: Application 層の基盤となるクラス群を実装する。`Docs/Application/Application_Overview.md`・`Application_GameState.md`・`Application_GamePhaseState.md` に基づく。
- **実装対象**:
    - `GameState`: ゲームデータを保持する `sealed record`。`Field`・`ActiveMino?`・`Score`・`Level`・`ClearedLineCount`・`IsGameOver` を持つ。`static GameState Initial` を提供する
    - `GamePhase`: フェーズ識別用列挙型（`Spawning` / `Falling` / `LockDown` / `Clearing` / `GameOver`）
    - `IGamePhaseState`: `GamePhase Phase` プロパティと `Execute(GameState) → (IGamePhaseState, GameState)` を持つインターフェース
    - 各フェーズの **スタブ実装**（`SpawningState` / `FallingState` / `LockDownState` / `ClearingState` / `GameOverState`）: 現時点では `Execute` が `(this, gameState)` を返すだけでよい。ロジックは後続タスクで実装する
    - `GameStateMachine`: `IGamePhaseState` を保持し `OnUpdate()` で遷移を管理する。`ReadOnlyReactiveProperty<GameState> GameStateObservable` を公開し、`Reset()` で初期状態に戻す
- **配置**:
    - `Assets/Scripts/Application/GameState.cs`
    - `Assets/Scripts/Application/PhaseStates/GamePhase.cs`
    - `Assets/Scripts/Application/PhaseStates/IGamePhaseState.cs`
    - `Assets/Scripts/Application/PhaseStates/SpawningState.cs` ほか各フェーズ
    - `Assets/Scripts/Application/GameStateMachine.cs`
- **完了条件**:
    - `UnityEngine` に依存しない純粋な C# であること
    - `GameStateMachine` が R3 の `ReactiveProperty<GameState>` を使用していること
    - 各フェーズのスタブが `IGamePhaseState` を正しく実装していること

## [Task 012] MinoFactory の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `MinoType` から `ActiveMino` を生成するファクトリ。`Docs/Application/Application_MinoFactory.md` に基づく。
- **実装対象**:
    - `MinoFactory`: `MinoType` を受け取り、対応する形状・初期配色・Pivot の `ActiveMino` を返す `static class`
    - 7種（I / O / S / Z / J / L / T）それぞれの `IBlockGroup` 定義（奥行き2層・標準配色）
    - Pivot は `Application_MinoFactory.md` §5 の定義に従う
    - スポーン位置はファクトリでは持たず、オフセット `(0,0,0)` で生成する。配置は `SpawnMinoUseCase` が担う
- **配置**: `Assets/Scripts/Application/MinoFactory.cs`
- **完了条件**:
    - 7種すべてのミノが生成できること
    - 生成された `ActiveMino` の `IBlockGroup` が奥行き2層（z=0・z=1）を持つこと
    - 生成された `ActiveMino` が正しい `Pivot` を持つこと
    - `UnityEngine` に依存しない純粋な C# であること
- **参照仕様**: `Docs/Application/Application_MinoFactory.md`

## [Task 013] MinoFactory ユニットテスト [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `MinoFactory` が 7 種すべてのミノを正しく生成することを NUnit で検証する。
- **実装対象**: `Assets/Tests/Application/MinoFactoryTest.cs`、`Assets/Tests/Application/Application.Tests.asmdef`
- **テストケース**:
    - `Create_ReturnsCorrectMinoType`: 7種すべてについて `MinoFactory.Create(type).MinoType == type` であること
    - `Create_HasTwoLayers`: 7種すべてについて生成された `IBlockGroup.Blocks` のキーに z=0 と z=1 の両方が含まれること
    - `Create_PivotIsCorrect`: I は `(1.5, 0.5, 0.5)`、O は `(0.5, 0.5, 0.5)`、T/S/Z/J/L は `(1.0, 0.5, 0.5)` であること
    - `Create_OffsetIsZero`: 7種すべてについてオフセットが `(0, 0, 0)` であること
    - `Create_AllTypes_NoException`: 7種すべてで例外が発生しないこと
- **完了条件**: `MinoFactoryTest` が NUnit でオールグリーンであること

## [Task 014] SpawnMinoUseCase の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: 新しいミノを生成しフィールド上部中央に配置するユースケース。`Docs/Application/UseCases/UseCase_SpawnMino.md` に基づく。
- **実装対象**:
    - `SpawnMinoUseCase`: `Execute(GameState, System.Random) → GameState` の `static class`
    - `MinoFactory.Create` で生成 → `Cube` に変換 → 20回ランダム回転 → スポーン位置にオフセット設定 → 衝突判定
    - スポーン位置: 回転後の `AbsolutePositions` の最小 X/Y/Z を基準に Guideline 目標（X=3, Y=18, Z=0）へ寄せ、ウェル内に収まるよう Clamp（`UseCase_SpawnMino.md` §5）
    - ランダム回転: X/Y/Z軸をランダム選択・`CubeTurn.Clockwise` で20回
    - 衝突あり → `IsGameOver = true` の `GameState` を返す
    - 衝突なし → `ActiveMino` をセットした `GameState` を返す
- **配置**: `Assets/Scripts/Application/UseCases/SpawnMinoUseCase.cs`
- **完了条件**:
    - `UnityEngine` に依存しない純粋な C# であること
    - 純粋関数（引数の `GameState` を変更しない）であること
    - `System.Random` を引数で受け取ること
- **参照仕様**: `Docs/Application/UseCases/UseCase_SpawnMino.md`

## [Task 015] SpawnMinoUseCase のユニットテスト [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `SpawnMinoUseCase` の動作を NUnit で検証する。
- **実装対象**: `Assets/Tests/Application/SpawnMinoUseCaseTest.cs`
- **テストケース**:
    - `Execute_SetsActiveMino`: 空のフィールドでミノが `GameState.ActiveMino` にセットされること
    - `Execute_ActiveMinoIsWithinField`: スポーン直後の絶対座標がフィールド範囲内であること
    - `Execute_GameOver_WhenFieldFull`: フィールド上部が埋まっている場合に `IsGameOver = true` になること
    - `Execute_DoesNotMutateOriginalState`: 元の `GameState` が変更されていないこと（不変性）
- **完了条件**: `SpawnMinoUseCaseTest` が NUnit でオールグリーンであること

## [Task 016] MoveMinoUseCase の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: プレイヤー入力によるミノの左右移動・下方向への1段落下を処理するユースケース。`Docs/Application/UseCases/UseCase_MoveMino.md` に基づく。
- **実装対象**:
    - `MoveDirection`: 移動方向を表す列挙型（`Left` / `Right` / `Down`）
    - `MoveMinoUseCase`: `Execute(GameState, MoveDirection) → GameState` の `static class`
    - 移動後に `IsColliding` が `true` の場合は元の `GameState` をそのまま返す
    - `Down` 方向への移動が失敗した場合も元の `GameState` を返す（接地判定は `LockMinoUseCase` が担う）
- **配置**:
    - `Assets/Scripts/Application/UseCases/MoveDirection.cs`
    - `Assets/Scripts/Application/UseCases/MoveMinoUseCase.cs`
- **完了条件**:
    - `UnityEngine` に依存しない純粋な C# であること
    - 純粋関数（引数の `GameState` を変更しない）であること
- **参照仕様**: `Docs/Application/UseCases/UseCase_MoveMino.md`

## [Task 017] MoveMinoUseCase のユニットテスト [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `MoveMinoUseCase` の動作を NUnit で検証する。
- **実装対象**: `Assets/Tests/Application/MoveMinoUseCaseTest.cs`
- **テストケース**:
    - `Execute_Left_MovesOffset`: 左移動でオフセットX が -1 されること
    - `Execute_Right_MovesOffset`: 右移動でオフセットX が +1 されること
    - `Execute_Down_MovesOffset`: 下移動でオフセットY が -1 されること
    - `Execute_Blocked_ReturnsOriginal`: 移動先が壁の場合に元の `GameState` を返すこと（`AreSame` で参照一致）
    - `Execute_NoActiveMino_ReturnsOriginal`: `ActiveMino` が `null` の場合に元の `GameState` を返すこと
- **完了条件**: `MoveMinoUseCaseTest` が NUnit でオールグリーンであること

## [Task 018] RotateMinoUseCase の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: プレイヤー入力によるミノの回転を処理するユースケース。`Docs/Application/UseCases/UseCase_RotateMino.md` に基づく。
- **実装対象**:
    - `RotateMinoUseCase`: `Execute(GameState, RotateAxis, CubeTurn) → GameState` の `static class`
    - `ActiveMino.BlockGroup` を `Cube` に変換し `Cube.Rotate` で回転する
    - 回転後に `IsColliding` が `true` の場合は元の `GameState` をそのまま返す
    - `Pivot` は `ActiveMino.Pivot` をそのまま使用する
- **配置**: `Assets/Scripts/Application/UseCases/RotateMinoUseCase.cs`
- **完了条件**:
    - `UnityEngine` に依存しない純粋な C# であること
    - 純粋関数（引数の `GameState` を変更しない）であること
- **参照仕様**: `Docs/Application/UseCases/UseCase_RotateMino.md`

## [Task 019] RotateMinoUseCase のユニットテスト [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `RotateMinoUseCase` の動作を NUnit で検証する。
- **実装対象**: `Assets/Tests/Application/RotateMinoUseCaseTest.cs`
- **テストケース**:
    - `Execute_Clockwise_ChangesBlockGroup`: 時計回り回転でブロックの配色が変化すること
    - `Execute_Blocked_ReturnsOriginal`: 回転後に衝突する場合は元の `GameState` を返すこと
    - `Execute_NoActiveMino_ReturnsOriginal`: `ActiveMino` が `null` の場合に元の `GameState` を返すこと
    - `Execute_FourRotations_RestoresOriginal`: 同じ軸で4回回転すると元の状態に戻ること
- **完了条件**: `RotateMinoUseCaseTest` が NUnit でオールグリーンであること

---

## 進行メモ（未イシュー化の候補）

- Task 020以降: 各ユースケース実装（DropMino / LockMino / LineClear）
- Application 層ユースケースのユニットテスト
- TechSpecs `BlockColor.Empty` の要否と `IBlock` 仕様の一本化