using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// 指定軸・方向・Pivot に対して回転対象となるブロックの座標を返す。
        /// CW は正方向側、CCW は負方向側を対象（X: CW→pos.X&gt;pivot.X, CCW→pos.X&lt;pivot.X など）。
        /// </summary>
        public IReadOnlyCollection<BlockPosition> GetAffectedBlocks(RotateAxis axis, CubeTurn turn, PivotPosition pivot)
        {
            return _blockGroup.Blocks.Keys
                .Where(pos => IsAffected(pos, axis, turn, pivot))
                .ToList();
        }

        /// <summary>
        /// 指定軸・方向・Pivot に対して、回転対象ブロックの「旧座標 → 新座標」のマッピングを返す。
        /// Refresh 時に View を正しいブロックに対応付けるために使用する。
        /// </summary>
        public IReadOnlyDictionary<BlockPosition, BlockPosition> GetPositionMap(
            RotateAxis axis,
            CubeTurn turn,
            PivotPosition pivot)
        {
            var map = new Dictionary<BlockPosition, BlockPosition>();

            foreach (var pos in _blockGroup.Blocks.Keys)
            {
                if (!IsAffected(pos, axis, turn, pivot)) continue;
                var newPos = RotatePosition(pos, axis, turn, pivot);
                map[pos] = newPos;
            }

            return map;
        }

        /// <summary>
        /// 指定の回転を実行したとき、ブロック座標の衝突が起きないか検証する。
        /// 回転後の座標が回転対象外ブロック、または回転対象ブロック同士で物理的に重なる場合は false を返す。
        /// </summary>
        public bool CanRotate(RotateAxis axis, CubeTurn turn, PivotPosition pivot)
        {
            // 回転しないブロックの座標セット
            var stationaryPositions = _blockGroup.Blocks.Keys
                .Where(pos => !IsAffected(pos, axis, turn, pivot))
                .ToList();

            // 回転対象同士の衝突検出用に、既に計算した新座標を保持
            var rotatedPositions = new List<BlockPosition>();

            foreach (var pos in _blockGroup.Blocks.Keys)
            {
                if (!IsAffected(pos, axis, turn, pivot)) continue;
                var newPos = RotatePosition(pos, axis, turn, pivot);

                // 静止ブロックとの重なりチェック
                foreach (var stationaryPos in stationaryPositions)
                {
                    if (Overlaps(newPos, stationaryPos))
                        return false;
                }

                // 回転対象同士の重なりチェック
                foreach (var alreadyRotated in rotatedPositions)
                {
                    if (Overlaps(newPos, alreadyRotated))
                        return false;
                }

                rotatedPositions.Add(newPos);
            }

            return true;
        }

        // サイズ1の立方体同士が物理的に重なるかどうか（中心座標の差が全軸で1未満）
        private static bool Overlaps(BlockPosition a, BlockPosition b)
        {
            return Math.Abs(a.X - b.X) < 1.0f &&
                   Math.Abs(a.Y - b.Y) < 1.0f &&
                   Math.Abs(a.Z - b.Z) < 1.0f;
        }

        // 指定軸・方向・Pivot で回転した新しい Cube を返す（不変操作）
        // 回転対象は境界比較で決定（Domain_Cube.md 4.6）
        public Cube Rotate(RotateAxis axis, CubeTurn turn, PivotPosition pivot)
        {
            var blocks = _blockGroup.Blocks;
            if (blocks.Count == 0) return this;

            var nextBlocks = new Dictionary<BlockPosition, Block>();

            foreach (var kv in blocks)
            {
                var pos = kv.Key;
                var block = (Block)kv.Value;

                if (IsAffected(pos, axis, turn, pivot))
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

        // 境界比較: CW は正方向側、CCW は負方向側を対象。HalfTurn は CW と同じ側とする。
        private static bool IsAffected(BlockPosition pos, RotateAxis axis, CubeTurn turn, PivotPosition pivot)
        {
            var usePositiveSide = turn == CubeTurn.Clockwise || turn == CubeTurn.HalfTurn;
            return axis switch
            {
                RotateAxis.X => usePositiveSide ? pos.X > pivot.X : pos.X < pivot.X,
                RotateAxis.Y => usePositiveSide ? pos.Y > pivot.Y : pos.Y < pivot.Y,
                RotateAxis.Z => usePositiveSide ? pos.Z < pivot.Z : pos.Z > pivot.Z,
                _ => false
            };
        }

        // Z軸用：公転はそのままだが自転だけ公式の「手前から時計回り」に合わせるため turn を反転する
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

        // CubeTurn に応じて Block の自転を行う
        // CounterClockwise = Clockwise × 3、HalfTurn = Clockwise × 2
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

        // 軸・方向・Pivot に基づいて BlockPosition を変換する
        // 手順:
        //   1. ブロック座標から Pivot を引いて相対座標を得る
        //   2. 軸に直交する2軸を CubeTurn に応じて入れ替える
        //   3. Pivot を足し戻して絶対座標に変換し、そのまま BlockPosition で返す（丸めなし）
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

            float rx = ndx + pivot.X;
            float ry = ndy + pivot.Y;
            float rz = ndz + pivot.Z;

            return new BlockPosition(rx, ry, rz);
        }

        // 2D回転変換
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