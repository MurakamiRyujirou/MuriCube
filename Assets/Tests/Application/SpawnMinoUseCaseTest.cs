using System;
using System.Linq;
using Application;
using Application.UseCases;
using Domain.Cube;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Tetris;
using NUnit.Framework;

namespace Application.Tests
{
    public class SpawnMinoUseCaseTest
    {
        [Test]
        public void Execute_SetsActiveMino()
        {
            var gameState = GameState.Initial;

            var result = SpawnMinoUseCase.Execute(gameState, new Random(0));

            Assert.IsFalse(result.IsGameOver);
            Assert.IsNotNull(result.ActiveMino);
        }

        [Test]
        public void Execute_ActiveMinoIsWithinField()
        {
            var gameState = GameState.Initial;

            var result = SpawnMinoUseCase.Execute(gameState, new Random(0));

            Assert.IsFalse(result.IsGameOver);
            Assert.IsNotNull(result.ActiveMino);

            Assert.That(
                result.ActiveMino.AbsolutePositions().All(Field.Contains),
                Is.True);
        }

        [Test]
        public void Execute_GameOver_WhenFieldFull()
        {
            var fullField = CreateFullField();
            var gameState = new GameState(
                Field: fullField,
                ActiveMino: null,
                Score: 0,
                Level: 0,
                ClearedLineCount: 0,
                IsGameOver: false,
                ScramblingMoves: Array.Empty<ScramblingMove>());

            var result = SpawnMinoUseCase.Execute(gameState, new Random(0));

            Assert.IsTrue(result.IsGameOver);
            Assert.IsNull(result.ActiveMino);
        }

        [Test]
        public void Execute_DoesNotMutateOriginalState()
        {
            var originalField = new Field();
            var original = new GameState(
                Field: originalField,
                ActiveMino: null,
                Score: 0,
                Level: 0,
                ClearedLineCount: 0,
                IsGameOver: false,
                ScramblingMoves: Array.Empty<ScramblingMove>());

            var result = SpawnMinoUseCase.Execute(original, new Random(0));

            Assert.AreNotSame(original, result);
            Assert.AreSame(originalField, original.Field);
            Assert.IsNull(original.ActiveMino);
            Assert.IsFalse(original.IsGameOver);
        }

        // 動的スポーンで上部2行だけでは空きに逃げられるため、ウェル全体を埋めて必ず衝突させる
        private static Field CreateFullField()
        {
            var field = new Field();
            var block = CreateAnyBlock();

            for (int y = Field.MinY; y <= Field.MaxY; y++)
            {
                for (int x = Field.MinX; x <= Field.MaxX; x++)
                {
                    for (int z = Field.MinZ; z <= Field.MaxZ; z++)
                    {
                        field = field.WithCell(new CubePosition(x, y, z), block);
                    }
                }
            }

            return field;
        }

        private static Block CreateAnyBlock()
        {
            var c = BlockColor.Red;
            return new Block(new System.Collections.Generic.Dictionary<BlockFace, BlockColor>
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
