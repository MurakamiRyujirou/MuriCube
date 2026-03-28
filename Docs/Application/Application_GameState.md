# Docs/Application/Application_GameState.md

## 1. 概要

`GameState` はゲーム全体の**データ**を保持する不変レコード。フェーズのロジックは持たない。
各ユースケースは現在の `GameState` を受け取り、変更後の新しい `GameState` を返す。
Presentation 層は `ReactiveProperty<GameState>` を購読し、差分を View に反映する。

フェーズ管理（State パターン）は `IGamePhaseState` が担う。詳細は `Application_GamePhaseState.md` を参照。`ScramblingMoves` のライフサイクルは `Application_GamePhaseState_Scrambling.md` を参照。

## 2. 配置


| 項目   | 値                                         |
| ---- | ----------------------------------------- |
| ソース  | `Assets/Scripts/Application/GameState.cs` |
| 名前空間 | `Application`                             |
| 型    | `sealed record GameState`                 |


## 3. プロパティ定義


| プロパティ名             | 型             | 初期値           | 説明                  |
| ------------------ | ------------- | ------------- | ------------------- |
| `Field`            | `Field`       | `new Field()` | 接地済みブロックの配置         |
| `ActiveMino`       | `ActiveMino`  | `null`        | 操作中のミノ。未生成時は `null`（NRT 無効のため型に `?` は付けない） |
| `Score`            | `int`         | `0`           | 累積スコア               |
| `Level`            | `int`         | `0`           | 現在レベル               |
| `ClearedLineCount` | `int`         | `0`           | 累積消去ライン数（Level 計算用） |
| `IsGameOver`       | `bool`        | `false`       | ゲームオーバーフラグ          |
| `ScramblingMoves`  | `IReadOnlyList<ScramblingMove>` | 空リスト | スポーン直後のスクランブル手順。再生完了後は空に戻す |


`ActiveMino` は C# 上は `ActiveMino` 型だが、NRT 無効のため **未生成は `null`** を渡す（`GameState.Initial` と同様）。

## 4. 初期状態

```csharp
public static GameState Initial => new GameState(
    Field: new Field(),
    ActiveMino: null,
    Score: 0,
    Level: 0,
    ClearedLineCount: 0,
    IsGameOver: false,
    ScramblingMoves: Array.Empty<ScramblingMove>()
);
```

## 5. スコア計算

`GameDesign.md` §5.2 に準拠する。


| 消去ライン数 | スコア加算                |
| ------ | -------------------- |
| 1      | `40 × (Level + 1)`   |
| 2      | `100 × (Level + 1)`  |
| 3      | `300 × (Level + 1)`  |
| 4      | `1200 × (Level + 1)` |


計算は `LineClearUseCase` が担い、結果を `Score` に加算した新しい `GameState` を返す。

## 6. レベル計算

- `ClearedLineCount` の累積が `10 × (Level + 1)` に達したら `Level` を +1 する。
- レベルアップのタイミングは `LineClearUseCase` が判定する。

## 7. 設計指針

- **データのみ**: フェーズのロジックや遷移は持たない。`IGamePhaseState` に委ねる。
- **不変性**: `record` の `with` 構文を使い、常に新しいインスタンスを生成する。
- **単一の真実**: 全データをこの 1 レコードに集約し、Presentation 層が分散して状態を持たないようにする。
- **UnityEngine 非依存**: `UnityEngine` を参照しない純粋な C# とする。

