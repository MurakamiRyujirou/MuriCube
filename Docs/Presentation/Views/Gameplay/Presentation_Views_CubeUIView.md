# Presentation.Views.CubeUIView

## 1. 概要

ドメイン層の Cube ロジック（`IBlockGroup` / `Cube`）を Unity 上で視覚化し、DOTween を用いたアニメーション回転を行う Presentation 層の View コンポーネント。  
複数の `BlockUIView` を生成・管理し、ドメインで計算された「回転」を視覚的なアニメーションとして表現する。  
2x2x2 に限定せず、4x4x4 やテトリミノなど任意形状のブロック集合に対応する。

## 2. 参照ドキュメント

- **回転ロジックの正解**: `Docs/Domains/Domain_Cube.md`（公転・自転、Pivot、軸ごとの対象ブロック抽出ルール）
- **個別ブロック表示**: `Docs/Presentation/Views/Gameplay/Presentation_Views_BlockUIView.md`（色反映・UpdateView）

## 3. 責務

- **ブロックの生成・管理**: `IBlockGroup` または `Cube` のドメインモデルに基づき、必要な数だけ `BlockUIView` を `_blockPrefab` から生成し、`_blocksRoot` の子として配置する。
- **回転の視覚化**: ドメイン層で定義された「回転」（軸・方向・Pivot）に従い、対象ブロック群を一時的に `_pivot` の子にし、DOTween で `_pivot` を回転させることでアニメーションを再生する。
- **状態の同期**: アニメーション完了後、呼び出し側が `Refresh` を呼び出したときに、全ブロックの位置・向き・色を現在の `IBlockGroup` と一致させる（スナップ）。移動したブロックの View のひも付け更新には、ドメインの `Cube.GetPositionMap` が返す「旧座標→新座標」を渡す。
- **デバッグ補助（任意）**: Pivot 貫通の X/Y/Z 軸線（`LineRenderer`）やブロック座標ログを、テスト・検証用に公開する。

## 4. 配置

| 項目 | 値 |
|------|-----|
| **パス** | `Assets/Scripts/Presentation/Views/Gameplay/CubeUIView.cs` |
| **名前空間** | `Presentation.Views.Gameplay` |
| **基底クラス** | `MonoBehaviour` |

- **公開プロパティ**: `IsRotating` — 回転アニメーション実行中は `true`（多重入力防止のため外部から参照可能）。

## 5. 構造（SerializedFields）

| フィールド名 | 型 | 説明 |
|-------------|-----|------|
| `_visualRoot` | `Transform` | キューブ全体の基準点。ワールド座標系の原点やスケールの基準とする。 |
| `_pivot` | `Transform` | 回転アニメーション時に、対象ブロック群を一時的に子にする Transform。回転はこの Transform に DOTween で適用する。 |
| `_blocksRoot` | `Transform` | 非回転時（通常時）のブロックの親。生成した `BlockUIView` はここを親として配置する。 |
| `_blockPrefab` | `BlockUIView` | `BlockUIView` のプレハブ。`Build` 時にこのプレハブからインスタンスを生成する。 |
| `_pivotAxisLineMaterial` | `Material` | `SetPivotAxisLine` で使う軸線用マテリアル。未設定時は実行時に `Unlit/Color` 等から生成するフォールバックあり。 |

- 上記は `[SerializeField]` で Inspector に公開する。
- 非 Serialized の内部状態として `_positionToView`、`BlockUIView` のリスト、`LineRenderer`（軸線 3 本）、回転中フラグなどを保持する。
- `Awake` または初期化時に未割り当てチェックを行い、`null` の場合は `MissingReferenceException` をスローする。

## 6. アニメーション回転のアルゴリズム

TumiCube の手法を継承しつつ、Domain_Cube の「任意 Pivot」に対応するため、以下の手順で回転アニメーションを実行する。

### 6.1 対象の抽出

- **責務分離**: 回転対象の判定は **Domain 層**（`Cube.GetAffectedBlocks(axis, turn, pivot)`）で行う。View は「回る」と判断されたブロックの座標リスト（`IReadOnlyCollection<BlockPosition> affectedPositions`）を **呼び出し側から受け取る**。
- **入力**: `RotateAsync` の引数として `affectedPositions` を渡す。呼び出し側（例: テストコントローラ）は `cube.GetAffectedBlocks(axis, turn, pivot)` で取得したリストをそのまま渡す。
- **処理**: `affectedPositions` に含まれる座標に対応する `BlockUIView` を `_positionToView` から取り出し、それらを `_pivot` の子にしてアニメーションする。View は対象判定ロジックを持たず、モデルが「回る」と判断したブロックのみを回転させる役割に専念する。

### 6.2 Pivot の設置

- 引数で渡された `PivotPosition`（ドメイン座標）を、Unity のワールド座標に変換する。
  - 実装では `_visualRoot.TransformPoint(x, y, z)` を用い、`_visualRoot` の位置・回転・スケールを一括反映する。
