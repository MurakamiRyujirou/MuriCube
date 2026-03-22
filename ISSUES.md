# MuriCube Development Issues

最終更新: 2026-03-22（Task 041 実装完了）

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
| 020 | DropMinoUseCase の実装 | ✅ |
| 021 | DropMinoUseCase のユニットテスト | ✅ |
| 022 | LockMinoUseCase の実装 | ✅ |
| 023 | LockMinoUseCase のユニットテスト | ✅ |
| 024 | LineClearUseCase の実装 | ✅ |
| 025 | LineClearUseCase のユニットテスト | ✅ |
| 026 | SpawningState の実装 | ✅ |
| 027 | FallingState の実装 | ✅ |
| 028 | LockDownState の実装 | ✅ |
| 029 | ClearingState の実装 | ✅ |
| 030 | FieldUIView の実装 | ✅ |
| 031 | FieldUIView の動作確認 | ✅ |
| 032 | CubeUIController の実装 | ✅ |
| 033 | CubeUIController の動作確認 | ✅ |
| 034 | KeyboardInputDetector の実装 | ✅ |
| 036 | CubeInputDetector の実装 | ✅ |
| 037 | スクランブル演出の実装 | ✅ |
| 038 | スクランブル演出の動作確認 | ✅ |
| 039 | CubeOperation の実装 | ✅ |
| 040 | CubeOperation のユニットテスト | ✅ |
| 041 | GamepadInputDetector の実装 | ✅ |
| 042 | GamepadInputDetector の動作確認 | 未着手 |

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
    - `IsColliding(Field)`: **Z が `Field.MinZ` のセルのみ**判定。各対象セルが `Field.Contains` の範囲外、または `Field.TryGetBlock` で占有済みなら `true`（それ以外の Z はスキップ）
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
    - `Create_PivotIsCorrect`: I は `(1.5, 0.5, 0.5)`、O は `(0.5, 0.5, 0.5)`、T/S/Z/J/L は `(1.5, 0.5, 0.5)` であること
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

## [Task 020] DropMinoUseCase の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: ソフトドロップ・ハードドロップを処理するユースケース。`Docs/Application/UseCases/UseCase_DropMino.md` に基づく。
- **実装対象**:
    - `DropType`: ドロップ種別を表す列挙型（`Soft` / `Hard`）
    - `DropMinoUseCase`: `Execute(GameState, DropType) → GameState` の `static class`
    - `Soft`: Y-1 の移動を試みる。失敗した場合は元の `GameState` を返す（接地判定は `LockMinoUseCase` が担う）
    - `Hard`: 衝突しない限り Y-1 を繰り返し、最下段まで即座に落下させる
- **配置**:
    - `Assets/Scripts/Application/UseCases/DropType.cs`
    - `Assets/Scripts/Application/UseCases/DropMinoUseCase.cs`
- **完了条件**:
    - `UnityEngine` に依存しない純粋な C# であること
    - 純粋関数（引数の `GameState` を変更しない）であること
- **参照仕様**: `Docs/Application/UseCases/UseCase_DropMino.md`

## [Task 021] DropMinoUseCase のユニットテスト [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `DropMinoUseCase` の動作を NUnit で検証する。
- **実装対象**: `Assets/Tests/Application/DropMinoUseCaseTest.cs`
- **テストケース**:
    - `Execute_Soft_MovesOffsetDown`: ソフトドロップでオフセットY が -1 されること
    - `Execute_Soft_Blocked_ReturnsOriginal`: 下が埋まっている場合に元の `GameState` を返すこと
    - `Execute_Hard_LandsAtBottom`: ハードドロップで空のフィールドの最下段まで落下すること
    - `Execute_Hard_LandsOnBlock`: ハードドロップで既存ブロックの直上に着地すること
    - `Execute_NoActiveMino_ReturnsOriginal`: `ActiveMino` が `null` の場合に元の `GameState` を返すこと
- **完了条件**: `DropMinoUseCaseTest` が NUnit でオールグリーンであること

