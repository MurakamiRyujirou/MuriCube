using System;
using System.Collections.Generic;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Cube;
using Domain.Tetris;

namespace Application
{
    // MinoType ごとの固定形状・標準配色・Pivot で ActiveMino を生成する（Application_MinoFactory.md）
    public static class MinoFactory
    {
        private static readonly CubePosition ZeroOffset = new CubePosition(0, 0, 0);

        private static readonly Block StandardBlock = CreateStandardBlock();

        private static readonly PivotPosition PivotI = new PivotPosition(1.5f, 0.5f, 0.5f);
        private static readonly PivotPosition PivotO = new PivotPosition(0.5f, 0.5f, 0.5f);
        private static readonly PivotPosition PivotTetrominoStandard = new PivotPosition(1.5f, 0.5f, 0.5f);

        public static ActiveMino Create(MinoType type)
        {
            switch (type)
            {
                case MinoType.I:
                    return new ActiveMino(
                        type,
                        Group(
                            P(0, 0, 0), P(1, 0, 0), P(2, 0, 0), P(3, 0, 0),
                            P(0, 0, 1), P(1, 0, 1), P(2, 0, 1), P(3, 0, 1)),
                        ZeroOffset,
                        PivotI);
                case MinoType.O:
                    return new ActiveMino(
                        type,
                        Group(
                            P(0, 0, 0), P(1, 0, 0), P(0, 1, 0), P(1, 1, 0),
                            P(0, 0, 1), P(1, 0, 1), P(0, 1, 1), P(1, 1, 1)),
                        ZeroOffset,
                        PivotO);
                case MinoType.T:
                    return new ActiveMino(
                        type,
                        Group(
                            P(0, 0, 0), P(1, 0, 0), P(2, 0, 0), P(1, 1, 0),
                            P(0, 0, 1), P(1, 0, 1), P(2, 0, 1), P(1, 1, 1)),
                        ZeroOffset,
                        PivotTetrominoStandard);
                case MinoType.S:
                    return new ActiveMino(
                        type,
                        Group(
                            P(0, 0, 0), P(1, 0, 0), P(1, 1, 0), P(2, 1, 0),
                            P(0, 0, 1), P(1, 0, 1), P(1, 1, 1), P(2, 1, 1)),
                        ZeroOffset,
                        PivotTetrominoStandard);
                case MinoType.Z:
                    return new ActiveMino(
                        type,
                        Group(
                            P(1, 0, 0), P(2, 0, 0), P(0, 1, 0), P(1, 1, 0),
                            P(1, 0, 1), P(2, 0, 1), P(0, 1, 1), P(1, 1, 1)),
                        ZeroOffset,
                        PivotTetrominoStandard);
                case MinoType.J:
                    return new ActiveMino(
                        type,
                        Group(
                            P(0, 0, 0), P(1, 0, 0), P(2, 0, 0), P(0, 1, 0),
                            P(0, 0, 1), P(1, 0, 1), P(2, 0, 1), P(0, 1, 1)),
                        ZeroOffset,
                        PivotTetrominoStandard);
                case MinoType.L:
                    return new ActiveMino(
                        type,
                        Group(
                            P(0, 0, 0), P(1, 0, 0), P(2, 0, 0), P(2, 1, 0),
                            P(0, 0, 1), P(1, 0, 1), P(2, 0, 1), P(2, 1, 1)),
                        ZeroOffset,
                        PivotTetrominoStandard);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static Block CreateStandardBlock()
        {
            return new Block(new Dictionary<BlockFace, BlockColor>
            {
                [BlockFace.Up] = BlockColor.White,
                [BlockFace.Down] = BlockColor.Yellow,
                [BlockFace.Front] = BlockColor.Green,
                [BlockFace.Back] = BlockColor.Blue,
                [BlockFace.Left] = BlockColor.Orange,
                [BlockFace.Right] = BlockColor.Red,
            });
        }

        private static BlockPosition P(float x, float y, float z) => new BlockPosition(x, y, z);

        private static BlockGroup Group(params BlockPosition[] cells)
        {
            var dict = new Dictionary<BlockPosition, Block>(cells.Length);
            foreach (var c in cells)
                dict[c] = StandardBlock;
            return new BlockGroup(dict);
        }
    }
}
