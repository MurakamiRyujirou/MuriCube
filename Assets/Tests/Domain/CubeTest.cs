using System.Collections.Generic;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Cube.Enums;
using NUnit.Framework;

namespace Domain.Tests
{
    // Cube の Rotate(axis, turn, pivot) が公転・自転ともに正しく動作することを検証する
    public class CubeTest
    {
        // Up=White, Down=Yellow, Left=Orange, Right=Red, Front=Green, Back=Blue
        private static Domain.Cube.Block CreateStandardBlock()
        {
            return new Domain.Cube.Block(new Dictionary<BlockFace, BlockColor>
            {
                [BlockFace.Up]    = BlockColor.White,
                [BlockFace.Down]  = BlockColor.Yellow,
                [BlockFace.Left]  = BlockColor.Orange,
                [BlockFace.Right] = BlockColor.Red,
                [BlockFace.Front] = BlockColor.Green,
                [BlockFace.Back]  = BlockColor.Blue,
            });
        }

        // -----------------------------------------------------------------------
        // BlockGroup
        // -----------------------------------------------------------------------

        [Test]
        public void BlockGroup_ExposesBlocksAsIBlockGroup()
        {
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
            };
            var group = new Domain.Cube.BlockGroup(blocks);

            Assert.AreEqual(1, group.Blocks.Count, "Blocks の件数");
            Assert.IsTrue(group.Blocks.ContainsKey(new BlockPosition(0, 0, 0)), "座標 (0,0,0) が存在すること");
            Assert.AreEqual(BlockColor.Green, group.Blocks[new BlockPosition(0, 0, 0)].GetColor(BlockFace.Front));
        }

        // -----------------------------------------------------------------------
        // 空の Cube
        // -----------------------------------------------------------------------

        [Test]
        public void Cube_Rotate_EmptyBlockGroup_ReturnsSameInstance()
        {
            var cube = new Domain.Cube.Cube(new Domain.Cube.BlockGroup(new Dictionary<BlockPosition, Domain.Cube.Block>()));
            var pivot = new Domain.Cube.PivotPosition(0f, 0f, 0f);
            var result = cube.Rotate(RotateAxis.X, CubeTurn.Clockwise, pivot);
            Assert.AreSame(cube, result, "空の BlockGroup は同インスタンスを返すこと");
        }

        // -----------------------------------------------------------------------
        // X 軸回転
        // -----------------------------------------------------------------------

