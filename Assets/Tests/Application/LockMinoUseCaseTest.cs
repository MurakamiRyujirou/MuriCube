using System;
using System.Collections.Generic;
using Application;
using Application.UseCases;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Cube;
using Domain.Tetris;
using NUnit.Framework;

namespace Application.Tests
{
    public class LockMinoUseCaseTest
    {
        private static readonly CubePosition Offset = new CubePosition(3, 5, 0);

        // MinoFactory の T と同形（z=0 に4セル・z=1 に4セル）
        private static readonly CubePosition[] ExpectedZ0Abs =
        {
            new CubePosition(3, 5, 0),
            new CubePosition(4, 5, 0),
            new CubePosition(5, 5, 0),
            new CubePosition(4, 6, 0),
        };

        private static readonly CubePosition[] ExpectedZ1Abs =
        {
            new CubePosition(3, 5, 1),
            new CubePosition(4, 5, 1),
            new CubePosition(5, 5, 1),
            new CubePosition(4, 6, 1),
        };

        [Test]
        public void Execute_PlacesBlocksInField()
        {
            var gameState = StateWithTShapeMino();
            var result = LockMinoUseCase.Execute(gameState);

            foreach (var p in ExpectedZ0Abs)
            {
                Assert.IsTrue(result.Field.TryGetBlock(p, out var block), $"z=0 にブロックが必要: {p}");
                Assert.IsNotNull(block);
            }
        }

        [Test]
        public void Execute_Z1_NotPlaced()
        {
            var gameState = StateWithTShapeMino();
            var result = LockMinoUseCase.Execute(gameState);

            foreach (var p in ExpectedZ1Abs)
                Assert.IsFalse(result.Field.TryGetBlock(p, out _), $"z=1 にブロックがあってはならない: {p}");
        }

        [Test]
        public void Execute_ClearsActiveMino()
        {
            var gameState = StateWithTShapeMino();
            var result = LockMinoUseCase.Execute(gameState);

            Assert.IsNull(result.ActiveMino);
        }

        [Test]
        public void Execute_NoActiveMino_ReturnsOriginal()
        {
            var gameState = GameState.Initial;

            var result = LockMinoUseCase.Execute(gameState);

            Assert.AreSame(gameState, result);
        }

        private static GameState StateWithTShapeMino()
        {
            var b = CreateAnyBlock();
            var blocks = new Dictionary<BlockPosition, Block>
            {
                [new BlockPosition(0f, 0f, 0f)] = b,
                [new BlockPosition(1f, 0f, 0f)] = b,
                [new BlockPosition(2f, 0f, 0f)] = b,
                [new BlockPosition(1f, 1f, 0f)] = b,
                [new BlockPosition(0f, 0f, 1f)] = b,
                [new BlockPosition(1f, 0f, 1f)] = b,
                [new BlockPosition(2f, 0f, 1f)] = b,
                [new BlockPosition(1f, 1f, 1f)] = b,
            };
            var group = new BlockGroup(blocks);
            var mino = new ActiveMino(
                MinoType.T,
                group,
                Offset,
                new PivotPosition(1f, 0.5f, 0.5f));

            return new GameState(
                Field: new Field(),
                ActiveMino: mino,
                Score: 0,
                Level: 0,
                ClearedLineCount: 0,
                IsGameOver: false,
                ScramblingMoves: Array.Empty<ScramblingMove>());
        }

        private static Block CreateAnyBlock()
        {
            var c = BlockColor.Red;
            return new Block(new Dictionary<BlockFace, BlockColor>
            {
                [BlockFace.Up] = c,
                [BlockFace.Down] = c,
                [BlockFace.Left] = c,
                [BlockFace.Right] = c,
                [BlockFace.Front] = c,
                [BlockFace.Back] = c,
            });
        }
    }
}
