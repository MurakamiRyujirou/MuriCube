using System.Collections.Generic;
using System.Linq;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Cube;
using Domain.Tetris;
using NUnit.Framework;

namespace Domain.Tests
{
    public class ActiveMinoTest
    {
        private static PivotPosition Piv0 => new PivotPosition(0f, 0f, 0f);

        private static CubePosition O(int x, int y, int z) => new CubePosition(x, y, z);

        private static Block AnyBlock()
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

        private static BlockGroup Group(params BlockPosition[] locals)
        {
            var dict = new Dictionary<BlockPosition, Block>();
            foreach (var p in locals)
                dict[p] = AnyBlock();
            return new BlockGroup(dict);
        }

        [Test]
        public void AbsolutePositions_OffsetAdded()
        {
            var group = Group(
                new BlockPosition(0f, 0f, 0f),
                new BlockPosition(1.5f, 0f, 0f));
            var mino = new ActiveMino(MinoType.T, group, O(5, 10, 0), Piv0);

            var abs = mino.AbsolutePositions().OrderBy(p => p.X).ToList();

            Assert.AreEqual(2, abs.Count);
            Assert.AreEqual(O(5, 10, 0), abs[0]);
            // 1.5 は AwayFromZero で 2 → オフセット (5,10,0) で (7,10,0)
            Assert.AreEqual(O(7, 10, 0), abs[1]);
        }

        [Test]
        public void WithOffset_ReturnsNewInstance()
        {
            var mino = new ActiveMino(MinoType.O, Group(new BlockPosition(0f, 0f, 0f)), O(1, 2, 0), Piv0);
            var moved = mino.WithOffset(O(4, 5, 1));

            Assert.AreNotSame(mino, moved);
            Assert.AreEqual(O(1, 2, 0), mino.Offset);
            Assert.AreEqual(O(4, 5, 1), moved.Offset);
            Assert.AreEqual(mino.MinoType, moved.MinoType);
            Assert.AreSame(mino.BlockGroup, moved.BlockGroup);
            Assert.AreEqual(mino.Pivot, moved.Pivot);
        }

        [Test]
        public void IsColliding_OutOfBounds_ReturnsTrue()
        {
            var mino = new ActiveMino(MinoType.I, Group(new BlockPosition(0f, 0f, 0f)), O(0, Field.MaxY + 1, 0), Piv0);
            Assert.IsTrue(mino.IsColliding(new Field()));
        }

        [Test]
        public void IsColliding_OccupiedCell_ReturnsTrue()
        {
            var field = new Field().WithCell(O(3, 4, 0), new StubBlock(BlockColor.Red));
            var mino = new ActiveMino(MinoType.J, Group(new BlockPosition(3f, 4f, 0f)), O(0, 0, 0), Piv0);
            Assert.IsTrue(mino.IsColliding(field));
        }

        [Test]
        public void IsColliding_FreeCell_ReturnsFalse()
        {
            var mino = new ActiveMino(
                MinoType.L,
                Group(new BlockPosition(0f, 0f, 0f), new BlockPosition(1f, 0f, 0f)),
                O(2, 3, 0),
                Piv0);
            Assert.IsFalse(mino.IsColliding(new Field()));
        }
    }
}