## [Task 022] LockMinoUseCase の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: 接地したミノをフィールドに固定するユースケース。`Docs/Application/UseCases/UseCase_LockMino.md` に基づく。
- **実装対象**:
    - `LockMinoUseCase`: `Execute(GameState) → GameState` の `static class`
    - `ActiveMino.AbsolutePositions()` の各座標に `ActiveMino.BlockGroup` の対応ブロックを `Field.WithCell` で配置する
    - z=1 のセルは配置せず破棄する（`GameDesign.md` §2「消滅レイヤー」仕様）
    - 配置後に `ActiveMino` を `null` にクリアする
- **配置**: `Assets/Scripts/Application/UseCases/LockMinoUseCase.cs`
- **完了条件**:
    - `UnityEngine` に依存しない純粋な C# であること
    - 純粋関数（引数の `GameState` を変更しない）であること
    - z=1 のセルが Field に配置されないこと
- **参照仕様**: `Docs/Application/UseCases/UseCase_LockMino.md`

## [Task 023] LockMinoUseCase のユニットテスト [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `LockMinoUseCase` の動作を NUnit で検証する。
- **実装対象**: `Assets/Tests/Application/LockMinoUseCaseTest.cs`
- **テストケース**:
    - `Execute_PlacesBlocksInField`: T型相当（z=0・z=1 各4セル）をオフセット (3,5,0) でロックし、z=0 の各絶対座標にブロックがあること
    - `Execute_Z1_NotPlaced`: 同形状で z=1 の絶対座標にブロックが無いこと
    - `Execute_ClearsActiveMino`: 固定後に `ActiveMino` が `null` になること
    - `Execute_NoActiveMino_ReturnsOriginal`: `ActiveMino` が `null` の場合に元の `GameState` を返すこと（`AreSame`）
- **完了条件**: `LockMinoUseCaseTest` が NUnit でオールグリーンであること

## [Task 024] LineClearUseCase の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: ライン消去・スコア更新・レベルアップを処理するユースケース。`Docs/Application/UseCases/UseCase_LineClear.md` に基づく。
- **実装対象**:
    - `LineClearUseCase`: `Execute(GameState) → GameState` の `static class`
    - `Field.ClearCompletedLines()` でライン消去を実行する
    - 消去ライン数を算出し `GameDesign.md` §5.2 のスコア計算式でスコアを加算する
    - `ClearedLineCount` を更新し、`10 × (Level + 1)` に達したら `Level` を +1 する
    - 消去ライン数が0の場合は `Field` 以外を変更せず返す
- **配置**: `Assets/Scripts/Application/UseCases/LineClearUseCase.cs`
- **完了条件**:
    - `UnityEngine` に依存しない純粋な C# であること
    - 純粋関数（引数の `GameState` を変更しない）であること
- **参照仕様**: `Docs/Application/UseCases/UseCase_LineClear.md`

## [Task 025] LineClearUseCase のユニットテスト [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `LineClearUseCase` の動作を NUnit で検証する。
- **実装対象**: `Assets/Tests/Application/LineClearUseCaseTest.cs`
- **テストケース**:
    - `Execute_ClearsLine_UpdatesField`: z=0 の1行を同色で埋め、実行後にその行が消えていること
    - `Execute_ScoreAdded_OneLine`: 1ラインで `40 × (Level + 1)` が加算されること（Level=0 なら +40）
    - `Execute_ScoreAdded_TwoLines`: 2ライン同時で `100 × (Level + 1)` が加算されること
    - `Execute_LevelUp`: Level=0・`ClearedLineCount=9` から1ライン消去で Level が 1 になること
    - `Execute_NoLine_ReturnsUnchanged`: 消去対象なしで Score・Level・`ClearedLineCount` が変化しないこと
- **完了条件**: `LineClearUseCaseTest` が NUnit でオールグリーンであること