        [Test]
        public void Cube_RotateX_Clockwise_公転と自転()
        {
            // Pivot (1, 0.5, 0.5) で (1,0,0)→(1,1,0)、(1,1,0)→(1,1,1) に移動
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 1, 0)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, RotateAxis.X, CubeTurn.Clockwise, 1f, 0.5f, 0.5f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(1, 1, 0)), "(1,0,0)→(1,1,0)");
            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(1, 1, 1)), "(1,1,0)→(1,1,1)");
            // 自転: Front(Green)→Up
            Assert.AreEqual(BlockColor.Green, rotated[new BlockPosition(1, 1, 0)].GetColor(BlockFace.Up));
        }

        [Test]
        public void Cube_RotateX_CounterClockwise_公転と自転()
        {
            // Clockwise の逆: (1,0,0)→(1,0,1)、(1,1,0)→(1,1,1) → CCW なので
            // CCW: 新dy=dz, 新dz=-dy
            // (1,0,0): 相対(0,-0.5,-0.5) → CCW → (0,-0.5,0.5) → (1,0,1)
            // (1,1,0): 相対(0,0.5,-0.5)  → CCW → (0,-0.5,-0.5) は間違い
            // CCW(a,b)=(b,-a): dy=-0.5,dz=-0.5 → ndy=-0.5,ndz=0.5 → (1,0,1) ✓
            // (1,1,0): dy=0.5,dz=-0.5 → ndy=-0.5,ndz=-0.5 → (1,0,0) ✓
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 1, 0)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, RotateAxis.X, CubeTurn.CounterClockwise, 1f, 0.5f, 0.5f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(1, 0, 1)), "(1,0,0)→(1,0,1)");
            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(1, 0, 0)), "(1,1,0)→(1,0,0)");
            // 自転CCW = Clockwise×3: Front→Down→Back→Up の3ステップ = Front→Down
            Assert.AreEqual(BlockColor.Green, rotated[new BlockPosition(1, 0, 1)].GetColor(BlockFace.Down));
        }

        [Test]
        public void Cube_RotateX_HalfTurn_公転と自転()
        {
            // HalfTurn: 新dy=-dy, 新dz=-dz
            // (1,0,0): 相対(0,-0.5,-0.5) → (0,0.5,0.5) → (1,1,1)
            // (1,1,0): 相対(0,0.5,-0.5)  → (0,-0.5,0.5) → (1,0,1)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 1, 0)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, RotateAxis.X, CubeTurn.HalfTurn, 1f, 0.5f, 0.5f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(1, 1, 1)), "(1,0,0)→(1,1,1)");
            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(1, 0, 1)), "(1,1,0)→(1,0,1)");
            // 自転HalfTurn=Clockwise×2: Front→Up→Back
            Assert.AreEqual(BlockColor.Green, rotated[new BlockPosition(1, 1, 1)].GetColor(BlockFace.Back));
        }

        [Test]
        public void Cube_RotateX_Clockwise_4回転で元に戻る()
        {
            var original = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
            };
            var cube  = MakeCube(original);
            var pivot = new Domain.Cube.PivotPosition(1f, 0.5f, 0.5f);

            var result = cube
                .Rotate(RotateAxis.X, CubeTurn.Clockwise, pivot)
                .Rotate(RotateAxis.X, CubeTurn.Clockwise, pivot)
                .Rotate(RotateAxis.X, CubeTurn.Clockwise, pivot)
                .Rotate(RotateAxis.X, CubeTurn.Clockwise, pivot);

            Assert.IsTrue(result.BlockGroup.Blocks.ContainsKey(new BlockPosition(1, 0, 0)), "4回転後に元の座標へ戻ること");
            Assert.AreEqual(BlockColor.Green, result.BlockGroup.Blocks[new BlockPosition(1, 0, 0)].GetColor(BlockFace.Front), "4回転後に面の色が元に戻ること");
        }

        [Test]
        public void Cube_RotateX_Clockwise_実測値_3_3_3_to_3_minus2_3()
        {
            // Unity 実測: Pivot(3, 0.5, 0.5) で (3,3,3) → (3,-2,3)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(3, 3, 3)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, RotateAxis.X, CubeTurn.Clockwise, 3f, 0.5f, 0.5f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(3, -2, 3)), "実測値: (3,3,3)→(3,-2,3)");
        }

        // -----------------------------------------------------------------------
        // Y 軸回転
        // -----------------------------------------------------------------------

        [Test]
        public void Cube_RotateY_Clockwise_公転と自転()
        {
            // Pivot(0.5, 0, 0.5): Y軸CW: 新dx=dz, 新dz=-dx
            // (0,0,0): dx=-0.5,dz=-0.5 → ndx=-0.5,ndz=0.5 → (0,0,1)
            // (1,0,0): dx=0.5,dz=-0.5  → ndx=-0.5,ndz=-0.5 → (0,0,0)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, RotateAxis.Y, CubeTurn.Clockwise, 0.5f, 0f, 0.5f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(0, 0, 1)), "(0,0,0)→(0,0,1)");
            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(0, 0, 0)), "(1,0,0)→(0,0,0)");
            // 自転Y CW: Front→Left
            Assert.AreEqual(BlockColor.Green, rotated[new BlockPosition(0, 0, 1)].GetColor(BlockFace.Left));
        }

        [Test]
        public void Cube_RotateY_Clockwise_4回転で元に戻る()
        {
            var original = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
            };
            var cube  = MakeCube(original);
            var pivot = new Domain.Cube.PivotPosition(0.5f, 0f, 0.5f);

            var result = cube
                .Rotate(RotateAxis.Y, CubeTurn.Clockwise, pivot)
                .Rotate(RotateAxis.Y, CubeTurn.Clockwise, pivot)
                .Rotate(RotateAxis.Y, CubeTurn.Clockwise, pivot)
                .Rotate(RotateAxis.Y, CubeTurn.Clockwise, pivot);

            Assert.IsTrue(result.BlockGroup.Blocks.ContainsKey(new BlockPosition(0, 0, 0)), "4回転後に元の座標へ戻ること");
            Assert.AreEqual(BlockColor.Green, result.BlockGroup.Blocks[new BlockPosition(0, 0, 0)].GetColor(BlockFace.Front), "4回転後に面の色が元に戻ること");
        }

        // -----------------------------------------------------------------------
        // Z 軸回転
        // -----------------------------------------------------------------------

        [Test]
        public void Cube_RotateZ_Clockwise_公転と自転()
        {
            // Pivot(0.5, 0.5, 0): Z軸CW: 新dx=-dy, 新dy=dx
            // (0,0,0): dx=-0.5,dy=-0.5 → ndx=0.5,ndy=-0.5 → (1,0,0)
            // (1,0,0): dx=0.5,dy=-0.5  → ndx=0.5,ndy=0.5  → (1,1,0)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, RotateAxis.Z, CubeTurn.Clockwise, 0.5f, 0.5f, 0f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(1, 0, 0)), "(0,0,0)→(1,0,0)");
            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(1, 1, 0)), "(1,0,0)→(1,1,0)");
            // 自転Z CW: Up→Right
            Assert.AreEqual(BlockColor.White, rotated[new BlockPosition(1, 1, 0)].GetColor(BlockFace.Right));
            // Front は Z 軸回転で不変
            Assert.AreEqual(BlockColor.Green, rotated[new BlockPosition(1, 1, 0)].GetColor(BlockFace.Front));
        }

        [Test]
        public void Cube_RotateZ_Clockwise_4回転で元に戻る()
        {
            var original = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
            };
            var cube  = MakeCube(original);
            var pivot = new Domain.Cube.PivotPosition(0.5f, 0.5f, 0f);

            var result = cube
                .Rotate(RotateAxis.Z, CubeTurn.Clockwise, pivot)
                .Rotate(RotateAxis.Z, CubeTurn.Clockwise, pivot)
                .Rotate(RotateAxis.Z, CubeTurn.Clockwise, pivot)
                .Rotate(RotateAxis.Z, CubeTurn.Clockwise, pivot);

            Assert.IsTrue(result.BlockGroup.Blocks.ContainsKey(new BlockPosition(0, 0, 0)), "4回転後に元の座標へ戻ること");
            Assert.AreEqual(BlockColor.Green, result.BlockGroup.Blocks[new BlockPosition(0, 0, 0)].GetColor(BlockFace.Front), "4回転後に面の色が元に戻ること");
        }

        // -----------------------------------------------------------------------
        // Pivot の影響（4x4x4 具体例）
        // -----------------------------------------------------------------------

        [Test]
        public void Cube_RotateX_Clockwise_Pivot変化で移動先が変わる()
        {
            var blocksA = new Dictionary<BlockPosition, Domain.Cube.Block> { [new BlockPosition(3, 3, 3)] = CreateStandardBlock() };
            var blocksB = new Dictionary<BlockPosition, Domain.Cube.Block> { [new BlockPosition(3, 3, 3)] = CreateStandardBlock() };

            // Pivot(3, 0.5, 0.5) → (3,-2,3)
            var rotatedA = Rotate(blocksA, RotateAxis.X, CubeTurn.Clockwise, 3f, 0.5f, 0.5f);
            Assert.IsTrue(rotatedA.ContainsKey(new BlockPosition(3, -2, 3)), "Pivot(3,0.5,0.5) → (3,-2,3)");

            // Pivot(3, 1.5, 1.5) → (3,0,3)
            var rotatedB = Rotate(blocksB, RotateAxis.X, CubeTurn.Clockwise, 3f, 1.5f, 1.5f);
            Assert.IsTrue(rotatedB.ContainsKey(new BlockPosition(3, 0, 3)), "Pivot(3,1.5,1.5) → (3,0,3)");
        }

        // -----------------------------------------------------------------------
        // ヘルパー
        // -----------------------------------------------------------------------

        private static IReadOnlyDictionary<BlockPosition, IBlock> Rotate(
            Dictionary<BlockPosition, Domain.Cube.Block> blocks,
            RotateAxis axis,
            CubeTurn turn,
            float px, float py, float pz)
        {
            var cube  = MakeCube(blocks);
            var pivot = new Domain.Cube.PivotPosition(px, py, pz);
            return cube.Rotate(axis, turn, pivot).BlockGroup.Blocks;
        }

        private static Domain.Cube.Cube MakeCube(Dictionary<BlockPosition, Domain.Cube.Block> blocks)
            => new Domain.Cube.Cube(new Domain.Cube.BlockGroup(blocks));
    }
}