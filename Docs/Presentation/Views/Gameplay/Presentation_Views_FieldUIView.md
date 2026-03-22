# Presentation.Views.FieldUIView

## 1. 概要

`FieldUIView` は Application 層の `GameState` を購読し、フィールドエリアの描画を担う Presentation 層の View コンポーネント。
積み上がったブロック（`Field`）と落下中のミノ（`ActiveMino`）の z=0 面を、正面カメラで平面的に表示する。
`GameState` が変化するたびにフィールド全体を再描画する。

## 2. 配置

| 項目 | 値 |
|------|-----|
| パス | `Assets/Scripts/Presentation/Views/Gameplay/FieldUIView.cs` |
| 名前空間 | `Presentation.Views.Gameplay` |
| 基底クラス | `MonoBehaviour` |

## 3. 構造（SerializedFields）

| フィールド名 | 型 | 説明 |
|-------------|-----|------|
| `_blockPrefab` | `BlockUIView` | ブロック1セル分のプレハブ。`BlockUIView` を持つ Cube メッシュ |
| `_fieldRoot` | `Transform` | 積み上がりブロックの親 Transform |
| `_activeMinoRoot` | `Transform` | 落下中ミノのブロックの親 Transform |
| `_cellSize` | `float` | 1セルのワールドサイズ（デフォルト: 1.0f） |

## 4. 初期化

### 4.1 フィールドのセル生成

`Awake` または `Initialize` 時に、フィールドの最大セル数（10×20=200個）分の `BlockUIView` をプールとして事前生成し、`_fieldRoot` の子として配置する。初期状態はすべて非アクティブ。

### 4.2 ActiveMino のセル生成

同様に、1ミノの最大セル数（z=0 のみで最大4セル）分の `BlockUIView` を事前生成し、`_activeMinoRoot` の子として配置する。初期状態はすべて非アクティブ。

## 5. 描画（Refresh）

`GameState` が更新されるたびに `Refresh(GameState)` を呼び出し、フィールド全体を再描画する。

### 5.1 Field の描画

1. プールのすべての `BlockUIView` を非アクティブにする
2. `gameState.Field.Blocks` をイテレートし、z=0 のセルのみを対象にする
3. 各セルについてプールから `BlockUIView` を1つ取り出し、以下を設定する
   - `transform.position`: `DomainToWorld(cubePosition)` で変換したワールド座標
   - `BlockUIView.UpdateView(iBlock)`: ブロックの色を反映する
   - `gameObject.SetActive(true)`: 表示する

### 5.2 ActiveMino の描画

1. ActiveMino 用プールのすべての `BlockUIView` を非アクティブにする
2. `gameState.ActiveMino` が `null` なら終了
3. `gameState.ActiveMino.BlockGroup.Blocks` をイテレートし、絶対座標を計算して z=0 のセルのみを対象にする
4. 各セルについてプールから `BlockUIView` を1つ取り出し、Field と同様に設定する

### 5.3 座標変換（DomainToWorld）

ドメインの `CubePosition`（int グリッド）を Unity のワールド座標に変換する。

```csharp
private Vector3 DomainToWorld(CubePosition pos)
{
    return new Vector3(pos.X * _cellSize, pos.Y * _cellSize, 0f);
}
```

Z は常に 0f（正面カメラに向いた平面表示）。

## 6. GameStateMachine との接続

`GameStateMachine.GameStateObservable` を R3 で購読し、`GameState` が変化するたびに `Refresh` を呼ぶ。

```csharp
_subscription = _stateMachine.GameStateObservable
    .Subscribe(state => Refresh(state))
    .AddTo(this);  // MonoBehaviour の Destroy 時に自動購読解除
```

`GameStateMachine` の参照は `[SerializeField]` で受け取るか、DI（VContainer）で注入する。

## 7. 設計指針

- **全体再描画**: `GameState` 変化のたびにフィールド全体を再描画する。テトリスの規模（10×20=200セル）では問題なし。
- **z=0 のみ表示**: フィールドエリアはテトリスの平面世界。z=1 のセルは表示しない。
- **ActiveMino の絶対座標計算**: `ActiveMino.BlockGroup.Blocks` の相対座標にオフセットを加算して絶対座標を求める。`ActiveMino.AbsolutePositions()` は `CubePosition` の列挙を返すが、対応する `IBlock` が必要なため `BlockGroup.Blocks` から直接計算する。
- **プールの使用**: `Instantiate` / `Destroy` を毎フレーム行わず、事前生成したプールを使い回す。