using System;
using Application;
using Domain.Tetris;

namespace Application.UseCases
{
    // ソフトドロップ・ハードドロップ（UseCase_DropMino.md）
    public static class DropMinoUseCase
    {
        public static GameState Execute(GameState gameState, DropType dropType)
        {
            if (gameState == null)
                throw new ArgumentNullException(nameof(gameState));

            return dropType switch
            {
                DropType.Soft => ExecuteSoft(gameState),
                DropType.Hard => ExecuteHard(gameState),
                _ => throw new ArgumentOutOfRangeException(nameof(dropType)),
            };
        }

        // MoveMinoUseCase と等価（UseCase_DropMino.md §6）
        private static GameState ExecuteSoft(GameState gameState) =>
            MoveMinoUseCase.Execute(gameState, MoveDirection.Down);

        private static GameState ExecuteHard(GameState gameState)
        {
            var mino = gameState.ActiveMino;
            if (mino == null)
                return gameState;

            var current = mino;
            var field = gameState.Field;

            var maxIterations = Field.MaxY - Field.MinY + 10;
            var iterations = 0;

            while (true)
            {
                if (++iterations > maxIterations)
                    break;

                var offset = current.Offset;
                var nextOffset = new CubePosition(offset.X, offset.Y - 1, offset.Z);
                var next = current.WithOffset(nextOffset);

                if (next.IsColliding(field))
                    break;

                current = next;
            }

            return gameState with { ActiveMino = current };
        }
    }
}