- `_pivot` の `position` を、変換したワールド座標に設定する。
- `_pivot` の `rotation` は、各回転の開始時に **identity** にリセットする（累積回転を避ける）。

### 6.3 親子付け

- 6.1 で抽出した対象ブロック群の `Transform` を、`_pivot` の子にする。
- `SetParent(_pivot, worldPositionStays: true)` を用い、ワールド座標を保ったまま親子関係だけ変更する。

### 6.4 回転再生

- `CubeTurn` に応じた回転角度（度）を求める。
  - `Clockwise` → +90°
  - `CounterClockwise` → -90°
  - `HalfTurn` → 180°
- 回転軸は `RotateAxis`（X / Y / Z）に応じて、`Vector3.right` / `Vector3.up` / `Vector3.forward`（いずれも**ワールド軸**）に対応させる。
- **Z 軸のみ符号反転**: Unity の `forward` は奥向き（+Z）のため、正面（手前）から見た公式記法の向きと揃える目的で、`RotateAxis.Z` のときは上記角度を**反転**してからトゥイーンに渡す（ドメインの公転と見た目を一致させるための調整）。
- DOTween は `_pivot.DORotate(axisVector * angleDeg, duration, RotateMode.WorldAxisAdd)` とし、`Ease.OutQuad` を適用する（ワールド軸に対する加算回転）。
- 回転中は**回転中フラグ**を立て、他メソッドが割り込まないようにする（詳細は §8）。

### 6.5 完了処理

- 回転アニメーション終了後、対象だった全ブロックを `_pivot` から外し、`_blocksRoot` の子に戻す。`SetParent(_blocksRoot, worldPositionStays: true)` でよい。
- **スナップ**: `RotateAsync` 内ではスナップしない。呼び出し側は **アニメーション完了後**、まだドメインを更新していない**回転前**の `Cube` に対して `GetPositionMap(axis, turn, pivot)` を取得し、続けて `cube.Rotate(...)` により**回転後**の `Cube` / `IBlockGroup` を得て、その `BlockGroup` と先ほどの `positionMap` を `Refresh` に渡す（`CubeViewTestController` がこの順序）。これで位置・向き（`Quaternion.identity`）・色をドメインに完全一致させる。
- `affectedPositions` に対応する `BlockUIView` が 1 つも解決できない場合は、早期 return する（この場合も `finally` で回転中フラグは解除される）。
- 回転中フラグを解除する。

## 7. 公開メソッド

### 7.1 Build(IBlockGroup group)

- **概要**: 最初のブロック生成。`group.Blocks` の各 `BlockPosition` に対して `_blockPrefab` から `BlockUIView` を 1 つずつ生成し、`_blocksRoot` の子として配置する。
- **引数**: `group` — ブロックの配置と各ブロックの `IBlock` を保持する `IBlockGroup`。
- **挙動**:
  - 回転中（`_isRotating`）に呼ぶと `InvalidOperationException` をスローする。
  - 既存の `BlockUIView` はすべて破棄し、内部マップをクリアしてから生成する。
  - `group.Blocks` は **X → Y → Z の昇順**でソートしてから処理する（決定的な生成順）。
  - 各 `(BlockPosition pos, IBlock block)` についてプレハブをインスタンス化し、`transform.position = DomainToWorld(pos)`、`rotation = Quaternion.identity` とする。
  - 各インスタンスに対して `BlockUIView.UpdateView(block)` を呼び、色を反映する。
  - `BlockPosition` と `BlockUIView` の対応を `_positionToView` に保持し、以降の `RotateAsync` や `Refresh` で使用する。

### 7.2 RotateAsync(RotateAxis axis, CubeTurn turn, PivotPosition pivot, float duration, IReadOnlyCollection&lt;BlockPosition&gt; affectedPositions)

- **概要**: 指定した軸・方向・Pivot で回転アニメーションを再生する。非同期（UniTask）で完了を返す。回転対象は **呼び出し側**が Domain の `Cube.GetAffectedBlocks(axis, turn, pivot)` で取得し、`affectedPositions` として渡す。
- **引数**:
  - `axis`: 回転軸（X / Y / Z）
  - `turn`: 回転方向・角度（Clockwise / CounterClockwise / HalfTurn）
  - `pivot`: 回転の中心となる空間座標（ドメインの `PivotPosition`）
  - `duration`: アニメーション時間（秒）
  - `affectedPositions`: 回転対象となるブロックの座標リスト。Domain の `Cube.GetAffectedBlocks(axis, turn, pivot)` で取得すること。
- **戻り値**: `UniTask`（完了またはキャンセルまで待機可能）。
- **挙動**:
  - 回転中フラグが立っていれば `InvalidOperationException` をスローする（多重実行を防ぐ）。
  - `_positionToView` が空のとき、または `affectedPositions` が null / 空のときは、何もせず return する。
  - 6.1～6.5 の手順で、渡された `affectedPositions` に対応する `BlockUIView` を Pivot の子にし、DOTween 回転・親の戻しまで行う（**スナップは行わない**）。
  - **重要**: アニメーション完了後は、**回転前**の `Cube` で `GetPositionMap` → **続けて** `Rotate` した**後**の `IBlockGroup` と、その `positionMap` を渡して `Refresh` を呼び、見た目・内部マップをドメインと一致させる（順序を誤ると View の座標ひも付けが壊れる）。
  - 呼び出し側は必要に応じて `Cube.CanRotate` で衝突検証してから `RotateAsync` してよい（View は検証しない）。
