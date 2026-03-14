using System.Collections.Generic;
using Domain.Common.Enums;
using Domain.Cube.Enums;
using NUnit.Framework;

namespace Domain.Tests
{
    // Cube の X/Y/Z 軸回転が TechSpecs 3.2 のスワップロジック通りに動作することを検証する
    public class CubeRotationTest
    {
        private static IReadOnlyDictionary<BlockFace, BlockColor> CreateSixFaceCube()
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
        public void RotateX_FrontColorMovesToDown_AndFourFacesSwapCorrectly()
        {
            var cube = new Domain.Cube.Cube(CreateSixFaceCube());
            var rotated = cube.Rotate(RotateAxis.X);

            // X軸: Front -> Down, Down -> Back, Back -> Up, Up -> Front (L/R不変)
            Assert.AreEqual(BlockColor.Green, rotated.GetColor(BlockFace.Down), "Front の色が Down へ移動すること");
            Assert.AreEqual(BlockColor.Yellow, rotated.GetColor(BlockFace.Back), "Down の色が Back へ移動すること");
            Assert.AreEqual(BlockColor.Blue, rotated.GetColor(BlockFace.Up), "Back の色が Up へ移動すること");
            Assert.AreEqual(BlockColor.White, rotated.GetColor(BlockFace.Front), "Up の色が Front へ移動すること");
            Assert.AreEqual(BlockColor.Orange, rotated.GetColor(BlockFace.Left), "Left は不変であること");
            Assert.AreEqual(BlockColor.Red, rotated.GetColor(BlockFace.Right), "Right は不変であること");
        }

        [Test]
        public void RotateY_FrontColorMovesToLeft_AndFourFacesSwapCorrectly()
        {
            var cube = new Domain.Cube.Cube(CreateSixFaceCube());
            var rotated = cube.Rotate(RotateAxis.Y);

            // Y軸: Front -> Left, Left -> Back, Back -> Right, Right -> Front (U/D不変)
            Assert.AreEqual(BlockColor.Green, rotated.GetColor(BlockFace.Left), "Front の色が Left へ移動すること");
            Assert.AreEqual(BlockColor.Orange, rotated.GetColor(BlockFace.Back), "Left の色が Back へ移動すること");
            Assert.AreEqual(BlockColor.Blue, rotated.GetColor(BlockFace.Right), "Back の色が Right へ移動すること");
            Assert.AreEqual(BlockColor.Red, rotated.GetColor(BlockFace.Front), "Right の色が Front へ移動すること");
            Assert.AreEqual(BlockColor.White, rotated.GetColor(BlockFace.Up), "Up は不変であること");
            Assert.AreEqual(BlockColor.Yellow, rotated.GetColor(BlockFace.Down), "Down は不変であること");
        }

        [Test]
        public void RotateZ_UpColorMovesToRight_AndFourFacesSwapCorrectly()
        {
            var cube = new Domain.Cube.Cube(CreateSixFaceCube());
            var rotated = cube.Rotate(RotateAxis.Z);

            // Z軸: Up -> Right, Right -> Down, Down -> Left, Left -> Up (F/B不変)
            Assert.AreEqual(BlockColor.White, rotated.GetColor(BlockFace.Right), "Up の色が Right へ移動すること");
            Assert.AreEqual(BlockColor.Red, rotated.GetColor(BlockFace.Down), "Right の色が Down へ移動すること");
            Assert.AreEqual(BlockColor.Yellow, rotated.GetColor(BlockFace.Left), "Down の色が Left へ移動すること");
            Assert.AreEqual(BlockColor.Orange, rotated.GetColor(BlockFace.Up), "Left の色が Up へ移動すること");
            Assert.AreEqual(BlockColor.Green, rotated.GetColor(BlockFace.Front), "Front は不変であること");
            Assert.AreEqual(BlockColor.Blue, rotated.GetColor(BlockFace.Back), "Back は不変であること");
        }
    }
}
