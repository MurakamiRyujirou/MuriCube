# Docs/Application/Application_GameState.md

## 1. 概要

`GameState` はゲーム全体の状態を保持する**不変レコード**。  
各ユースケースは現在の `GameState` を受け取り、変更後の新しい `GameState` を返す。  
Presentation 層は `ReactiveProperty<GameState>` を購読し、差分を View に反映する。

## 2. 配置

| 項目 | 値 |
|------|-----|
| ソース | `Assets/Scripts/Application/GameState.cs` |
| 名前空間 | `Application` |
| 型 | `sealed record GameState` |

## 3. プロパティ定義

| プロパティ名 | 型 | 初期値 | 説明 |
|------------|-----|--------|------|
| `Field` | `Field` | `new Field()` | 接地済みブロックの配置 |
| `ActiveMino` | `ActiveMino?` | `null` | 操作中のミノ。未生成時は `null` |
| `Score` | `int` | `0` | 累積スコア |
| `Level` | `int` | `0` | 現在レベル（消去ライン数で上昇） |
| `Phase` | `GamePhase` | `GamePhase.Spawning` | 現在のゲームフェーズ |
| `IsGameOver` | `bool` | `false` | ゲームオーバーフラグ |
| `ClearedLineCount` | `int` | `0` | 累積消去ライン数（Level 計算用） |

## 4. GamePhase（列挙型）

```csharp
public enum GamePhase
{
    Spawning,   // ミノを生成中
    Falling,    // ミノが落下・操作受付中
    LockDown,   // 接地後のロック猶予中
    Clearing,   // ライン消去処理中
    GameOver    // ゲームオーバー
}
```

- **配置**: `Assets/Scripts/Application/GamePhase.cs`
- **名前空間**: `Application`

## 5. 初期状態

```csharp
public static GameState Initial => new GameState(
    Field: new Field(),
    ActiveMino: null,
    Score: 0,
    Level: 0,
    Phase: GamePhase.Spawning,
    IsGameOver: false,
    ClearedLineCount: 0
);
```

## 6. スコア計算

`GameDesign.md` §5.2 に準拠する。

| 消去ライン数 | スコア加算 |
|------------|-----------|
| 1 | `40 × (Level + 1)` |
| 2 | `100 × (Level + 1)` |
| 3 | `300 × (Level + 1)` |
| 4 | `1200 × (Level + 1)` |

計算は `LineClearUseCase` が担い、結果を `Score` に加算した新しい `GameState` を返す。

## 7. レベル計算

- 消去ライン数の累積（`ClearedLineCount`）が `10 × (Level + 1)` に達したら `Level` を +1 する。
- レベルアップのタイミングは `LineClearUseCase` が判定する。

## 8. 設計指針

- **不変性**: `record` の `with` 構文を使い、常に新しいインスタンスを生成する。既存インスタンスを直接変更しない。
- **単一の真実**: 全フェーズ・全状態をこの 1 レコードに集約し、Presentation 層が分散して状態を持たないようにする。
- **UnityEngine 非依存**: `UnityEngine` を参照しない純粋な C# とする。