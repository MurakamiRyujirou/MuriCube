using System;
using System.Collections.Generic;
using Domain.Tetris;

namespace Application
{
    // ゲーム全体のデータを保持する不変レコード（フェーズロジックは持たない）
    public sealed record GameState(
        Field Field,
        ActiveMino ActiveMino,
        int Score,
        int Level,
        int ClearedLineCount,
        bool IsGameOver,
        IReadOnlyList<ScramblingMove> ScramblingMoves)
    {
        // ActiveMino: 操作中のミノ。未生成時は null（NRT 無効のため型は ActiveMino のまま）
        public static GameState Initial { get; } = new GameState(
            Field: new Field(),
            ActiveMino: null,
            Score: 0,
            Level: 0,
            ClearedLineCount: 0,
            IsGameOver: false,
            ScramblingMoves: Array.Empty<ScramblingMove>());
    }
}
