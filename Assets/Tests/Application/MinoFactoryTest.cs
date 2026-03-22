using System;
using System.Linq;
using Application;
using Domain.Cube;
using Domain.Tetris;
using NUnit.Framework;

namespace Application.Tests
{
    public class MinoFactoryTest
    {
        private static readonly MinoType[] AllTypes = (MinoType[])Enum.GetValues(typeof(MinoType));

        [Test]
        public void Create_ReturnsCorrectMinoType()
        {
            foreach (var type in AllTypes)
            {
                var mino = MinoFactory.Create(type);
                Assert.AreEqual(type, mino.MinoType);
            }
        }

        [Test]
        public void Create_HasTwoLayers()
        {
            foreach (var type in AllTypes)
            {
                var mino = MinoFactory.Create(type);
                var zs = mino.BlockGroup.Blocks.Keys.Select(k => k.Z).Distinct().ToList();
                Assert.That(zs, Does.Contain(0f), type.ToString());
                Assert.That(zs, Does.Contain(1f), type.ToString());
            }
        }

        [Test]
        public void Create_PivotIsCorrect()
        {
            var pivI = new PivotPosition(1.5f, 0.5f, 0.5f);
            var pivO = new PivotPosition(0.5f, 0.5f, 0.5f);
            var pivStd = new PivotPosition(1.5f, 0.5f, 0.5f);

            AssertPivot(MinoType.I, pivI);
            AssertPivot(MinoType.O, pivO);
            foreach (var t in new[] { MinoType.T, MinoType.S, MinoType.Z, MinoType.J, MinoType.L })
                AssertPivot(t, pivStd);
        }

        [Test]
        public void Create_OffsetIsZero()
        {
            foreach (var type in AllTypes)
            {
                var mino = MinoFactory.Create(type);
                var o = mino.Offset;
                Assert.AreEqual(0, o.X, type.ToString());
                Assert.AreEqual(0, o.Y, type.ToString());
                Assert.AreEqual(0, o.Z, type.ToString());
            }
        }

        [Test]
        public void Create_AllTypes_NoException()
        {
            foreach (var type in AllTypes)
                Assert.DoesNotThrow(() => MinoFactory.Create(type));
        }

        private static void AssertPivot(MinoType type, PivotPosition expected)
        {
            var p = MinoFactory.Create(type).Pivot;
            Assert.AreEqual(expected.X, p.X, 1e-6f, type.ToString());
            Assert.AreEqual(expected.Y, p.Y, 1e-6f, type.ToString());
            Assert.AreEqual(expected.Z, p.Z, 1e-6f, type.ToString());
        }
    }
}
