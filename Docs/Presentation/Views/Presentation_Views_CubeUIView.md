# Presentation.Views.CubeUIView

## 1. 概要

ドメイン層の Cube ロジック（`IBlockGroup` / `Cube`）を Unity 上で視覚化し、DOTween を用いたアニメーション回転を行う Presentation 層の View コンポーネント。  
複数の `BlockUIView` を生成・管理し、ドメインで計算された「回転」を視覚的なアニメーションとして表現する。  
2x2x2 に限定せず、4x4x4 やテトリミノなど任意形状のブロック集合に対応する。

## 2. 参照ドキュメント

- **回転ロジックの正解**: `Docs/Domains/Domain_Cube.md`（公転・自転、Pivot、軸ごとの対象ブロック抽出ルール）
- **個別ブロック表示**: `Docs/Presentation/Views/Presentation_Views_BlockUIView.md`（色反映・UpdateView）

## 3. 責務

- **ブロックの生成・管理**: `IBlockGroup` または `Cube` のドメインモデルに基づき、必要な数だけ `BlockUIView` を `_blockPrefab` から生成し、`_blocksRoot` の子として配置する。
- **回転の視覚化**: ドメイン層で定義された「回転」（軸・方向・Pivot）に従い、対象ブロック群を一時的に `_pivot` の子にし、DOTween で `_pivot` を回転させることでアニメーションを再生する。
- **状態の同期**: アニメーション完了後、または `Refresh` 呼び出し時に、全ブロックの位置・向き・色を現在の `IBlockGroup` と一致させる（スナップ）。

## 4. 配置

| 項目 | 値 |
|------|-----|
| **パス** | `Assets/Scripts/Presentation/Views/Gameplay/CubeUIView.cs` |
| **名前空間** | `Presentation.Views.Gameplay` |
| **基底クラス** | `MonoBehaviour` |

## 5. 構造（SerializedFields）

| フィールド名 | 型 | 説明 |
|-------------|-----|------|
| `_visualRoot` | `Transform` | キューブ全体の基準点。ワールド座標系の原点やスケールの基準とする。 |
| `_pivot` | `Transform` | 回転アニメーション時に、対象ブロック群を一時的に子にする Transform。回転はこの Transform に DOTween で適用する。 |
| `_blocksRoot` | `Transform` | 非回転時（通常時）のブロックの親。生成した `BlockUIView` はここを親として配置する。 |
| `_blockPrefab` | `BlockUIView` | `BlockUIView` のプレハブ。`Build` 時にこのプレハブからインスタンスを生成する。 |

- いずれも `[SerializeField]` で Inspector に公開する。
- `Awake` または初期化時に未割り当てチェックを行い、`null` の場合は `MissingReferenceException` をスローする。

## 6. アニメーション回転のアルゴリズム

TumiCube の手法を継承しつつ、Domain_Cube の「任意 Pivot」に対応するため、以下の手順で回転アニメーションを実行する。

### 6.1 対象の抽出

- **入力**: 回転軸（`RotateAxis axis`）、回転中心（`PivotPosition pivot`）、および現在表示しているブロック群とそのドメイン上の位置（`IBlockGroup` または内部で保持する `BlockPosition` との対応）。
- **処理**: `Domain_Cube.md` の公転ルールに従い、「回転軸に垂直な平面のうち、Pivot が含まれるレイヤー」に属するブロックを回転対象として特定する。
  - 例: X 軸回転で Pivot が `(3.0, 0.5, 0.5)` の場合、`BlockPosition.X == 3` であるブロックを対象とする（Pivot の軸座標を整数に丸めるなどして `BlockPosition` と比較する）。
  - 対象となる `BlockUIView` のリストを保持する。形状（2x2x2, 4x4x4, テトリミノ等）に依存せず、軸と Pivot から動的に判定する。

### 6.2 Pivot の設置

- 引数で渡された `PivotPosition`（ドメイン座標）を、Unity のワールド座標に変換する。
  - ドメインの座標系（x: 右+, y: 上+, z: 奥+）と Unity のワールド座標の対応はプロジェクトの慣例に従う（通常は 1:1 またはスケール係数あり）。
- `_pivot` の `position` を、変換したワールド座標に設定する。
- `_pivot` の `rotation` は、回転前は identity または現在の見た目と整合する状態にしておく。

### 6.3 親子付け

- 6.1 で抽出した対象ブロック群の `Transform` を、`_pivot` の子にする。
- `SetParent(_pivot, worldPositionStays: true)` を用い、ワールド座標を保ったまま親子関係だけ変更する。

### 6.4 回転再生

- `CubeTurn` に応じた回転角度を求める。
  - `Clockwise` → その軸の正方向から見て時計回りに 90°
  - `CounterClockwise` → -90°
  - `HalfTurn` → 180°
- 回転軸は `RotateAxis`（X / Y / Z）に応じて、Unity の `Vector3.right` / `Vector3.up` / `Vector3.forward` に対応させる。
- `_pivot` の現在の回転から、上記角度だけ回転した目標回転を計算し、DOTween（例: `DORotate` または `DOLocalRotate`）で `duration` 秒かけてアニメーションする。
- 回転中は**回転中の多重入力を防ぐフラグ**を立て、他メソッド（次の `RotateAsync` や `Build` の再実行）が割り込まないようにする。

### 6.5 完了処理

