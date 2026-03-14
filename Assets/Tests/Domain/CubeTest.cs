using System.Collections.Generic;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Cube;
using Domain.Cube.Enums;
using NUnit.Framework;

namespace Domain.Tests
{
    // Cube（BlockGroup を保持）の回転が座標入れ替えと Block 自転で正しく動作することを検証する
    public class CubeTest
    {
        private static Domain.Cube.Block CreateStandardBlock()
        {
            var colors = new Dictionary<BlockFace, BlockColor>
            {
                [BlockFace.Up] = BlockColor.White,
                [BlockFace.Down] = BlockColor.Yellow,
                [BlockFace.Left] = BlockColor.Orange,
                [BlockFace.Right] = BlockColor.Red,
                [BlockFace.Front] = BlockColor.Green,
                [BlockFace.Back] = BlockColor.Blue,
            };
            return new Domain.Cube.Block(colors);
        }

        [Test]
        public void BlockGroup_ExposesBlocksAsIBlockGroup()
        {
            var block = CreateStandardBlock();
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = block,
            };
            var group = new BlockGroup(blocks);

            Assert.AreEqual(1, group.Blocks.Count, "Blocks の件数");
            Assert.IsTrue(group.Blocks.ContainsKey(new BlockPosition(0, 0, 0)), "座標 (0,0,0) が存在すること");
            Assert.AreEqual(BlockColor.Green, group.Blocks[new BlockPosition(0, 0, 0)].GetColor(BlockFace.Front), "IBlock として Front の色を取得できること");
        }

        [Test]
        public void Cube_RotateX_PreservesRelativePosition_AndUpdatesFaceColors()
        {
            // X 軸回転（R 回転）。同一 X レイヤー内で (1,0,0) → (1,1,0) など Y-Z が時計回りに入れ替わる
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 1, 0)] = CreateStandardBlock(),
            };
            var cube = new Cube(new BlockGroup(blocks));
            var rotated = cube.Rotate(RotateAxis.X);

            var rotatedBlocks = rotated.BlockGroup.Blocks;
            Assert.AreEqual(2, rotatedBlocks.Count, "ブロック数は不変");
            // X 軸: Y-Z 平面で時計回り。(1,0,0) → (1,1,0), (1,1,0) → (1,1,1)
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(1, 1, 0)), "(1,0,0) が (1,1,0) へ移動していること");
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(1, 1, 1)), "(1,1,0) が (1,1,1) へ移動していること");

            // 移動したブロックは X 軸自転済み。Front→Down なので、元の Green が Down に来る
            var movedBlock = rotatedBlocks[new BlockPosition(1, 1, 0)];
            Assert.AreEqual(BlockColor.Green, movedBlock.GetColor(BlockFace.Down), "X 回転後: Front の色が Down へ移動していること");
        }

        [Test]
        public void Cube_RotateY_PreservesRelativePosition_AndUpdatesFaceColors()
        {
            // Y 軸回転。同一 Y レイヤー内で X-Z が時計回りに入れ替わる。(0,0,0) と (1,0,0) は入れ替わる
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
            };
            var cube = new Cube(new BlockGroup(blocks));
            var rotated = cube.Rotate(RotateAxis.Y);

            var rotatedBlocks = rotated.BlockGroup.Blocks;
            Assert.AreEqual(2, rotatedBlocks.Count, "ブロック数は不変");
            // (0,0,0) → (1,0,0), (1,0,0) → (0,0,1)
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(1, 0, 0)), "(0,0,0) が (1,0,0) へ移動していること");
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(0, 0, 1)), "(1,0,0) が (0,0,1) へ移動していること");

            var movedBlock = rotatedBlocks[new BlockPosition(1, 0, 0)];
            Assert.AreEqual(BlockColor.Green, movedBlock.GetColor(BlockFace.Left), "Y 回転後: Front の色が Left へ移動していること");
        }

        [Test]
        public void Cube_RotateZ_PreservesRelativePosition_AndUpdatesFaceColors()
        {
            // Z 軸回転。同一 Z レイヤー内で (1,0,0) → (1,1,0) など X-Y が時計回りに入れ替わる
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
            };
            var group = new BlockGroup(blocks);
            var cube = new Cube(group);
            var rotated = cube.Rotate(RotateAxis.Z);

            var rotatedBlocks = rotated.BlockGroup.Blocks;
            Assert.AreEqual(2, rotatedBlocks.Count, "ブロック数は不変");
            // (0,0,0) → (1,0,0), (1,0,0) → (1,1,0)
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(1, 0, 0)), "(0,0,0) が (1,0,0) へ移動していること");
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(1, 1, 0)), "(1,0,0) が (1,1,0) へ移動していること");

            var movedBlock = rotatedBlocks[new BlockPosition(1, 1, 0)];
            Assert.AreEqual(BlockColor.White, movedBlock.GetColor(BlockFace.Right), "Z 回転後: Up の色が Right へ移動していること");
            Assert.AreEqual(BlockColor.Green, movedBlock.GetColor(BlockFace.Front), "Z 回転後: Front は不変であること");
        }
    }
}