- **依存**: UniTask を使用する。DOTween の Ease は `OutQuad`（実装値）。

### 7.3 SetPivotAxisLine(float pivotX, float pivotY, float pivotZ)

- **概要**: Pivot を通る X/Y/Z の 3 本の軸線を `LineRenderer` で表示する（`_visualRoot` の子として生成し、`_pivot` の子にはしない。そのため DOTween の Pivot 回転アニメーションでは軸線は動かない）。
- **挙動**: 初回呼び出しで 3 本の `LineRenderer` を生成。`_pivotAxisLineMaterial` が null なら実行時にマテリアルを用意する。各線は長さ `2 * PivotAxisLineHalfLength`（実装定数 2.5 を半長に使用）、幅約 0.03、赤色。

### 7.4 LogBlockPositions(string label)

- **概要**: `_positionToView` の各エントリについて、ドメイン座標とワールド `transform.position` を `Debug.Log` する（検証・デバッグ用）。

### 7.5 Refresh(IBlockGroup group, IReadOnlyDictionary&lt;BlockPosition, BlockPosition&gt; positionMap)

- **概要**: アニメーションなしで、全ブロックの位置・回転・色を、引数 `group` が表す現在のドメイン状態に同期させる。回転直後はブロックが**別座標に移動している**ため、`positionMap` で「どの View がどの新座標に付くか」を解決する。
- **引数**:
  - `group` — 現在のブロック配置と各 `IBlock` を表す `IBlockGroup`（**直前の回転の適用後**のドメイン状態。例: `_cube.Rotate(...)` の戻りの `BlockGroup`）。
  - `positionMap` — **旧座標 → 新座標**のマッピング。**その回転を適用する直前**の `Cube` に対して `GetPositionMap(同じ axis, turn, pivot)` を呼んだ戻り値を渡す。移動しなかったブロックはマップに含まれない。
- **挙動**:
  - 回転中に呼ぶと `InvalidOperationException`。`group` / `positionMap` が null なら `ArgumentNullException`。
  - `positionMap` から「新座標 → 旧座標」の逆引きを構築し、`group.Blocks` の各エントリについて、新座標に対応する `BlockUIView` を（移動があれば旧座標経由で）取得する。
  - 各ビューを `_blocksRoot` の子にし、`DomainToWorld(pos)` と `Quaternion.identity`、`UpdateView(block)` を適用する。
  - マップ化に成功したエントリだけを集め、キーを `group.Blocks` の各座標とした `_positionToView` を**新しい辞書で置き換える**（回転後スナップでは各ブロックの新座標がキーになる）。
- **用途**: `RotateAsync` 完了後のスナップが主。`positionMap` が空の場合、各 `BlockPosition` はマップを経由せず `_positionToView` を同一キーで引くため、**座標が変わっていない**同期（例: 色だけ更新）に利用できる。

## 8. 回転中の多重入力を防ぐフラグ

- 回転アニメーションの開始時に `_isRotating` を `true` にし、`try` / `finally` の `finally` で必ず `false` に戻す（例外時も解放される）。
- `RotateAsync` の先頭で、フラグが既に `true` なら `InvalidOperationException` をスローする。
- `Build` および `Refresh` も、回転中に呼ばれた場合は `InvalidOperationException` をスローする。

## 9. 注意事項・非ハードコード

- **形状の汎用性**: 2x2x2 固定ではなく、4x4x4 やテトリミノのような変形オブジェクトでも動作するようにする。ブロック数や Pivot の位置は引数とドメインの `IBlockGroup` から得て、固定のインデックスや「4 個だけ」といったハードコードを避ける。
- **座標系**: ドメインの `BlockPosition`（float。格子に沿った値が多い）、`PivotPosition`（float）と Unity の Transform の対応は、`DomainToWorld` が `_visualRoot.TransformPoint` で一箇所変換する。
- **依存**: Domain の `IBlockGroup`, `BlockPosition`, `IBlock`, `RotateAxis`, `CubeTurn`, `PivotPosition` を参照する。Application 層や他 View の具象には依存しない。

## 10. 備考

- ドメインの `Cube.Rotate` は「新しい Cube を返す」不変操作のため、回転後の状態は呼び出し側が保持する。View は `Refresh` でその状態を反映する。アニメーションとドメイン更新の典型的な流れは `CubeViewTestController`（`GetPositionMap` → `Rotate` → `Refresh`）を参照。
- DOTween の Kill やキャンセル（UniTask の CancellationToken）は現状の実装では未使用。ライフサイクル上必要になった場合に追加する。
