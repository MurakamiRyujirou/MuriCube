# Docs/Application/UseCases/UseCase_LineClear.md

## 1. 概要

`LineClearUseCase` はミノ固定後のライン消去・スコア更新・レベルアップを処理する。
`LockMinoUseCase` の直後に呼び出されることを前提とする。

## 2. 配置

| 項目 | 値 |
|------|-----|
| ソース | `Assets/Scripts/Application/UseCases/LineClearUseCase.cs` |
| 名前空間 | `Application.UseCases` |
| 型 | `static class LineClearUseCase` |

## 3. 公開API

```csharp
public static class LineClearUseCase
{
    // ライン消去・スコア更新・レベルアップを処理する。
    // 消去対象がない場合も Field を更新した GameState を返す。
    public static GameState Execute(GameState gameState);
}
```

## 4. 処理フロー

```
1. Field.ClearCompletedLines() でライン消去を実行し、新しい Field を得る
2. 消去ライン数 = 消去前の Field のブロック数 - 消去後の Field のブロック数
   ÷ Field の横幅（MaxX - MinX + 1 = 10）
   ※ z=0 のブロックのみフィールドに残っているため、この計算で正確に行数が求まる
3. 消去ライン数が 0 なら gameState with { Field = newField } を返す（スコア・レベル変化なし）
4. スコア加算額を §5 の計算式で求める
5. 新しい ClearedLineCount = 現在の ClearedLineCount + 消去ライン数
6. レベルアップ判定: 新しい ClearedLineCount >= 10 × (Level + 1) なら Level を +1 する
7. gameState with { Field, Score, Level, ClearedLineCount } を返す
```

## 5. スコア計算式

`GameDesign.md` §5.2 に準拠する。

| 消去ライン数 | スコア加算 |
|------------|-----------|
| 1 | `40 × (Level + 1)` |
| 2 | `100 × (Level + 1)` |
| 3 | `300 × (Level + 1)` |
| 4 | `1200 × (Level + 1)` |

スコア計算には**消去前の Level** を使用する。

## 6. 消去ライン数の算出

`Field.ClearCompletedLines()` は消去後の新しい `Field` を返すが、消去ライン数を直接返さない。
そのため消去前後のブロック数の差分から消去ライン数を算出する。

```csharp
var beforeCount = gameState.Field.Blocks.Count;
var newField = gameState.Field.ClearCompletedLines();
var afterCount = newField.Blocks.Count;
var clearedLines = (beforeCount - afterCount) / (Field.MaxX - Field.MinX + 1);
```

フィールドに残るのはz=0のブロックのみ（z=1はロック時に破棄済み）なので、
差分÷横幅（10）で正確に消去行数が求まる。

## 7. レベルアップ仕様

- レベルアップの閾値: `10 × (Level + 1)`（累積消去ライン数）
  - Level 0 → 1: 累積10ライン
  - Level 1 → 2: 累積20ライン
  - Level 2 → 3: 累積30ライン
- 1回の`Execute`で複数ライン消去しても、レベルアップは**1段階のみ**とする。

## 8. 設計指針

- **純粋関数**: 引数の `GameState` を変更せず、新しい `GameState` を返す。
- **UnityEngine 非依存**: 純粋な C# とする。
- **呼び出し順序**: `LockMinoUseCase.Execute` → `LineClearUseCase.Execute` の順で呼ぶ。`LineClearUseCase` 単独での呼び出しも可能だが、`ActiveMino` が `null` であることを前提とする。