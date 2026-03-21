using System;
using Application;
using Domain.Tetris;

namespace Application.UseCases
{
    // ライン消去・スコア・レベルを更新する（UseCase_LineClear.md）
    public static class LineClearUseCase
    {
        private static int FieldWidth => Field.MaxX - Field.MinX + 1;

        public static GameState Execute(GameState gameState)
        {
            if (gameState == null)
                throw new ArgumentNullException(nameof(gameState));

            var beforeCount = gameState.Field.Blocks.Count;
            var newField = gameState.Field.ClearCompletedLines();
            var afterCount = newField.Blocks.Count;
            var clearedLines = (beforeCount - afterCount) / FieldWidth;

            if (clearedLines == 0)
                return gameState with { Field = newField };

            var level = gameState.Level;
            var scoreDelta = ScoreForLines(clearedLines, level);
            var newClearedLineCount = gameState.ClearedLineCount + clearedLines;
            var newLevel = newClearedLineCount >= 10 * (level + 1) ? level + 1 : level;

            return gameState with
            {
                Field = newField,
                Score = gameState.Score + scoreDelta,
                Level = newLevel,
                ClearedLineCount = newClearedLineCount
            };
        }

        private static int ScoreForLines(int clearedLines, int level)
        {
            var m = level + 1;
            return clearedLines switch
            {
                1 => 40 * m,
                2 => 100 * m,
                3 => 300 * m,
                4 => 1200 * m,
                _ => 0
            };
        }
    }
}
