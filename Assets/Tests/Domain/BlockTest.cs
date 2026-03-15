using System.Collections.Generic;
using Domain.Common.Enums;
using Domain.Cube;
using Domain.Cube.Enums;
using NUnit.Framework;

namespace Domain.Tests
{
    // Block 単体の X/Y/Z 軸回転が TechSpecs 3.2 のスワップロジック通りに動作することを検証する
    public class BlockTest
    {
        private static IReadOnlyDictionary<BlockFace, BlockColor> CreateSixFaceBlock()
        {
            // 世界標準配色（上:白, 下:黄, 右:赤, 左:橙, 前:緑, 後:青）
            return new Dictionary<BlockFace, BlockColor>
            {
                [BlockFace.Up] = BlockColor.White,
                [BlockFace.Down] = BlockColor.Yellow,
                [BlockFace.Left] = BlockColor.Orange,
                [BlockFace.Right] = BlockColor.Red,
                [BlockFace.Front] = BlockColor.Green,
                [BlockFace.Back] = BlockColor.Blue,
            };
        }

        [Test]
        public void RotateX_FrontColorMovesToUp_AndFourFacesSwapCorrectly()
        {
            var block = new Block(CreateSixFaceBlock());
            var rotated = block.Rotate(RotateAxis.X);

            // X軸: Front -> Up, Up -> Back, Back -> Down, Down -> Front (L/R不変)
            Assert.AreEqual(BlockColor.Green, rotated.GetColor(BlockFace.Up), "Front の色が Up へ移動すること");
            Assert.AreEqual(BlockColor.White, rotated.GetColor(BlockFace.Back), "Up の色が Back へ移動すること");
            Assert.AreEqual(BlockColor.Blue, rotated.GetColor(BlockFace.Down), "Back の色が Down へ移動すること");
            Assert.AreEqual(BlockColor.Yellow, rotated.GetColor(BlockFace.Front), "Down の色が Front へ移動すること");
            Assert.AreEqual(BlockColor.Orange, rotated.GetColor(BlockFace.Left), "Left は不変であること");
            Assert.AreEqual(BlockColor.Red, rotated.GetColor(BlockFace.Right), "Right は不変であること");
        }

        [Test]
        public void RotateY_FrontColorMovesToLeft_AndFourFacesSwapCorrectly()
        {
            var block = new Block(CreateSixFaceBlock());
            var rotated = block.Rotate(RotateAxis.Y);

            // Y軸: Front -> Left, Left -> Back, Back -> Right, Right -> Front (U/D不変)
            Assert.AreEqual(BlockColor.Green, rotated.GetColor(BlockFace.Left), "Front の色が Left へ移動すること");
            Assert.AreEqual(BlockColor.Orange, rotated.GetColor(BlockFace.Back), "Left の色が Back へ移動すること");
            Assert.AreEqual(BlockColor.Blue, rotated.GetColor(BlockFace.Right), "Back の色が Right へ移動すること");
            Assert.AreEqual(BlockColor.Red, rotated.GetColor(BlockFace.Front), "Right の色が Front へ移動すること");
            Assert.AreEqual(BlockColor.White, rotated.GetColor(BlockFace.Up), "Up は不変であること");
            Assert.AreEqual(BlockColor.Yellow, rotated.GetColor(BlockFace.Down), "Down は不変であること");
        }

        [Test]
        public void RotateZ_UpColorMovesToLeft_AndFourFacesSwapCorrectly()
        {
            var block = new Block(CreateSixFaceBlock());
            var rotated = block.Rotate(RotateAxis.Z);

            // Z軸（手前から見て時計回り）: Up -> Left, Left -> Down, Down -> Right, Right -> Up (F/B不変)
            Assert.AreEqual(BlockColor.White, rotated.GetColor(BlockFace.Left), "Up の色が Left へ移動すること");
            Assert.AreEqual(BlockColor.Orange, rotated.GetColor(BlockFace.Down), "Left の色が Down へ移動すること");
            Assert.AreEqual(BlockColor.Yellow, rotated.GetColor(BlockFace.Right), "Down の色が Right へ移動すること");
            Assert.AreEqual(BlockColor.Red, rotated.GetColor(BlockFace.Up), "Right の色が Up へ移動すること");
            Assert.AreEqual(BlockColor.Green, rotated.GetColor(BlockFace.Front), "Front は不変であること");
            Assert.AreEqual(BlockColor.Blue, rotated.GetColor(BlockFace.Back), "Back は不変であること");
        }
    }
}