## [Task 026] SpawningState の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: ミノを生成して `FallingState` へ遷移するフェーズ。`Docs/Application/Application_GamePhaseState.md` §3 に基づく。
- **実装対象**:
    - `SpawningState.Execute`: `SpawnMinoUseCase.Execute` を呼び出し、`IsGameOver` なら `(new GameOverState(), newGameState)` を返す。正常生成なら `(new FallingState(_random), newGameState)` を返す
    - `System.Random` は `SpawningState` のコンストラクタで受け取り `readonly` 保持する
- **配置**: `Assets/Scripts/Application/PhaseStates/SpawningState.cs`
- **完了条件**:
    - スポーン成功時に `FallingState` へ遷移すること
    - ゲームオーバー時に `GameOverState` へ遷移すること
    - `UnityEngine` に依存しない純粋な C# であること
- **参照仕様**: `Docs/Application/Application_GamePhaseState.md`

## [Task 027] FallingState の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: 自然落下タイマーを管理し、接地で `LockDownState` へ遷移するフェーズ。`Docs/Application/Application_GamePhaseState.md` §3 に基づく。
- **実装対象**:
    - `_timer` を `deltaTime` で進め、間隔 `GetFallInterval(Level)`（`Math.Max(0.1f, 1.0f - level * 0.1f)`）未満なら `(this, gameState)`
    - 間隔到達で `_timer -= interval` のあと `DropMinoUseCase.Execute(gameState, DropType.Soft)`
    - 戻り値が `ReferenceEquals` で元と同一なら `(new LockDownState(_random), gameState)`、そうでなければ `(this, newGameState)`
- **配置**: `Assets/Scripts/Application/PhaseStates/FallingState.cs`
- **完了条件**:
    - タイマーが落下間隔に達したら1段落下すること
    - 接地時に `LockDownState` へ遷移すること
    - `UnityEngine` に依存しない純粋な C# であること
- **参照仕様**: `Docs/Application/Application_GamePhaseState.md`

## [Task 028] LockDownState の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: 接地後の猶予時間を管理し、`LockMinoUseCase` → `LineClearUseCase` → `SpawningState` へ遷移するフェーズ。
- **実装対象**:
    - `_timer` と `LockDelay = 0.5f`。`_timer < LockDelay` の間は `(this, gameState)`
    - 猶予経過後: `LockMinoUseCase.Execute` → `LineClearUseCase.Execute` の順で実行し `(new SpawningState(_random), clearedState)` を返す
    - `System.Random` はコンストラクタで受け取り `SpawningState` に引き継ぐ
    - 猶予中の移動・回転でタイマーリセット→`FallingState` へ戻す処理は未実装（将来拡張・コード内コメント）
- **配置**: `Assets/Scripts/Application/PhaseStates/LockDownState.cs`
- **完了条件**:
    - 猶予時間後に `LockMino` → `LineClear` → `Spawning` の順で処理されること
    - `UnityEngine` に依存しない純粋な C# であること
- **参照仕様**: `Docs/Application/Application_GamePhaseState.md`

## [Task 029] ClearingState の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 中
- **概要**: ライン消去アニメーション用の待機フェーズ。現時点では即座に `SpawningState` へ遷移する。
- **実装対象**:
    - `System.Random` をコンストラクタで受け取り `readonly` 保持
    - `Execute`: 即座に `(new SpawningState(_random), gameState)` を返す（`deltaTime` は将来のアニメ待機用）
- **配置**: `Assets/Scripts/Application/PhaseStates/ClearingState.cs`
- **完了条件**:
    - 即座に `SpawningState` へ遷移すること
    - `UnityEngine` に依存しない純粋な C# であること
- **参照仕様**: `Docs/Application/Application_GamePhaseState.md`