- 回転アニメーション終了後、対象だった全ブロックを `_pivot` から外し、`_blocksRoot` の子に戻す。`SetParent(_blocksRoot, worldPositionStays: true)` でよい。
- **スナップ**: 見た目とドメインの整合性をとるため、座標・向きをドメインの計算結果に合わせる。
  - **方式 A**: 呼び出し側が、アニメーション完了後に `Refresh(回転後の IBlockGroup)` を呼ぶ。`Refresh` 内で全ブロックの位置・回転・色を `IBlockGroup` に合わせてスナップする。
  - **方式 B**: `RotateAsync` の引数に「回転後の `IBlockGroup`」を受け取り、アニメーション完了時に内部で同じスナップ処理を行う。
- いずれにせよ、最終的には全 `BlockUIView` の位置・回転・色が、渡された `IBlockGroup` と一致していること。
- 回転中フラグを解除する。

## 7. 公開メソッド

### 7.1 Build(IBlockGroup group)

- **概要**: 最初のブロック生成。`group.Blocks` の各 `BlockPosition` に対して `_blockPrefab` から `BlockUIView` を 1 つずつ生成し、`_blocksRoot` の子として配置する。
- **引数**: `group` — ブロックの配置と各ブロックの `IBlock` を保持する `IBlockGroup`。
- **挙動**:
  - 既存の子ブロックがいる場合は削除するか、再利用方針を決める（仕様では「最初のブロック生成」のため、クリアしてから生成でよい）。
  - 各 `(BlockPosition pos, IBlock block)` について、プレハブをインスタンス化し、`pos` をワールド座標（または `_blocksRoot` からの相対座標）に変換して配置する。
  - 各インスタンスに対して `BlockUIView.UpdateView(block)` を呼び、色を反映する。
  - `BlockPosition` と `BlockUIView` の対応を内部で保持し、以降の `RotateAsync`（対象抽出）や `Refresh` で使用する。

### 7.2 RotateAsync(RotateAxis axis, CubeTurn turn, PivotPosition pivot, float duration)

- **概要**: 指定した軸・方向・Pivot で回転アニメーションを再生する。非同期（UniTask）で完了を返す。
- **引数**:
  - `axis`: 回転軸（X / Y / Z）
  - `turn`: 回転方向・角度（Clockwise / CounterClockwise / HalfTurn）
  - `pivot`: 回転の中心となる空間座標（ドメインの `PivotPosition`）
  - `duration`: アニメーション時間（秒）
- **戻り値**: `UniTask`（完了またはキャンセルまで待機可能）。
- **挙動**:
  - 回転中フラグが立っていれば、即時 return または `InvalidOperationException` などで多重実行を防ぐ。
  - 6.1～6.5 の手順で対象抽出・Pivot 設置・親子付け・DOTween 回転・完了処理（親子戻し・スナップ）を実行する。
  - **重要**: アニメーション完了後に、見た目とドメインの整合性をとるため、座標（および必要なら回転・色）をスナップする。スナップに必要な「回転後の状態」は、呼び出し側が `Refresh(回転後の IBlockGroup)` で渡すか、`RotateAsync` のオーバーロードで `IBlockGroup` を受け取るかは実装で選択してよい。
- **依存**: UniTask を使用する。DOTween の `SetEase` 等はプロジェクトの演出方針に合わせる。

### 7.3 Refresh(IBlockGroup group)

- **概要**: アニメーションなしで、全ブロックの位置・回転・色を、引数 `group` が表す現在のドメイン状態に同期させる。
- **引数**: `group` — 現在のブロック配置と各 `IBlock` を表す `IBlockGroup`。
- **挙動**:
  - 既存の `BlockUIView` の数・対応関係が `group` と一致している前提で、各 `BlockPosition` に対応する `BlockUIView` の `Transform` の position / rotation を、`group` の座標系に合わせて設定する。
  - 各ブロックの色は、対応する `IBlock` の `GetColor(BlockFace)` に基づき `BlockUIView.UpdateView(block)` で更新する。
  - 回転アニメーション中でない場合の「一瞬で状態を合わせる」用途のほか、`RotateAsync` 完了後に呼び出し側から呼ぶことでスナップを実現する。

## 8. 回転中の多重入力を防ぐフラグ

- 回転アニメーションの開始時にフラグ（例: `_isRotating`）を `true` にし、完了処理の最後で `false` に戻す。
- `RotateAsync` の先頭で、フラグが `true` の場合は新たな回転を開始せず、`UniTask.CompletedTask` を返すか、または `InvalidOperationException` をスローするなど、プロジェクト方針に合わせて扱う。
- `Build` や `Refresh` が回転中に呼ばれた場合の挙動（無視する / 待つ / エラーにする）もあわせて定義する。

## 9. 注意事項・非ハードコード

- **形状の汎用性**: 2x2x2 固定ではなく、4x4x4 やテトリミノのような変形オブジェクトでも動作するようにする。ブロック数や Pivot の位置は引数とドメインの `IBlockGroup` から得て、固定のインデックスや「4 個だけ」といったハードコードを避ける。
- **座標系**: ドメインの `BlockPosition`（整数）および `PivotPosition`（float）と Unity の Transform の対応は、`_visualRoot` の位置・スケールを考慮して一箇所で変換するようにし、仕様では「ワールド座標に変換」とだけ記載する。スケールが 1 でない場合の考慮も実装で行う。
- **依存**: Domain の `IBlockGroup`, `BlockPosition`, `IBlock`, `RotateAxis`, `CubeTurn`, `PivotPosition` を参照する。Application 層や他 View の具象には依存しない。

## 10. 備考

- ドメインの `Cube.Rotate` は「新しい Cube を返す」不変操作のため、回転後の状態は呼び出し側（Application 層）が保持する。View はその状態を `Refresh` や `RotateAsync` 完了時のスナップで反映する。
- DOTween の Kill やキャンセル（UniTask の CancellationToken）をどう扱うかは、実装時にプロジェクトのライフサイクルに合わせて決める。
