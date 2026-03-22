# Presentation.Views.BlockUIView

## 1. 概要

ブロックの見た目を担当する Presentation 層の View コンポーネント。  
ドメインモデル（`IBlock`）の配色情報を、6 面の `MeshRenderer` に反映する責務を持つ。  
`MonoBehaviour` を継承し、Inspector から各面の Renderer と色用 Material を割り当て可能にする。

## 2. 配置

| 項目 | 値 |
|------|-----|
| **パス** | `Assets/Scripts/Presentation/Views/Gameplay/BlockUIView.cs` |
| **名前空間** | `Presentation.Views.Gameplay` |
| **基底クラス** | `MonoBehaviour` |

## 3. 構造

### 3.1 Face Renderers（面ごとの MeshRenderer）

6 面それぞれに対応する `MeshRenderer` をシリアライズし、Inspector で個別に割り当てる。  
面ごとにマテリアルを差し替え可能にするため、各面用の参照を保持する。

| フィールド名 | 型 | 説明 |
|-------------|-----|------|
| `_up` | `MeshRenderer` | 上面（Up） |
| `_down` | `MeshRenderer` | 下面（Down） |
| `_left` | `MeshRenderer` | 左面（Left） |
| `_right` | `MeshRenderer` | 右面（Right） |
| `_front` | `MeshRenderer` | 手前面（Front） |
| `_back` | `MeshRenderer` | 奥面（Back） |

- `[SerializeField]` で Inspector に公開する。
- `BlockFace` の各値と 1:1 で対応する。

### 3.2 Color Materials（色用マテリアル）

`BlockColor` の各色に対応する `Material` を保持する。  
`SetColor(BlockFace, BlockColor)` および `UpdateView(IBlock)` 実行時に、該当面の `MeshRenderer.material`（または `sharedMaterial`）を差し替えるために使用する。

| フィールド名 | 型 | 説明 |
|-------------|-----|------|
| `_materialRed` | `Material` | Red 用 |
| `_materialOrange` | `Material` | Orange 用 |
| `_materialWhite` | `Material` | White 用 |
| `_materialYellow` | `Material` | Yellow 用 |
| `_materialGreen` | `Material` | Green 用 |
| `_materialBlue` | `Material` | Blue 用 |

- `[SerializeField]` で Inspector に公開する。
- `BlockColor` の各値と 1:1 で対応する。

## 4. 公開メソッド

### 4.1 SetColor(BlockFace face, BlockColor color)

- **概要**: 指定した面（`face`）の表示色を、指定した色（`color`）のマテリアルに差し替える。
- **引数**:
  - `face`: 対象面（`BlockFace`）
  - `color`: 適用する色（`BlockColor`）
- **挙動**: `face` に対応する `MeshRenderer` に、`color` に対応する `Material` を設定する。

### 4.2 UpdateView(IBlock block)

- **概要**: ドメインモデル `IBlock` を受け取り、全 6 面の色を一括更新する。
- **引数**: `block` — 配色情報を持つ `IBlock` インスタンス（`Domain.Common`）。
- **挙動**:
  - 各 `BlockFace`（Up, Down, Left, Right, Front, Back）に対して `block.GetColor(face)` で色を取得する。
  - 取得した色を、対応する面の Renderer に `SetColor(face, color)` と同等の処理で反映する。

## 5. 初期化・検証（Awake）

`Awake` で以下を実行する。

- **Face Renderers**: 6 面すべての `MeshRenderer` が割り当て済みであることを検証する。  
  未割り当て（`null`）の面がある場合は、`MissingReferenceException` をスローする。
- **Color Materials**: 各色（Red, Orange, White, Yellow, Green, Blue）の `Material` が割り当て済みであることを同様に検証する。  
  未割り当ての色がある場合も `MissingReferenceException` をスローする。

例外メッセージの例（右面が未割り当ての場合）:

```csharp
if (_right == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: Right is not assigned.");
```

同様に、各 Face Renderer および各 Color Material に対して、識別しやすい名前（例: `Right`, `MaterialRed`）を含めたメッセージで未割り当てチェックを行うこと。

## 6. 依存

- **Domain**: `Domain.Common.IBlock`, `Domain.Common.Enums.BlockFace`, `Domain.Common.Enums.BlockColor` を参照する。
- **Unity**: `UnityEngine.MonoBehaviour`, `UnityEngine.MeshRenderer`, `UnityEngine.Material` を使用する。
- **アーキテクチャ**: Presentation 層のため、ドメインのインターフェースと Enum のみに依存し、Application 層や他 View の具象には依存しない。

## 7. 備考

- マテリアルの差し替えでインスタンスを生成するか、共有マテリアルを使うかは実装で決定する（ランタイムで色を変えるだけなら `sharedMaterial` の差し替えでも可。ただし他オブジェクトとの共有に注意）。
- `UpdateView` は、`IBlock` が不変で状態が切り替わるたびに新しいインスタンスが渡される設計を前提とする。
