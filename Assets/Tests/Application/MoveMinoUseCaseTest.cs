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
    public class MoveMinoUseCaseTest
    {
        [Test]
        public void Execute_Left_MovesOffset()
        {
            var mino = CreateSingleCellMino(new CubePosition(5, 10, 0));
            var gameState = StateWithMino(mino);

            var result = MoveMinoUseCase.Execute(gameState, MoveDirection.Left);

            Assert.IsNotNull(result.ActiveMino);
            Assert.AreEqual(4, result.ActiveMino.Offset.X);
            Assert.AreEqual(10, result.ActiveMino.Offset.Y);
            Assert.AreEqual(0, result.ActiveMino.Offset.Z);
        }

        [Test]
        public void Execute_Right_MovesOffset()
        {
            var mino = CreateSingleCellMino(new CubePosition(5, 10, 0));
            var gameState = StateWithMino(mino);

            var result = MoveMinoUseCase.Execute(gameState, MoveDirection.Right);

            Assert.IsNotNull(result.ActiveMino);
            Assert.AreEqual(6, result.ActiveMino.Offset.X);
            Assert.AreEqual(10, result.ActiveMino.Offset.Y);
            Assert.AreEqual(0, result.ActiveMino.Offset.Z);
        }

        [Test]
        public void Execute_Down_MovesOffset()
        {
            var mino = CreateSingleCellMino(new CubePosition(5, 10, 0));
            var gameState = StateWithMino(mino);

            var result = MoveMinoUseCase.Execute(gameState, MoveDirection.Down);

            Assert.IsNotNull(result.ActiveMino);
            Assert.AreEqual(5, result.ActiveMino.Offset.X);
            Assert.AreEqual(9, result.ActiveMino.Offset.Y);
            Assert.AreEqual(0, result.ActiveMino.Offset.Z);
        }

        [Test]
        public void Execute_Blocked_ReturnsOriginal()
        {
            // 絶対 X = MinX で左へ1セルするとウェル外になる
            var mino = CreateSingleCellMino(new CubePosition(Field.MinX, 10, 0));
            var gameState = StateWithMino(mino);

            var result = MoveMinoUseCase.Execute(gameState, MoveDirection.Left);

            Assert.AreSame(gameState, result);
        }

        [Test]
        public void Execute_NoActiveMino_ReturnsOriginal()
        {
            var gameState = GameState.Initial;

            var result = MoveMinoUseCase.Execute(gameState, MoveDirection.Left);

            Assert.AreSame(gameState, result);
        }

        private static GameState StateWithMino(ActiveMino mino)
        {
            return new GameState(
                Field: new Field(),
                ActiveMino: mino,
                Score: 0,
                Level: 0,
                ClearedLineCount: 0,
                IsGameOver: false,
                ScramblingMoves: Array.Empty<ScramblingMove>());
        }

        // ローカル (0,0,0) の1セルのみ（SpawnMinoUseCaseTest.CreateAnyBlock と同様）
        private static ActiveMino CreateSingleCellMino(CubePosition offset)
        {
            var block = CreateAnyBlock();
            var blocks = new Dictionary<BlockPosition, Block>
            {
                [new BlockPosition(0f, 0f, 0f)] = block,
            };
            var group = new BlockGroup(blocks);
            return new ActiveMino(MinoType.I, group, offset, new PivotPosition(0.5f, 0.5f, 0.5f));
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
