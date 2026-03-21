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
    public class LineClearUseCaseTest
    {
        private static CubePosition P(int x, int y, int z) => new CubePosition(x, y, z);

        private static Field RowWithUniformFront(Field field, int y)
        {
            var block = CreateAnyBlock();
            var f = field;
            for (int x = Field.MinX; x <= Field.MaxX; x++)
                f = f.WithCell(P(x, y, Field.MinZ), block);
            return f;
        }

        [Test]
        public void Execute_ClearsLine_UpdatesField()
        {
            var field = RowWithUniformFront(new Field(), 5);
            var gameState = BaseState(field);

            var result = LineClearUseCase.Execute(gameState);

            Assert.AreEqual(0, result.Field.Blocks.Count, "消去後はブロックが残らない想定");
        }

        [Test]
        public void Execute_ScoreAdded_OneLine()
        {
            var field = RowWithUniformFront(new Field(), 0);
            var gameState = BaseState(field, level: 0, score: 0);

            var result = LineClearUseCase.Execute(gameState);

            Assert.AreEqual(40, result.Score, "1ラインは 40 × (Level+1)、Level=0 なら +40");
        }

        [Test]
        public void Execute_ScoreAdded_TwoLines()
        {
            var field = RowWithUniformFront(new Field(), 0);
            field = RowWithUniformFront(field, 1);
            var gameState = BaseState(field, level: 0, score: 0);

            var result = LineClearUseCase.Execute(gameState);

            Assert.AreEqual(100, result.Score, "2ライン同時は 100 × (Level+1)");
        }

        [Test]
        public void Execute_LevelUp()
        {
            var field = RowWithUniformFront(new Field(), 0);
            var gameState = BaseState(field, level: 0, clearedLineCount: 9);

            var result = LineClearUseCase.Execute(gameState);

            Assert.AreEqual(1, result.Level);
            Assert.AreEqual(10, result.ClearedLineCount);
        }

        [Test]
        public void Execute_NoLine_ReturnsUnchanged()
        {
            var field = new Field();
            for (int x = Field.MinX; x <= Field.MaxX - 1; x++)
                field = field.WithCell(P(x, 0, Field.MinZ), CreateAnyBlock());
            var gameState = BaseState(field, score: 100, level: 2, clearedLineCount: 5);

            var result = LineClearUseCase.Execute(gameState);

            Assert.AreEqual(100, result.Score);
            Assert.AreEqual(2, result.Level);
            Assert.AreEqual(5, result.ClearedLineCount);
        }

        private static GameState BaseState(
            Field field,
            int score = 0,
            int level = 0,
            int clearedLineCount = 0)
        {
            return new GameState(
                Field: field,
                ActiveMino: null,
                Score: score,
                Level: level,
                ClearedLineCount: clearedLineCount,
                IsGameOver: false);
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
