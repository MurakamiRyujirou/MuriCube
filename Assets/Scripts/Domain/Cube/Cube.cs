using System.Collections.Generic;
using Domain.Common;
using Domain.Cube.Enums;

namespace Domain.Cube
{
    // BlockGroup を保持し、グループ全体の回転を担う
    public sealed class Cube
    {
        private readonly BlockGroup _blockGroup;

        public Cube(BlockGroup blockGroup)
        {
            _blockGroup = blockGroup;
        }

        public IBlockGroup BlockGroup => _blockGroup;

        // 指定軸で90度回転した新しい Cube を返す（不変操作）。レイヤー内での位置入れ替えと Block.Rotate による自転を行う
        public Cube Rotate(RotateAxis axis)
        {
            var blocks = _blockGroup.Blocks;
            if (blocks.Count == 0)
            {
                return this;
            }

            // 現在のブロック群からバウンディングボックスを取得（任意サイズの N×N×N を許容）
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            int minZ = int.MaxValue, maxZ = int.MinValue;

            foreach (var kv in blocks)
            {
                var p = kv.Key;
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
                if (p.Z < minZ) minZ = p.Z;
                if (p.Z > maxZ) maxZ = p.Z;
            }

            var nextBlocks = new Dictionary<BlockPosition, Block>();
            foreach (var kv in blocks)
            {
                var newPos = RotatePosition(kv.Key, axis, minX, maxX, minY, maxY, minZ, maxZ);
                var block = (Block)kv.Value;
                nextBlocks[newPos] = block.Rotate(axis);
            }
            var newGroup = new BlockGroup(nextBlocks);
            return new Cube(newGroup);
        }

        // 軸に応じた座標変換。軸に直交する平面上で N×N グリッドを 90度時計回りに回転させる
        private static BlockPosition RotatePosition(
            BlockPosition pos,
            RotateAxis axis,
            int minX,
            int maxX,
            int minY,
            int maxY,
            int minZ,
            int maxZ)
        {
            switch (axis)
            {
                case RotateAxis.X:
                    // X 軸回転: X は不変。Y-Z 平面上で (y, z) を 90度時計回りに回転
                    {
                        int sizeY = maxY - minY + 1;
                        int localY = pos.Y - minY;
                        int localZ = pos.Z - minZ;

                        int rotatedLocalY = (sizeY - 1) - localZ;
                        int rotatedLocalZ = localY;

                        int newY = minY + rotatedLocalY;
                        int newZ = minZ + rotatedLocalZ;
                        return new BlockPosition(pos.X, newY, newZ);
                    }
                case RotateAxis.Y:
                    // Y 軸回転: Y は不変。X-Z 平面上で (x, z) を 90度時計回りに回転
                    {
                        int sizeX = maxX - minX + 1;
                        int localX = pos.X - minX;
                        int localZ = pos.Z - minZ;

                        int rotatedLocalX = (sizeX - 1) - localZ;
                        int rotatedLocalZ = localX;

                        int newX = minX + rotatedLocalX;
                        int newZ = minZ + rotatedLocalZ;
                        return new BlockPosition(newX, pos.Y, newZ);
                    }
                case RotateAxis.Z:
                    // Z 軸回転: Z は不変。X-Y 平面上で (x, y) を 90度時計回りに回転
                    {
                        int sizeX = maxX - minX + 1;
                        int localX = pos.X - minX;
                        int localY = pos.Y - minY;

                        int rotatedLocalX = (sizeX - 1) - localY;
                        int rotatedLocalY = localX;

                        int newX = minX + rotatedLocalX;
                        int newY = minY + rotatedLocalY;
                        return new BlockPosition(newX, newY, pos.Z);
                    }
                default:
                    return pos;
            }
        }
    }
}
