# Docs/Standards/CodingGuidelines.md

## 1. コーディング規約
### 1.1 命名規則
- **クラス・メソッド・プロパティ**: `PascalCase`
- **引数・ローカル変数**: `camelCase`
- **private フィールド**: `_camelCase` (例: `_activeMino`)

### 1.2 コメント
- **言語**: 日本語を使用する。
- **形式**: `//` による 1 行コメントを基本とし、末尾に「。」はつけない。
- **XMLコメント**: 原則として記述しない。

## 1.3 言語仕様
- **C# バージョン**: 安全のため C# 9.0 互換の記述を優先せよ。
- **record struct**: 使用を禁止し、`readonly struct` による手動実装を選択せよ。

## 2. 技術スタックの作法（導入経路とバージョン）
AIは以下のライブラリが導入済みであることを前提とし、そのAPIを使用せよ。

### 2.1 UniTask (V2.5.10) [導入: UPM]
- 非同期処理の標準。

### 2.2 R3 (V1.3.0) [導入: NuGetForUnity]
- 状態通知の標準。依存ライブラリ（Bcl.TimeProvider等）は NuGet により解決済み。

### 2.3 VContainer (V1.17.0) [導入: UPM/NuGet]
- 依存性の注入（DI）の標準。

### 2.4 DOTween (1.2.825) [導入: Asset Store]
- 演出・アニメーションの標準。