## [Task 030] FieldUIView の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: フィールドエリアの平面表示を担う View コンポーネント。`Docs/Presentation/Views/Gameplay/Presentation_Views_FieldUIView.md` に基づく。
- **実装対象**:
    - `FieldUIView`: `MonoBehaviour`。`BlockUIView` のプールを持ち、`GameState` 変化時にフィールド全体を再描画する
    - フィールド用プール（最大200個）と ActiveMino 用プール（最大4個）を `Awake` 時に事前生成する
    - `Refresh(GameState)`: Field の z=0 セルと ActiveMino の z=0 セルをプールから取り出して配置する
    - `DomainToWorld(CubePosition)`: `pos.X * _cellSize`, `pos.Y * _cellSize`, `0f` でワールド座標に変換する
    - `Initialize(GameStateMachine)` で `GameStateObservable` を R3 で購読し、`Refresh` を呼ぶ。`IsGameOver` 時は購読解除
- **配置**: `Assets/Scripts/Presentation/Views/Gameplay/FieldUIView.cs`
- **完了条件**:
    - フィールドのブロックが z=0 のみ平面表示されること
    - ActiveMino が落下に応じてリアルタイムで更新されること
    - ゲームオーバー時に描画が止まること
- **参照仕様**: `Docs/Presentation/Views/Gameplay/Presentation_Views_FieldUIView.md`

## [Task 031] FieldUIView の動作確認 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `FieldUIView` を Unity シーンに配置し、ゲームループと接続して動作確認する。
- **確認内容**:
    - ミノがスポーンして落下する様子が画面に表示されること
    - ロック後にブロックがフィールドに残ること
    - ゲームオーバー時に `IsGameOver` が `true` になり描画が止まること
- **完了条件**: Unity エディタの Play モードで上記が目視確認できること

## [Task 032] CubeUIController の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `GameStateMachine` と `CubeUIView` を接続し、ActiveMino のスポーン時に `Build` を呼び、回転入力を `RotateMinoUseCase` → `CubeUIView.RotateAsync` → `Refresh` の順で処理する `MonoBehaviour`。
- **実装対象**:
    - `CubeUIController`: `MonoBehaviour`
    - `Initialize(GameStateMachine)`: `GameStateObservable` を購読し、`ActiveMino` が切り替わったとき（前回と異なる `MinoType` またはスポーン直後）に `CubeUIView.Build(activeMino.BlockGroup)` を呼ぶ
    - `ExecuteRotate(RotateAxis, CubeTurn)`: `RotateMinoUseCase.Execute` → `CubeUIView.RotateAsync` → `Cube.GetPositionMap` → `Cube.Rotate` → `CubeUIView.Refresh` の順で処理する
    - `OnDestroy` で購読を解除する
- **配置**: `Assets/Scripts/Presentation/Views/Gameplay/CubeUIController.cs`
- **完了条件**:
    - スポーン時に `CubeUIView` にミノが表示されること
    - `ExecuteRotate` 呼び出しで回転アニメーションが再生されること
    - `UnityEngine` 以外の依存は Application・Domain 層のインターフェース経由であること

## [Task 033] CubeUIController の動作確認 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `CubeUIController` を Unity シーンに配置し、キューブエリアとフィールドエリアが同期して動作することを確認する。
- **確認内容**:
    - スポーン時にキューブエリアにミノが表示されること
    - フィールドエリアの落下中ミノと同じ `ActiveMino` が表示されていること
    - ゲームオーバー時に両エリアの描画が止まること
- **完了条件**: Unity エディタの Play モードで上記が目視確認できること

## [Task 034] KeyboardInputDetector の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: キーボード入力を検知し、回転・移動・落下操作を `CubeUIController` / `MoveMinoUseCase` / `DropMinoUseCase` に橋渡しする `MonoBehaviour`。
- **実装対象**:
    - `KeyboardInputDetector`: `MonoBehaviour`
    - R/U/F/L/D/B キー → 対応する `RotateAxis` / `CubeTurn` で `CubeUIController.ExecuteRotateAsync` を呼ぶ
    - Shift + 対応キー → 逆回転（`CubeTurn.CounterClockwise`）
    - ← → → `MoveMinoUseCase.Execute` で左右移動し `GameStateMachine.ApplyGameState`
    - ↓ → `DropMinoUseCase.Execute(DropType.Soft)` で `ApplyGameState`
    - Space → `DropMinoUseCase.Execute(DropType.Hard)` で `ApplyGameState`
