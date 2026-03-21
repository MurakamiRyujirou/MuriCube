using System;
using Application;
using Domain.Cube;
using Domain.Cube.Enums;
using Domain.Tetris;

namespace Application.UseCases
{
    // ActiveMino を指定軸・方向で 90° 回転する（UseCase_RotateMino.md）
    public static class RotateMinoUseCase
    {
        public static GameState Execute(GameState gameState, RotateAxis axis, CubeTurn turn)
        {
            if (gameState == null)
                throw new ArgumentNullException(nameof(gameState));

            var mino = gameState.ActiveMino;
            if (mino == null)
                return gameState;

            var cube = new Cube(new BlockGroup(mino.BlockGroup.Blocks));
            var rotatedCube = cube.Rotate(axis, turn, mino.Pivot);
            var rotatedMino = mino.WithBlockGroup(rotatedCube);

            if (rotatedMino.IsColliding(gameState.Field))
                return gameState;

            return gameState with { ActiveMino = rotatedMino };
        }
    }
}
