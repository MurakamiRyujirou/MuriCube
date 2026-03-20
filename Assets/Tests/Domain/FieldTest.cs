using System;
using Domain.Common.Enums;
using Domain.Tetris;
using NUnit.Framework;

namespace Domain.Tests
{
    public class FieldTest
    {
        private static CubePosition P(int x, int y, int z) => new CubePosition(x, y, z);

        private static Field RowWithUniformFront(Field field, int y, BlockColor color)
        {
            var f = field;
            for (int x = Field.MinX; x <= Field.MaxX; x++)
                f = f.WithCell(P(x, y, Field.MinZ), new StubBlock(color));
            return f;
        }

        // --------------------------------------------------------------------
        // A. WithCell / WithoutCell
        // --------------------------------------------------------------------

        [Test]
        public void WithCell_AddsBlock()
        {
            var field = new Field();
            var block = new StubBlock(BlockColor.Red);
            var next = field.WithCell(P(0, 0, 0), block);

            Assert.AreEqual(0, field.Blocks.Count);
            Assert.AreEqual(1, next.Blocks.Count);
            Assert.IsTrue(next.TryGetBlock(P(0, 0, 0), out var got));
            Assert.AreSame(block, got);
        }

        [Test]
        public void WithCell_OriginalUnchanged()
        {
            var field = new Field();
            var next = field.WithCell(P(3, 4, 1), new StubBlock(BlockColor.Blue));

            Assert.AreEqual(0, field.Blocks.Count, "元の Field は空のまま");
            Assert.AreEqual(1, next.Blocks.Count);
        }

        [Test]
        public void WithCell_OutOfRange_ThrowsArgumentException()
        {
            var field = new Field();
            var block = new StubBlock(BlockColor.Red);

            Assert.Throws<ArgumentException>(() => field.WithCell(P(-1, 0, 0), block));
            Assert.Throws<ArgumentException>(() => field.WithCell(P(0, 20, 0), block));
            Assert.Throws<ArgumentException>(() => field.WithCell(P(0, 0, 2), block));
        }

        [Test]
        public void WithCell_NullBlock_ThrowsArgumentNullException()
        {
            var field = new Field();
            Assert.Throws<ArgumentNullException>(() => field.WithCell(P(0, 0, 0), null));
        }

        [Test]
        public void WithoutCell_RemovesBlock()
        {
            var field = new Field().WithCell(P(5, 10, 1), new StubBlock(BlockColor.Green));
            var next = field.WithoutCell(P(5, 10, 1));

            Assert.AreEqual(1, field.Blocks.Count);
            Assert.AreEqual(0, next.Blocks.Count);
        }

        [Test]
        public void WithoutCell_NonExistentKey_NoError()
        {
            var field = new Field();
            Assert.DoesNotThrow(() => field.WithoutCell(P(0, 0, 0)));
            var next = field.WithoutCell(P(0, 0, 0));
            Assert.AreEqual(0, next.Blocks.Count);
        }

        // --------------------------------------------------------------------
        // B. IsLineClearable
        // --------------------------------------------------------------------

        [Test]
        public void IsLineClearable_AllSameColor_ReturnsTrue()
        {
            var field = RowWithUniformFront(new Field(), 5, BlockColor.Red);
            Assert.IsTrue(field.IsLineClearable(5));
        }

        [Test]
        public void IsLineClearable_MixedColor_ReturnsFalse()
        {
            var field = new Field();
            for (int x = Field.MinX; x <= Field.MaxX - 1; x++)
                field = field.WithCell(P(x, 3, Field.MinZ), new StubBlock(BlockColor.Red));
            field = field.WithCell(P(Field.MaxX, 3, Field.MinZ), new StubBlock(BlockColor.Blue));

            Assert.IsFalse(field.IsLineClearable(3));
        }

        [Test]
        public void IsLineClearable_Incomplete_ReturnsFalse()
        {
            var field = new Field();
            for (int x = Field.MinX; x <= Field.MaxX - 1; x++)
                field = field.WithCell(P(x, 2, Field.MinZ), new StubBlock(BlockColor.White));

            Assert.IsFalse(field.IsLineClearable(2));
        }

