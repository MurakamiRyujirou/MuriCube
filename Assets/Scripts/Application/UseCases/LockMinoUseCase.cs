using System;
using Application;
using Domain.Common;
using Domain.Cube;
using Domain.Tetris;

namespace Application.UseCases
{
    // 接地した ActiveMino をフィールドに固定する（UseCase_LockMino.md）。ライン消去は行わない。
    public static class LockMinoUseCase
    {
        public static GameState Execute(GameState gameState)
        {
            if (gameState == null)
                throw new ArgumentNullException(nameof(gameState));

            var mino = gameState.ActiveMino;
            if (mino == null)
                return gameState;

            var field = gameState.Field;
            foreach (var kv in mino.BlockGroup.Blocks)
            {
                var absolute = Combine(mino.Offset, kv.Key);
                if (absolute.Z != Field.MinZ)
                    continue;

                field = field.WithCell(absolute, kv.Value);
            }

            return gameState with { Field = field, ActiveMino = null };
        }

        private static CubePosition Combine(CubePosition offset, BlockPosition local)
        {
            return new CubePosition(
                offset.X + ToGrid(local.X),
                offset.Y + ToGrid(local.Y),
                offset.Z + ToGrid(local.Z));
        }

        private static int ToGrid(float value) =>
            (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }
}