- **配置**: `Assets/Scripts/Presentation/InputDetectors/KeyboardInputDetector.cs`
- **参照仕様**: `Docs/Presentation/UI_Layout.md` §6.2

## [Task 036] CubeInputDetector の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 中
- **概要**: iPhoneタッチ入力を検知し、キューブエリアの楕円・角丸四角パーツへのスワイプ・タップを回転操作に変換する `MonoBehaviour`。
- **実装対象**:
    - `CubeInputDetector`: `MonoBehaviour`
    - 楕円パーツへのスワイプ → R/L/U/D 回転
    - 角丸四角パーツへのシングルタップ → F/B 回転
    - 角丸四角パーツへのダブルタップ → F'/B' 逆回転
- **配置**: `Assets/Scripts/Presentation/InputDetectors/CubeInputDetector.cs`
- **参照仕様**: `Docs/Presentation/UI_Layout.md` §6.1

## [Task 037] スクランブル演出の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: ミノスポーン時に「整列状態で表示 → 高速スクランブル回転アニメーション → 落下開始」という演出を追加する。
- **実装内容**:
    - `SpawningState.Execute` を修正し、スポーン直後に `GamePhase.Scrambling` フェーズへ遷移する
    - `ScramblingState` を新規作成する（`IGamePhaseState` を実装）
    - `ScramblingState` はスクランブル回転リスト（`SpawnMinoUseCase` が生成した回転手順）を保持し、`CubeUIController` に順番に `ExecuteRotateAsync` を呼ばせて高速回転アニメーションを再生する
    - 全回転完了後に `FallingState` へ遷移する
    - スクランブル中はテトリス操作（移動・落下）を受け付けない
    - スクランブル中はルービックキューブ操作も受け付けない
- **設計上の課題**:
    - `ScramblingState`（Application層）から `CubeUIController`（Presentation層）を呼ぶことはできない
    - 解決策: `SpawnMinoUseCase` が生成したスクランブル回転リストを `GameState` に持たせ、Presentation層が `GameState.ScramblingMoves` を購読して演出を再生する
- **配置**:
    - `Assets/Scripts/Application/PhaseStates/ScramblingState.cs`
    - `GameState` に `ScramblingMoves` プロパティを追加
    - `SpawnMinoUseCase` にスクランブル手順の記録を追加
- **参照仕様**: `Docs/Application/Application_GamePhaseState.md`

## [Task 038] スクランブル演出の動作確認 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: スクランブル演出が正しく動作することを Unity エディタで目視確認する。
- **確認内容**:
    - ミノスポーン時に整列状態で表示されること
    - 高速回転アニメーションが順番に再生されること
    - 全回転完了後に落下が開始されること
    - スクランブル中に操作が無効になること
- **完了条件**: Unity エディタの Play モードで上記が目視確認できること

## [Task 039] CubeOperation の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: ルービックキューブの12操作（R/Ri/L/Li/U/Ui/D/Di/F/Fi/B/Bi）を表す列挙型と、`RotateAxis`+`CubeTurn` への変換拡張メソッドを実装する。
- **実装対象**:
    - `CubeOperation`: `Domain.Cube.Enums` に追加する列挙型
    - `CubeOperationExtensions`: `ToAxisAndTurn(this CubeOperation op)` 拡張メソッドを持つ静的クラス。`Presentation` 名前空間に配置（`CubeUIController` から使用）
    - `SwipeConfig` / `TapConfig` / `CubeRotation` を `CubeOperation` ベースに変更する
    - `CubeInputDetector.OnRotationDetected` を `CubeOperation` 経由で `ExecuteRotateAsync` を呼ぶように変更する
    - `KeyboardInputDetector` の各回転ハンドラも `CubeOperation` 経由に変更する
- **配置**:
    - `Assets/Scripts/Domain/Cube/Enums/CubeOperation.cs`
    - `Assets/Scripts/Presentation/CubeOperationExtensions/CubeOperationExtensions.cs`（`Presentation.CubeOperationExtensions.asmdef`）
