using System;
using System.Linq;
using Application;
using Domain.Cube;
using Domain.Cube.Enums;
using Domain.Tetris;

namespace Application.UseCases
{
    // 新しいミノをランダム生成・回転し、フィールド上部中央に配置する（UseCase_SpawnMino.md）
    public static class SpawnMinoUseCase
    {
        // バウンディングの最小セルが乗る目標グリッド（回転後にウェル内へ収めるよう Clamp する）
        private const int TargetSpawnMinX = 3;
        private const int TargetSpawnMinY = 18;
        private const int TargetSpawnMinZ = 0;

        private const int RandomRotationCount = 20;

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

            mino = mino.WithBlockGroup(cube);
            var spawnOffset = ComputeSpawnOffset(mino);
            mino = mino.WithOffset(spawnOffset);

            if (mino.IsColliding(gameState.Field))
                return gameState with { IsGameOver = true };

            return gameState with { ActiveMino = mino };
        }

        // オフセット (0,0,0) 時の絶対セル（= 丸め後ローカル）の最小 X/Y を基準に目標位置へ寄せ、ウェル内に収まるよう Clamp する
        private static CubePosition ComputeSpawnOffset(ActiveMino minoAtOrigin)
        {
            var shape = minoAtOrigin.AbsolutePositions().ToList();
            var minX = shape.Min(p => p.X);
            var maxX = shape.Max(p => p.X);
            var minY = shape.Min(p => p.Y);
            var maxY = shape.Max(p => p.Y);
            var minZ = shape.Min(p => p.Z);
            var maxZ = shape.Max(p => p.Z);

            var spawnX = TargetSpawnMinX - minX;
            var spawnY = TargetSpawnMinY - minY;
            var spawnZ = TargetSpawnMinZ - minZ;

            var minAllowedX = Field.MinX - minX;
            var maxAllowedX = Field.MaxX - maxX;
            var minAllowedY = Field.MinY - minY;
            var maxAllowedY = Field.MaxY - maxY;
            var minAllowedZ = Field.MinZ - minZ;
            var maxAllowedZ = Field.MaxZ - maxZ;

            spawnX = ClampSpawnComponent(spawnX, minAllowedX, maxAllowedX);
            spawnY = ClampSpawnComponent(spawnY, minAllowedY, maxAllowedY);
            spawnZ = ClampSpawnComponent(spawnZ, minAllowedZ, maxAllowedZ);

            return new CubePosition(spawnX, spawnY, spawnZ);
        }

        private static int ClampSpawnComponent(int value, int minAllowed, int maxAllowed)
        {
            if (minAllowed > maxAllowed)
                return minAllowed;
            return Math.Min(Math.Max(value, minAllowed), maxAllowed);
        }

        private static MinoType RandomMinoType(Random random)
        {
            var values = (MinoType[])Enum.GetValues(typeof(MinoType));
            return values[random.Next(values.Length)];
        }
    }
}
