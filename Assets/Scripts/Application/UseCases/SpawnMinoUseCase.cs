using System;
using Application;
using Domain.Cube;
using Domain.Cube.Enums;
using Domain.Tetris;

namespace Application.UseCases
{
    // 新しいミノをランダム生成・回転し、フィールド上部中央に配置する（UseCase_SpawnMino.md）
    public static class SpawnMinoUseCase
    {
        private const int SpawnX = 3;
        private const int SpawnY = 18;
        private const int SpawnZ = 0;
        private const int RandomRotationCount = 20;

        private static readonly CubePosition SpawnOffset = new CubePosition(SpawnX, SpawnY, SpawnZ);

        public static GameState Execute(GameState gameState, Random random)
        {
            if (gameState == null)
                throw new ArgumentNullException(nameof(gameState));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var type = RandomMinoType(random);
            var mino = MinoFactory.Create(type);

            var blockGroup = new BlockGroup(mino.BlockGroup.Blocks);
            var cube = new Cube(blockGroup);
            var pivot = mino.Pivot;

            for (var i = 0; i < RandomRotationCount; i++)
            {
                var axis = (RotateAxis)random.Next(3);
                cube = cube.Rotate(axis, CubeTurn.Clockwise, pivot);
            }

            mino = mino.WithBlockGroup(cube).WithOffset(SpawnOffset);

            if (mino.IsColliding(gameState.Field))
                return gameState with { IsGameOver = true };

            return gameState with { ActiveMino = mino };
        }

        private static MinoType RandomMinoType(Random random)
        {
            var values = (MinoType[])Enum.GetValues(typeof(MinoType));
            return values[random.Next(values.Length)];
        }
    }
}
