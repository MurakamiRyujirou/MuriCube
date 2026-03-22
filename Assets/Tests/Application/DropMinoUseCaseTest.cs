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
    public class DropMinoUseCaseTest
    {
        [Test]
        public void Execute_Soft_MovesOffsetDown()
        {
            var mino = CreateSingleCellMino(new CubePosition(5, 10, 0));
            var gameState = StateWithMino(mino);

            var result = DropMinoUseCase.Execute(gameState, DropType.Soft);

            Assert.IsNotNull(result.ActiveMino);
            Assert.AreEqual(5, result.ActiveMino.Offset.X);
            Assert.AreEqual(9, result.ActiveMino.Offset.Y);
            Assert.AreEqual(0, result.ActiveMino.Offset.Z);
        }

        [Test]
        public void Execute_Soft_Blocked_ReturnsOriginal()
        {
            // Y=0 で下へ1セルするとウェル外になり移動不可
            var mino = CreateSingleCellMino(new CubePosition(5, Field.MinY, 0));
            var gameState = StateWithMino(mino);

            var result = DropMinoUseCase.Execute(gameState, DropType.Soft);

            Assert.AreSame(gameState, result);
        }

        [Test]
        public void Execute_Hard_LandsAtBottom()
        {
            var mino = CreateSingleCellMino(new CubePosition(5, 12, 0));
            var gameState = StateWithMino(mino);

            var result = DropMinoUseCase.Execute(gameState, DropType.Hard);

            Assert.IsNotNull(result.ActiveMino);
            Assert.AreEqual(Field.MinY, result.ActiveMino.Offset.Y);
        }

        [Test]
        public void Execute_Hard_LandsOnBlock()
        {
            var block = CreateAnyBlock();
            var field = new Field().WithCell(new CubePosition(5, 5, 0), block);
            var mino = CreateSingleCellMino(new CubePosition(5, 12, 0));
            var gameState = StateWithMino(mino, field);

            var result = DropMinoUseCase.Execute(gameState, DropType.Hard);

            Assert.IsNotNull(result.ActiveMino);
            Assert.AreEqual(6, result.ActiveMino.Offset.Y);
            Assert.AreEqual(5, result.ActiveMino.Offset.X);
            Assert.AreEqual(0, result.ActiveMino.Offset.Z);
        }

        [Test]
        public void Execute_NoActiveMino_ReturnsOriginal()
        {
            var gameState = GameState.Initial;

            Assert.AreSame(gameState, DropMinoUseCase.Execute(gameState, DropType.Soft));
            Assert.AreSame(gameState, DropMinoUseCase.Execute(gameState, DropType.Hard));
        }

        private static GameState StateWithMino(ActiveMino mino, Field field = null)
        {
            return new GameState(
                Field: field ?? new Field(),
                ActiveMino: mino,
                Score: 0,
                Level: 0,
                ClearedLineCount: 0,
                IsGameOver: false,
                ScramblingMoves: Array.Empty<ScramblingMove>());
        }

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
