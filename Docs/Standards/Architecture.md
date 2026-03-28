# Docs/Standards/Architecture.md

## 1. アーキテクチャの基本方針
本プロジェクトは、Unityというプラットフォームへの依存を最小限に抑え、ロジックの再利用性とテスト容易性を高めるために**オニオンアーキテクチャ**を厳守する。

## 2. レイヤーの定義
依存方向は常に「外側から内側」の一方向とし、内側の層は外側の層について一切の知識を持たないこと。

### 2.1 Domain Layer (最内層)
- **役割**: ゲームの核となるルール、エンティティ、インターフェースの定義。
- **制約**: `UnityEngine` への参照を一切禁止する。純粋な C# ロジックのみで構成せよ。
- **構成**: `Domain_Common`, `Domain_Cube`, `Domain_Tetris`。

### 2.2 Application Layer
- **役割**: ゲームの進行管理、ユースケース（ミノの生成、移動、回転、落下、ライン消去、ゲームオーバー判定）の実行。
- **状態管理**: `GameState`（不変レコード）にゲームデータを集約し、`GameStateMachine` がフェーズ遷移を管理する。
- **通知**: `GameStateMachine.GameStateObservable`（R3 `ReadOnlyReactiveProperty<GameState>`）で状態変化を Presentation 層に通知する。
- **制約**: ドメイン層を操作するが、表示（View）に関する具体的な実装は持たない。`UnityEngine` への参照を禁止する。

### 2.3 Presentation / Infrastructure Layer (最外層)
- **役割**: Unityコンポーネントによる表示、入力検知、エフェクト再生。
- **制約**: ロジックを持たず、Application層からの状態通知を画面に反映させることに専念する。
- **構成**: 以下の§3で詳述する。

## 3. Presentation層の構成

### 3.1 カメラ構成（1台・単一3D空間）

画面は **キューブエリア** と **フィールドエリア** の2表示エリアに分かれるが、描画は **1台のカメラ**で同一3D空間全体を行う。両エリアはワールド座標の Y 軸方向で上下に分離して配置する（座標範囲・カメラ角度は `Docs/Presentation/UI_Layout.md` に従う）。

| エリア | 表示内容 |
|--------|--------|
| キューブエリア（上方） | `ActiveMino` を3D表示。落下しない固定表示 |
| フィールドエリア（下方） | 積み上がりブロックと `ActiveMino` の z=MinZ 面に相当する表示 |

詳細なレイアウト仕様は `Docs/Presentation/UI_Layout.md` を参照。

### 3.2 View の購読構造

両エリアの View は `GameStateMachine.GameStateObservable` を R3 で購読し、`GameState` の変化に応じて独立して再描画する。エリア間に直接の依存関係は持たない。

```
GameStateMachine.GameStateObservable
    ├── CubeUIView（キューブエリア）
    │       └── ActiveMino の全面配色を3D表示
    └── FieldUIView（フィールドエリア）
            ├── Field（積み上がりブロック）を表示
            └── ActiveMino のプレイ面側の色を表示
```

### 3.3 入力検知

Unity InputSystem を使用し、デバイスごとに独立した Detector クラスを設ける。

| クラス | 対象デバイス | 検知する操作 |
|--------|--------|--------|
| `CubeInputDetector` | iPhone タッチ | 楕円スワイプ（R/L/U/D）・角丸四角タップ（F/B） |
| `KeyboardInputDetector` | キーボード | R/U/F/L/D/B + Shift で逆回転・矢印キー・Space |
| `GamepadInputDetector` | ゲームパッド | 右ボタン・DPad で回転・左スティックで移動落下 |

各 Detector は検知した操作を Application 層のユースケース（`RotateMinoUseCase` / `MoveMinoUseCase` 等）に橋渡しする。Detector 自身はゲームロジックを持たない。

入力の詳細なキー・ボタン割り当ては `Docs/Presentation/UI_Layout.md` §5 を参照。

## 4. 通信ルール
- **疎結合の維持**: テトリスはブロック集合を `IBlockGroup` 越しに扱い、`Cube` の回転アルゴリズム具象には依存しない（回転中心など幾何の値型は `Domain_Cube` 名前空間を参照しうる。`TechSpecs.md` 参照）。
- **不変性**: 状態の更新は、常に新しいインスタンスを生成して返すこと（副作用の排除）。
- **単方向依存**: Presentation → Application → Domain の方向のみ依存を許可する。逆方向の参照は禁止する。