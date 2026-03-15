using System;
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

        // 指定軸・方向・Pivot で回転した新しい Cube を返す（不変操作）
        // 回転対象は「Pivot が含まれるレイヤー」のブロックのみ。対象外は位置・向きとも変更しない（Domain_Cube.md 4.6）
        public Cube Rotate(RotateAxis axis, CubeTurn turn, PivotPosition pivot)
        {
            var blocks = _blockGroup.Blocks;
            if (blocks.Count == 0) return this;

            var layerValue = GetRotationLayerIndex(axis, pivot);
            var nextBlocks = new Dictionary<BlockPosition, Block>();

            foreach (var kv in blocks)
            {
                var pos  = kv.Key;
                var block = (Block)kv.Value;

                if (IsOnRotationLayer(pos, axis, layerValue))
                {
                    var newPos = RotatePosition(pos, axis, turn, pivot);
                    // Z軸は公転と自転の「時計回り」の向きが逆になるため、自転のみ turn を反転する（Domain_Cube 4.1.1 公式準拠）
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

        // 回転軸と Pivot から、回転対象レイヤーの座標値（整数）を返す
        private static int GetRotationLayerIndex(RotateAxis axis, PivotPosition pivot)
        {
            var coord = axis switch
            {
                RotateAxis.X => pivot.X,
                RotateAxis.Y => pivot.Y,
                RotateAxis.Z => pivot.Z,
                _ => 0f
            };
            return (int)Math.Round(coord);
        }

        private static bool IsOnRotationLayer(BlockPosition pos, RotateAxis axis, int layerValue)
        {
            var posCoord = axis switch
            {
                RotateAxis.X => pos.X,
                RotateAxis.Y => pos.Y,
                RotateAxis.Z => pos.Z,
                _ => -1
            };
            return posCoord == layerValue;
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
        //   1. ブロック座標から Pivot を引いて相対座標を得る（float で計算）
        //   2. 軸に直交する2軸を CubeTurn に応じて入れ替える
        //   3. Pivot を足し戻して絶対座標に変換し、四捨五入で BlockPosition に戻す
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
                    // X 軸 Clockwise (実測済み): 新dy = -dz, 新dz = dy
                    (ndy, ndz) = Rotate2D(dy, dz, turn);
                    break;

                case RotateAxis.Y:
                    // Y 軸 Clockwise (上から見て時計回り): 新dx = dz, 新dz = -dx
                    (ndx, ndz) = Rotate2D_Y(dx, dz, turn);
                    break;

                case RotateAxis.Z:
                    // Z 軸 Clockwise (手前から見て時計回り): 新dx = -dy, 新dy = dx
                    (ndx, ndy) = Rotate2D(dx, dy, turn);
                    break;
            }

            int newX = (int)Math.Round(ndx + pivot.X);
            int newY = (int)Math.Round(ndy + pivot.Y);
            int newZ = (int)Math.Round(ndz + pivot.Z);

            return new BlockPosition(newX, newY, newZ);
        }

        // X軸・Z軸共通の2D回転変換
        // Clockwise: 新a = -b, 新b = a
        private static (float na, float nb) Rotate2D(float a, float b, CubeTurn turn)
        {
            return turn switch
            {
                CubeTurn.Clockwise        => (-b,  a),
                CubeTurn.CounterClockwise => ( b, -a),
                CubeTurn.HalfTurn         => (-a, -b),
                _                         => ( a,  b),
            };
        }

        // Y軸専用の2D回転変換（X/Z 平面）
        // Y軸 Clockwise (上から見て時計回り): 新dx = dz, 新dz = -dx
        // ※ X軸・Z軸の Clockwise と回転方向が逆になるため専用メソッドで明示する
        private static (float ndx, float ndz) Rotate2D_Y(float dx, float dz, CubeTurn turn)
        {
            return turn switch
            {
                CubeTurn.Clockwise        => ( dz, -dx),
                CubeTurn.CounterClockwise => (-dz,  dx),
                CubeTurn.HalfTurn         => (-dx, -dz),
                _                         => ( dx,  dz),
            };
        }
    }
}