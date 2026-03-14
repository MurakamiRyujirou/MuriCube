using System.Collections.Generic;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Cube.Enums;
using NUnit.Framework;

namespace Domain.Tests
{
    // Cube（BlockGroup を保持）の Rotate(axis, turn, pivot) が公転・自転ともに正しく動作することを検証する
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
            var group = new Domain.Cube.BlockGroup(blocks);

            Assert.AreEqual(1, group.Blocks.Count, "Blocks の件数");
            Assert.IsTrue(group.Blocks.ContainsKey(new BlockPosition(0, 0, 0)), "座標 (0,0,0) が存在すること");
            Assert.AreEqual(BlockColor.Green, group.Blocks[new BlockPosition(0, 0, 0)].GetColor(BlockFace.Front), "IBlock として Front の色を取得できること");
        }

        [Test]
        public void Cube_RotateX_Clockwise_WithPivot_PreservesRelativePosition_AndUpdatesFaceColors()
        {
            // X 軸 Clockwise。Pivot (1, 0.5, 0.5) で 2 ブロック (1,0,0), (1,1,0) を回転
            // 相対→回転→復元: (1,0,0)→(1,1,0), (1,1,0)→(1,1,1)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 1, 0)] = CreateStandardBlock(),
            };
            var cube = new Domain.Cube.Cube(new Domain.Cube.BlockGroup(blocks));
            var pivot = new Domain.Cube.PivotPosition(1f, 0.5f, 0.5f);
            var rotated = cube.Rotate(RotateAxis.X, CubeTurn.Clockwise, pivot);

            var rotatedBlocks = rotated.BlockGroup.Blocks;
            Assert.AreEqual(2, rotatedBlocks.Count, "ブロック数は不変");
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(1, 1, 0)), "(1,0,0) が (1,1,0) へ移動していること");
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(1, 1, 1)), "(1,1,0) が (1,1,1) へ移動していること");

            // X 軸 Clockwise 1回: Front→Up。移動したブロックの上面は元の正面（Green）
            var movedBlock = rotatedBlocks[new BlockPosition(1, 1, 0)];
            Assert.AreEqual(BlockColor.Green, movedBlock.GetColor(BlockFace.Up), "X 回転後: Front の色が Up へ移動していること");
        }

        [Test]
        public void Cube_RotateY_Clockwise_WithPivot_PreservesRelativePosition_AndUpdatesFaceColors()
        {
            // Y 軸 Clockwise。Pivot (0.5, 0, 0.5) で (0,0,0), (1,0,0) を回転
            // 実装の Y 変換に従い移動先を検証
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
            };
            var cube = new Domain.Cube.Cube(new Domain.Cube.BlockGroup(blocks));
            var pivot = new Domain.Cube.PivotPosition(0.5f, 0f, 0.5f);
            var rotated = cube.Rotate(RotateAxis.Y, CubeTurn.Clockwise, pivot);

            var rotatedBlocks = rotated.BlockGroup.Blocks;
            Assert.AreEqual(2, rotatedBlocks.Count, "ブロック数は不変");
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(0, 0, 1)), "(0,0,0) が (0,0,1) へ移動していること");
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(0, 0, 0)), "(1,0,0) が (0,0,0) へ移動していること");

            var movedBlock = rotatedBlocks[new BlockPosition(0, 0, 1)];
            Assert.AreEqual(BlockColor.Green, movedBlock.GetColor(BlockFace.Left), "Y 回転後: Front の色が Left へ移動していること");
        }

        [Test]
        public void Cube_RotateZ_Clockwise_WithPivot_PreservesRelativePosition_AndUpdatesFaceColors()
        {
            // Z 軸 Clockwise。Pivot (0.5, 0.5, 0) で (0,0,0), (1,0,0) を回転
            // (0,0,0)→(1,0,0), (1,0,0)→(1,1,0)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
            };
            var cube = new Domain.Cube.Cube(new Domain.Cube.BlockGroup(blocks));
            var pivot = new Domain.Cube.PivotPosition(0.5f, 0.5f, 0f);
            var rotated = cube.Rotate(RotateAxis.Z, CubeTurn.Clockwise, pivot);

            var rotatedBlocks = rotated.BlockGroup.Blocks;
            Assert.AreEqual(2, rotatedBlocks.Count, "ブロック数は不変");
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(1, 0, 0)), "(0,0,0) が (1,0,0) へ移動していること");
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(1, 1, 0)), "(1,0,0) が (1,1,0) へ移動していること");

            var movedBlock = rotatedBlocks[new BlockPosition(1, 1, 0)];
            Assert.AreEqual(BlockColor.White, movedBlock.GetColor(BlockFace.Right), "Z 回転後: Up の色が Right へ移動していること");
            Assert.AreEqual(BlockColor.Green, movedBlock.GetColor(BlockFace.Front), "Z 回転後: Front は不変であること");
        }

        [Test]
        public void Cube_RotateX_Clockwise_PivotChangesDestination_4x4Example()
        {
            // Domain_Cube 4.4 例: 4x4x4 で (3,3,3) が Pivot により異なる移動先になる
            // Pivot (3, 1.5, 1.5): 相対 (0, 1.5, 1.5) → 新dy=-1.5, 新dz=1.5 → 絶対 (3, 0, 3)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(3, 3, 3)] = CreateStandardBlock(),
            };
            var cube = new Domain.Cube.Cube(new Domain.Cube.BlockGroup(blocks));
            var pivot = new Domain.Cube.PivotPosition(3f, 1.5f, 1.5f);
            var rotated = cube.Rotate(RotateAxis.X, CubeTurn.Clockwise, pivot);

            var rotatedBlocks = rotated.BlockGroup.Blocks;
            Assert.AreEqual(1, rotatedBlocks.Count);
            Assert.IsTrue(rotatedBlocks.ContainsKey(new BlockPosition(3, 0, 3)), "(3,3,3) が Pivot (3,1.5,1.5) で (3,0,3) へ移動すること");
        }
    }
}