- **完了条件**:
    - `UnityEngine` に依存しない純粋な C# であること
    - コンパイルエラーが出ないこと

## [Task 040] CubeOperation のユニットテスト [x]
- **ステータス**: 完了 ✅
- **優先度**: 高
- **概要**: `CubeOperationExtensions.ToAxisAndTurn` の変換結果を NUnit で検証する。
- **実装対象**: `Assets/Tests/Domain/CubeOperationExtensionsTest.cs`（`Domain.Tests.asmdef` が `Presentation.CubeOperationExtensions` を参照）
- **テストケース**:
    - `R_Returns_X_Clockwise` / `Ri_Returns_X_CounterClockwise`
    - `L_Returns_X_CounterClockwise` / `Li_Returns_X_Clockwise`
    - `U_Returns_Y_Clockwise` / `Ui_Returns_Y_CounterClockwise`
    - `D_Returns_Y_CounterClockwise` / `Di_Returns_Y_Clockwise`
    - `F_Returns_Z_Clockwise` / `Fi_Returns_Z_CounterClockwise`
    - `B_Returns_Z_CounterClockwise` / `Bi_Returns_Z_Clockwise`
- **完了条件**: `CubeOperationExtensionsTest` を含むプロジェクトのテストがオールグリーンであること

## [Task 041] GamepadInputDetector の実装 [x]
- **ステータス**: 完了 ✅
- **優先度**: 中
- **概要**: ゲームパッド入力を検知し、回転・移動・落下操作を `CubeUIController` / `MoveMinoUseCase` / `DropMinoUseCase` に橋渡しする `MonoBehaviour`。`KeyboardInputDetector` と同じ構造で入力デバイスのみ異なる。
- **実装対象**:
    - `GamepadInputDetector`: `MonoBehaviour`
    - `[SerializeField] InputActionReference` で各アクションを個別に持つ
      - 回転: `_rotateRAction` / `_rotateUAction` / `_rotateFAction` / `_rotateLAction` / `_rotateDAction` / `_rotateBAction`
      - 修飾: `_counterClockwiseModifierAction`
      - テトリス: `_moveLeftAction` / `_moveRightAction` / `_softDropAction` / `_hardDropAction`
    - `OnEnable` / `OnDisable` で各アクションを `EnableAndSubscribe` / `DisableAndUnsubscribe` で購読管理する
    - 各回転ハンドラは `CubeOperation` 経由で `_cubeUIController.ExecuteRotateAsync(operation).Forget()` を呼ぶ
    - テトリス操作は `MoveMinoUseCase` / `DropMinoUseCase` → `_stateMachine.ApplyGameState` で反映する
    - スクランブル中・ゲームオーバー時は入力を無視する（`TryConsumeGameplayInput` で判定）
    - `Initialize(GameStateMachine stateMachine)` で `_stateMachine` を受け取る
- **配置**: `Assets/Scripts/Presentation/InputDetectors/GamepadInputDetector.cs`
- **参照仕様**: `Docs/Presentation/UI_Layout.md` §6.3

## [Task 042] GamepadInputDetector の動作確認 [ ]
- **ステータス**: 未着手
- **優先度**: 中
- **概要**: ゲームパッドを接続して動作確認する。
- **確認内容**:
    - 各ボタンで対応する回転が実行されること
    - トリガーで逆回転になること
    - 左スティックで移動・ソフトドロップができること
    - Xボタンでハードドロップができること
    - スクランブル中に操作が無効になること
- **完了条件**: ゲームパッドで上記が目視確認できること

---

## 進行メモ（未イシュー化の候補）

- Task 043以降: ゲームオーバー画面・スコア表示
- ActiveMino落下中の表示欠け修正（1セル欠けるケースあり）
- TechSpecs `BlockColor.Empty` の要否と `IBlock` 仕様の一本化
- ドキュメントパスの整理（Docs/Domains/ と Docs/ の混在）