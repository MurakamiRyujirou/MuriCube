using System.Collections.Generic;
using System.Linq;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Cube.Enums;
using NUnit.Framework;

namespace Domain.Tests
{
    // Cube の Rotate(CubeOperation, pivot) が公転・自転ともに正しく動作することを検証する
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
            var result = cube.Rotate(CubeOperation.R, pivot);
            Assert.AreSame(cube, result, "空の BlockGroup は同インスタンスを返すこと");
        }

        [Test]
        public void Cube_GetAffectedBlocks_境界比較で対象を返す()
        {
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 1, 0)] = CreateStandardBlock(),
                [new BlockPosition(2, 0, 1)] = CreateStandardBlock(),
            };
            var cube = MakeCube(blocks);
            var pivot = new Domain.Cube.PivotPosition(1f, 0.5f, 0.5f);

            var xAffected = cube.GetAffectedBlocks(CubeOperation.R, pivot); // R: pos.X > 1 → (2,0,1)
            Assert.AreEqual(1, xAffected.Count);
            Assert.IsTrue(xAffected.Contains(new BlockPosition(2, 0, 1)));

            var yAffected = cube.GetAffectedBlocks(CubeOperation.U, pivot); // U: pos.Y > 0.5 → (1,1,0)
            Assert.AreEqual(1, yAffected.Count);
            Assert.IsTrue(yAffected.Contains(new BlockPosition(1, 1, 0)));

            var zAffected = cube.GetAffectedBlocks(CubeOperation.F, pivot); // F: pos.Z < 0.5 → (0,0,0), (1,1,0)
            Assert.AreEqual(2, zAffected.Count);
            Assert.IsTrue(zAffected.Contains(new BlockPosition(0, 0, 0)));
            Assert.IsTrue(zAffected.Contains(new BlockPosition(1, 1, 0)));
        }

        // -----------------------------------------------------------------------
        // X 軸回転
        // -----------------------------------------------------------------------

        [Test]
        public void Cube_RotateX_Clockwise_公転と自転()
        {
            // 境界比較: Block.X > 1 なので (2,0,0),(2,1,0) が対象。Pivot(1,0.5,0.5) で (ndz,ndy)=Rotate2D(dz,dy) により (2,0,0)→(2,1,0)、(2,1,0)→(2,1,1)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(2, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(2, 1, 0)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, CubeOperation.R, 1f, 0.5f, 0.5f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(2, 1, 0)), "(2,0,0)→(2,1,0)");
            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(2, 1, 1)), "(2,1,0)→(2,1,1)");
            // 自転: (2,1,0) のブロックは元 (2,0,0)。Front(Green)→Up
            Assert.AreEqual(BlockColor.Green, rotated[new BlockPosition(2, 1, 0)].GetColor(BlockFace.Up));
        }

        [Test]
        public void Cube_RotateRi_右レイヤーはRと同じブロックが対象()
        {
            var pivot = new Domain.Cube.PivotPosition(1f, 0.5f, 0.5f);
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(2, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
            };
            var cube = MakeCube(blocks);
            var r = cube.GetAffectedBlocks(CubeOperation.R, pivot);
            var ri = cube.GetAffectedBlocks(CubeOperation.Ri, pivot);
            Assert.AreEqual(r.Count, ri.Count);
            foreach (var p in r)
                Assert.IsTrue(ri.Contains(p));
        }

        [Test]
        public void Cube_RotateL_公転と自転()
        {
            // L: 左レイヤー pos.X < 1 の (0,0,0),(0,1,0)。Pivot(1,0.5,0.5) で (0,0,0)→(0,0,1)、(0,1,0)→(0,0,0)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(0, 1, 0)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, CubeOperation.L, 1f, 0.5f, 0.5f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(0, 0, 1)), "(0,0,0)→(0,0,1)");
            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(0, 0, 0)), "(0,1,0)→(0,0,0)");
            // 自転CCW: (0,0,1) のブロックは元 (0,0,0)。Front→Down
            Assert.AreEqual(BlockColor.Green, rotated[new BlockPosition(0, 0, 1)].GetColor(BlockFace.Down));
        }

        [Test]
        public void Cube_RotateX_HalfTurn_公転と自転()
        {
            // Block.X > 1 の (2,0,0),(2,1,0) が対象。R を2回で 180°（HalfTurn 相当）
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(2, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(2, 1, 0)] = CreateStandardBlock(),
            };
            var cube = MakeCube(blocks);
            var pivot = new Domain.Cube.PivotPosition(1f, 0.5f, 0.5f);
            var rotated = cube.Rotate(CubeOperation.R, pivot).Rotate(CubeOperation.R, pivot).Blocks;

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(2, 1, 1)), "(2,0,0)→(2,1,1)");
            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(2, 0, 1)), "(2,1,0)→(2,0,1)");
            // 自転HalfTurn=Clockwise×2: Front→Up→Back
            Assert.AreEqual(BlockColor.Green, rotated[new BlockPosition(2, 1, 1)].GetColor(BlockFace.Back));
        }

        [Test]
        public void Cube_RotateX_Clockwise_4回転で元に戻る()
        {
            // Block.X > 1 なので (2,0,0) が回転対象
            var original = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(2, 0, 0)] = CreateStandardBlock(),
            };
            var cube  = MakeCube(original);
            var pivot = new Domain.Cube.PivotPosition(1f, 0.5f, 0.5f);

            var result = cube
                .Rotate(CubeOperation.R, pivot)
                .Rotate(CubeOperation.R, pivot)
                .Rotate(CubeOperation.R, pivot)
                .Rotate(CubeOperation.R, pivot);

            Assert.IsTrue(result.Blocks.ContainsKey(new BlockPosition(2, 0, 0)), "4回転後に元の座標へ戻ること");
            Assert.AreEqual(BlockColor.Green, result.Blocks[new BlockPosition(2, 0, 0)].GetColor(BlockFace.Front), "4回転後に面の色が元に戻ること");
        }

        [Test]
        public void Cube_RotateX_Clockwise_実測値_3_3_3_to_3_minus2_3()
        {
            // 境界比較: Block.X > 2 なので Pivot(2, 0.5, 0.5) で (3,3,3) が対象 → (3,-2,3)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(3, 3, 3)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, CubeOperation.R, 2f, 0.5f, 0.5f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(3, -2, 3)), "実測値: (3,3,3)→(3,-2,3)");
        }

        // -----------------------------------------------------------------------
        // Y 軸回転
        // -----------------------------------------------------------------------

        [Test]
        public void Cube_RotateY_Clockwise_公転と自転()
        {
            // 境界比較: Block.Y > 0 なので (0,1,0),(1,1,0) が対象。Pivot(0.5, 0, 0.5) で (0,1,0)→(0,1,1)、(1,1,0)→(0,1,0)
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 1, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 1, 0)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, CubeOperation.U, 0.5f, 0f, 0.5f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(0, 1, 1)), "(0,1,0)→(0,1,1)");
            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(0, 1, 0)), "(1,1,0)→(0,1,0)");
            // 自転Y CW: Front→Left
            Assert.AreEqual(BlockColor.Green, rotated[new BlockPosition(0, 1, 1)].GetColor(BlockFace.Left));
        }

        [Test]
        public void Cube_RotateY_Clockwise_4回転で元に戻る()
        {
            // Block.Y > 0 なので (0,1,0) が回転対象
            var original = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 1, 0)] = CreateStandardBlock(),
            };
            var cube  = MakeCube(original);
            var pivot = new Domain.Cube.PivotPosition(0.5f, 0f, 0.5f);

            var result = cube
                .Rotate(CubeOperation.U, pivot)
                .Rotate(CubeOperation.U, pivot)
                .Rotate(CubeOperation.U, pivot)
                .Rotate(CubeOperation.U, pivot);

            Assert.IsTrue(result.Blocks.ContainsKey(new BlockPosition(0, 1, 0)), "4回転後に元の座標へ戻ること");
            Assert.AreEqual(BlockColor.Green, result.Blocks[new BlockPosition(0, 1, 0)].GetColor(BlockFace.Front), "4回転後に面の色が元に戻ること");
        }

        // -----------------------------------------------------------------------
        // Z 軸回転
        // -----------------------------------------------------------------------

        [Test]
        public void Cube_RotateZ_Clockwise_公転と自転()
        {
            // 境界比較: Block.Z < pivot.Z なので (0,0,0),(1,0,0) が対象。公転は Rotate2D(dx,dy) のみで dz は不変のため Z は 0 のまま（Domain_Cube.md 4.2）。
            // Pivot(0.5, 0.5, 0.5): (0,0,0)→(0,1,0)、(1,0,0)→(0,0,0)。自転は Z 軸のみ InvertTurn により CubeTurn.Clockwise → 自転は実質 CCW×1（Rotate×3）、Front 不変
            var blocks = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
                [new BlockPosition(1, 0, 0)] = CreateStandardBlock(),
            };
            var rotated = Rotate(blocks, CubeOperation.F, 0.5f, 0.5f, 0.5f);

            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(0, 1, 0)), "(0,0,0)→(0,1,0)");
            Assert.IsTrue(rotated.ContainsKey(new BlockPosition(0, 0, 0)), "(1,0,0)→(0,0,0)");
            // 元 (1,0,0) のブロックは (0,0,0) へ。Right=White, Front=Green
            Assert.AreEqual(BlockColor.White, rotated[new BlockPosition(0, 0, 0)].GetColor(BlockFace.Right));
            Assert.AreEqual(BlockColor.Green, rotated[new BlockPosition(0, 0, 0)].GetColor(BlockFace.Front));
        }

        [Test]
        public void Cube_RotateZ_Clockwise_4回転で元に戻る()
        {
            // Block.Z < 0.5 なので (0,0,0) が回転対象。公転で (0,0,0)→(0,1,0)→(1,1,0)→(1,0,0)→(0,0,0) と 4 手で座標が循環する
            var original = new Dictionary<BlockPosition, Domain.Cube.Block>
            {
                [new BlockPosition(0, 0, 0)] = CreateStandardBlock(),
            };
            var cube  = MakeCube(original);
            var pivot = new Domain.Cube.PivotPosition(0.5f, 0.5f, 0.5f);

            var result = cube
                .Rotate(CubeOperation.F, pivot)
                .Rotate(CubeOperation.F, pivot)
                .Rotate(CubeOperation.F, pivot)
                .Rotate(CubeOperation.F, pivot);

            Assert.IsTrue(result.Blocks.ContainsKey(new BlockPosition(0, 0, 0)), "4回転後に元の座標へ戻ること");
            Assert.AreEqual(BlockColor.Green, result.Blocks[new BlockPosition(0, 0, 0)].GetColor(BlockFace.Front), "4回転後に面の色が元に戻ること");
        }

        // -----------------------------------------------------------------------
        // Pivot の影響（4x4x4 具体例）
        // -----------------------------------------------------------------------

        [Test]
        public void Cube_RotateX_Clockwise_Pivot変化で移動先が変わる()
        {
            var blocksA = new Dictionary<BlockPosition, Domain.Cube.Block> { [new BlockPosition(3, 3, 3)] = CreateStandardBlock() };
            var blocksB = new Dictionary<BlockPosition, Domain.Cube.Block> { [new BlockPosition(3, 3, 3)] = CreateStandardBlock() };

            // 境界: Block.X > 2 で (3,3,3) が対象。Pivot(2, 0.5, 0.5) → (3,-2,3)
            var rotatedA = Rotate(blocksA, CubeOperation.R, 2f, 0.5f, 0.5f);
            Assert.IsTrue(rotatedA.ContainsKey(new BlockPosition(3, -2, 3)), "Pivot(2,0.5,0.5) → (3,-2,3)");

            // Pivot(2, 1.5, 1.5) → (3,0,3)
            var rotatedB = Rotate(blocksB, CubeOperation.R, 2f, 1.5f, 1.5f);
            Assert.IsTrue(rotatedB.ContainsKey(new BlockPosition(3, 0, 3)), "Pivot(2,1.5,1.5) → (3,0,3)");
        }

        // -----------------------------------------------------------------------
        // ヘルパー
        // -----------------------------------------------------------------------

        private static IReadOnlyDictionary<BlockPosition, IBlock> Rotate(
            Dictionary<BlockPosition, Domain.Cube.Block> blocks,
            CubeOperation op,
            float px, float py, float pz)
        {
            var cube  = MakeCube(blocks);
            var pivot = new Domain.Cube.PivotPosition(px, py, pz);
            return cube.Rotate(op, pivot).Blocks;
        }

        private static Domain.Cube.Cube MakeCube(Dictionary<BlockPosition, Domain.Cube.Block> blocks)
            => new Domain.Cube.Cube(new Domain.Cube.BlockGroup(blocks));
    }
}