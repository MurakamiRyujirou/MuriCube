using System;
using Application;
using Domain.Tetris;

namespace Application.UseCases
{
    // ActiveMino を左・右・下に1セル移動する（UseCase_MoveMino.md）
    public static class MoveMinoUseCase
    {
        public static GameState Execute(GameState gameState, MoveDirection direction)
        {
            if (gameState == null)
                throw new ArgumentNullException(nameof(gameState));

            var mino = gameState.ActiveMino;
            if (mino == null)
                return gameState;

            var (dx, dy, dz) = Delta(direction);
            var current = mino.Offset;
            var newOffset = new CubePosition(current.X + dx, current.Y + dy, current.Z + dz);
            var movedMino = mino.WithOffset(newOffset);

            if (movedMino.IsColliding(gameState.Field))
                return gameState;

            return gameState with { ActiveMino = movedMino };
        }

        private static (int dx, int dy, int dz) Delta(MoveDirection direction) =>
            direction switch
            {
                MoveDirection.Left => (-1, 0, 0),
                MoveDirection.Right => (1, 0, 0),
                MoveDirection.Down => (0, -1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction)),
            };
    }
}
