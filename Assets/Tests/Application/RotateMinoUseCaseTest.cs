using System;
using System.Collections.Generic;
using System.Linq;
using Application;
using Application.UseCases;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Cube;
using Domain.Cube.Enums;
using Domain.Tetris;
using NUnit.Framework;

namespace Application.Tests
{
    public class RotateMinoUseCaseTest
    {
        // Up=White, Down=Yellow, Left=Orange, Right=Red, Front=Green, Back=Blue
        [Test]
        public void Execute_Clockwise_ChangesBlockGroup()
        {
            // Y 軸 CW は Block.Y > pivot.Y のセルが対象。Pivot(0.5,0,0.5) で (0,1,0) が回転し Front→Left（CubeTest と同じ前提）
            var mino = CreateSingleStandardBlockMino(
                new BlockPosition(0f, 1f, 0f),
                new CubePosition(5, 10, 0),
                new PivotPosition(0.5f, 0f, 0.5f));
            var gameState = StateWithMino(mino);
            var before = GetOnlyBlock(gameState.ActiveMino!);
            Assert.AreEqual(BlockColor.Green, before.GetColor(BlockFace.Front));
            Assert.AreEqual(BlockColor.Orange, before.GetColor(BlockFace.Left));

            var result = RotateMinoUseCase.Execute(gameState, RotateAxis.Y, CubeTurn.Clockwise);

            Assert.IsNotNull(result.ActiveMino);
            var after = GetOnlyBlock(result.ActiveMino);
            Assert.AreEqual(BlockColor.Green, after.GetColor(BlockFace.Left), "Front の色が Left 面へ回ること");
        }

        [Test]
        public void Execute_Blocked_ReturnsOriginal()
        {
            // Z 軸 CW で (0,0,0)→(0,1,0)。オフセットを MaxY 上端に置き、回転後 Y=20 でウェル外（CubeTest Z 公転と同じ）
            var mino = CreateSingleCellMinoStub(new CubePosition(Field.MaxX, Field.MaxY, 0));
            var gameState = StateWithMino(mino);

            var result = RotateMinoUseCase.Execute(gameState, RotateAxis.Z, CubeTurn.Clockwise);

            Assert.AreSame(gameState, result);
        }

        [Test]
        public void Execute_NoActiveMino_ReturnsOriginal()
        {
            var gameState = GameState.Initial;

            var result = RotateMinoUseCase.Execute(gameState, RotateAxis.Y, CubeTurn.Clockwise);

            Assert.AreSame(gameState, result);
        }

        [Test]
        public void Execute_FourRotations_RestoresOriginal()
        {
            var mino = CreateSingleStandardBlockMino(
                new BlockPosition(0f, 1f, 0f),
                new CubePosition(5, 10, 0),
                new PivotPosition(0.5f, 0f, 0.5f));
            var gameState = StateWithMino(mino);
            var reference = CreateStandardBlock();

            var state = gameState;
            for (var i = 0; i < 4; i++)
                state = RotateMinoUseCase.Execute(state, RotateAxis.Y, CubeTurn.Clockwise);

            Assert.IsNotNull(state.ActiveMino);
            var finalBlock = GetOnlyBlock(state.ActiveMino);
            AssertStandardBlockColorsEqual(reference, finalBlock);
        }

        private static GameState StateWithMino(ActiveMino mino)
        {
            return new GameState(
                Field: new Field(),
                ActiveMino: mino,
                Score: 0,
                Level: 0,
                ClearedLineCount: 0,
                IsGameOver: false);
        }

        private static Block CreateStandardBlock()
        {
            return new Block(new Dictionary<BlockFace, BlockColor>
            {
                [BlockFace.Up] = BlockColor.White,
                [BlockFace.Down] = BlockColor.Yellow,
                [BlockFace.Left] = BlockColor.Orange,
                [BlockFace.Right] = BlockColor.Red,
                [BlockFace.Front] = BlockColor.Green,
                [BlockFace.Back] = BlockColor.Blue,
            });
        }

        private static ActiveMino CreateSingleStandardBlockMino(
            BlockPosition local,
            CubePosition offset,
            PivotPosition pivot)
        {
            var blocks = new Dictionary<BlockPosition, Block> { [local] = CreateStandardBlock() };
            var group = new BlockGroup(blocks);
            return new ActiveMino(MinoType.I, group, offset, pivot);
        }

        // MoveMinoUseCaseTest と同様の1セル（StubBlock 全面同色）
        private static ActiveMino CreateSingleCellMinoStub(CubePosition offset)
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

        private static Block GetOnlyBlock(ActiveMino mino)
        {
            var block = mino.BlockGroup.Blocks.Values.Single();
            Assert.IsInstanceOf<Block>(block);
            return (Block)block;
        }

        private static void AssertStandardBlockColorsEqual(Block expected, Block actual)
        {
            foreach (BlockFace face in Enum.GetValues(typeof(BlockFace)))
                Assert.AreEqual(expected.GetColor(face), actual.GetColor(face), face.ToString());
        }
    }
}