        [Test]
        public void IsLineClearable_Z1Only_ReturnsFalse()
        {
            var field = new Field();
            for (int x = Field.MinX; x <= Field.MaxX; x++)
                field = field.WithCell(P(x, 7, Field.MaxZ), new StubBlock(BlockColor.Green));

            Assert.IsFalse(field.IsLineClearable(7));
        }

        [Test]
        public void IsLineClearable_OutOfRange_ReturnsFalse()
        {
            var field = RowWithUniformFront(new Field(), 0, BlockColor.Yellow);
            Assert.IsFalse(field.IsLineClearable(-1));
            Assert.IsFalse(field.IsLineClearable(20));
        }

        // --------------------------------------------------------------------
        // C. ClearCompletedLines
        // --------------------------------------------------------------------

        [Test]
        public void ClearCompletedLines_NoLine_ReturnsSameInstance()
        {
            var field = new Field();
            var result = field.ClearCompletedLines();
            Assert.AreSame(field, result);
        }

        [Test]
        public void ClearCompletedLines_OneLine_Cleared()
        {
            var field = RowWithUniformFront(new Field(), 0, BlockColor.Orange);
            field = field.WithCell(P(0, 1, Field.MinZ), new StubBlock(BlockColor.Red));

            var next = field.ClearCompletedLines();

            Assert.IsFalse(next.IsLineClearable(0), "y=0 は消去済みで再び条件を満たさない想定");
            Assert.IsTrue(next.TryGetBlock(P(0, 0, Field.MinZ), out _), "上段が y-1 シフトしていること");
            Assert.IsFalse(next.TryGetBlock(P(0, 1, Field.MinZ), out _), "旧 y=1 は空になっていること");
            for (int x = Field.MinX + 1; x <= Field.MaxX; x++)
                Assert.IsFalse(next.TryGetBlock(P(x, 0, Field.MinZ), out _), "消去された行の他セルは空");
        }

        [Test]
        public void ClearCompletedLines_TwoLines_ShiftCorrect()
        {
            var field = RowWithUniformFront(new Field(), 0, BlockColor.Green);
            field = RowWithUniformFront(field, 1, BlockColor.Green);
            field = field.WithCell(P(2, 3, Field.MinZ), new StubBlock(BlockColor.Blue));

            var next = field.ClearCompletedLines();

            Assert.IsTrue(next.TryGetBlock(P(2, 1, Field.MinZ), out var shifted));
            Assert.AreEqual(BlockColor.Blue, shifted.GetColor(BlockFace.Front));
            Assert.IsFalse(next.TryGetBlock(P(2, 3, Field.MinZ), out _));
        }

        [Test]
        public void ClearCompletedLines_Z1_FollowsShift()
        {
            var field = RowWithUniformFront(new Field(), 0, BlockColor.White);
            field = field.WithCell(P(4, 5, Field.MaxZ), new StubBlock(BlockColor.Red));

            var next = field.ClearCompletedLines();

            Assert.IsTrue(next.TryGetBlock(P(4, 4, Field.MaxZ), out _), "z=1 も y が 1 段下がること");
            Assert.IsFalse(next.TryGetBlock(P(4, 5, Field.MaxZ), out _));
        }

        [Test]
        public void ClearCompletedLines_NonClearableLine_Stays()
        {
            var field = new Field();
            for (int x = Field.MinX; x <= Field.MinX + 4; x++)
                field = field.WithCell(P(x, 0, Field.MinZ), new StubBlock(BlockColor.Orange));
            field = RowWithUniformFront(field, 1, BlockColor.Yellow);

            var next = field.ClearCompletedLines();

            Assert.IsFalse(field.IsLineClearable(0), "前提: y=0 は未充填");
            Assert.IsTrue(field.IsLineClearable(1));
            for (int x = Field.MinX; x <= Field.MinX + 4; x++)
                Assert.IsTrue(next.TryGetBlock(P(x, 0, Field.MinZ), out _), "消去不可の y=0 は残る");
        }
    }
}
