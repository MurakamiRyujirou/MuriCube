using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Common;
using Domain.Cube.Enums;

namespace Domain.Cube
{
    // BlockGroup を保持し、グループ全体の回転を担う。View は IBlockGroup として受け取れる
    public sealed class Cube : IBlockGroup
    {
        private readonly BlockGroup _blockGroup;

        public Cube(BlockGroup blockGroup)
        {
            _blockGroup = blockGroup ?? throw new ArgumentNullException(nameof(blockGroup));
        }

        // 内包する BlockGroup（GetPositionMap 等は Cube 側の状態を前提にする）
        public IBlockGroup BlockGroup => _blockGroup;

        public IReadOnlyDictionary<BlockPosition, IBlock> Blocks => _blockGroup.Blocks;

        public IReadOnlyCollection<BlockPosition> GetAffectedBlocks(CubeOperation op, PivotPosition pivot)
        {
            return _blockGroup.Blocks.Keys
                .Where(pos => CubeOperationRotation.IsAffected(pos, op, pivot))
                .ToList();
        }

        public IReadOnlyDictionary<BlockPosition, BlockPosition> GetPositionMap(CubeOperation op, PivotPosition pivot)
        {
            var (axis, turn) = CubeOperationRotation.ToAxisAndTurn(op);
            var map = new Dictionary<BlockPosition, BlockPosition>();

            foreach (var pos in _blockGroup.Blocks.Keys)
            {
                if (!CubeOperationRotation.IsAffected(pos, op, pivot)) continue;
                var newPos = RotatePosition(pos, axis, turn, pivot);
                map[pos] = newPos;
            }

            return map;
        }

        public bool CanRotate(CubeOperation op, PivotPosition pivot)
        {
            var stationaryPositions = _blockGroup.Blocks.Keys
                .Where(pos => !CubeOperationRotation.IsAffected(pos, op, pivot))
                .ToList();

            var rotatedPositions = new List<BlockPosition>();
            var (axis, turn) = CubeOperationRotation.ToAxisAndTurn(op);

            foreach (var pos in _blockGroup.Blocks.Keys)
            {
                if (!CubeOperationRotation.IsAffected(pos, op, pivot)) continue;
                var newPos = RotatePosition(pos, axis, turn, pivot);

                foreach (var stationaryPos in stationaryPositions)
                {
                    if (Overlaps(newPos, stationaryPos))
                        return false;
                }

                foreach (var alreadyRotated in rotatedPositions)
                {
                    if (Overlaps(newPos, alreadyRotated))
                        return false;
                }

                rotatedPositions.Add(newPos);
            }

            return true;
        }

        private static bool Overlaps(BlockPosition a, BlockPosition b)
        {
            return Math.Abs(a.X - b.X) < 1.0f &&
                   Math.Abs(a.Y - b.Y) < 1.0f &&
                   Math.Abs(a.Z - b.Z) < 1.0f;
        }

        public Cube Rotate(CubeOperation op, PivotPosition pivot)
        {
            var blocks = _blockGroup.Blocks;
            if (blocks.Count == 0) return this;

            var (axis, turn) = CubeOperationRotation.ToAxisAndTurn(op);
            var nextBlocks = new Dictionary<BlockPosition, Block>();

            foreach (var kv in blocks)
            {
                var pos = kv.Key;
                if (kv.Value is not Block block)
                    throw new InvalidOperationException("Cube.Rotate には Block 具象が必要です。");

                if (CubeOperationRotation.IsAffected(pos, op, pivot))
                {
                    var newPos = RotatePosition(pos, axis, turn, pivot);
                    var blockTurn = axis == RotateAxis.Z ? InvertTurn(turn) : turn;
                    var rotatedBlock = RotateBlock(block, axis, blockTurn);
                    nextBlocks[newPos] = rotatedBlock;
                }
                else
                {
                    nextBlocks[pos] = block;
                }
            }

            return new Cube(new BlockGroup(nextBlocks));
        }

        private static CubeTurn InvertTurn(CubeTurn turn)
        {
            return turn switch
            {
                CubeTurn.Clockwise        => CubeTurn.CounterClockwise,
                CubeTurn.CounterClockwise => CubeTurn.Clockwise,
                CubeTurn.HalfTurn         => CubeTurn.HalfTurn,
                _                         => turn,
            };
        }

        private static Block RotateBlock(Block block, RotateAxis axis, CubeTurn turn)
        {
            return turn switch
            {
                CubeTurn.Clockwise        => block.Rotate(axis),
                CubeTurn.CounterClockwise => block.Rotate(axis).Rotate(axis).Rotate(axis),
                CubeTurn.HalfTurn         => block.Rotate(axis).Rotate(axis),
                _                         => block,
            };
        }

        private static BlockPosition RotatePosition(
            BlockPosition pos,
            RotateAxis axis,
            CubeTurn turn,
            PivotPosition pivot)
        {
            float dx = pos.X - pivot.X;
            float dy = pos.Y - pivot.Y;
            float dz = pos.Z - pivot.Z;

            float ndx = dx, ndy = dy, ndz = dz;

            switch (axis)
            {
                case RotateAxis.X:
                    (ndz, ndy) = Rotate2D(dz, dy, turn); break;
                case RotateAxis.Y:
                    (ndx, ndz) = Rotate2D(dx, dz, turn); break;
                case RotateAxis.Z:
                    (ndx, ndy) = Rotate2D(dx, dy, turn); break;
            }

            return new BlockPosition(ndx + pivot.X, ndy + pivot.Y, ndz + pivot.Z);
        }

        private static (float na, float nb) Rotate2D(float a, float b, CubeTurn turn)
        {
            return turn switch
            {
                CubeTurn.Clockwise        => ( b, -a),
                CubeTurn.CounterClockwise => (-b,  a),
                CubeTurn.HalfTurn         => (-a, -b),
                _                         => ( a,  b),
            };
        }
    }
}
