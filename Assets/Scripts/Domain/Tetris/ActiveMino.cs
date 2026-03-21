using System;
using System.Collections.Generic;
using Domain.Common;
using Domain.Cube;

namespace Domain.Tetris
{
    // 落下中のミノ。IBlockGroup はインターフェース経由のみ参照する（Domain_Tetris.md §3.2）
    public sealed class ActiveMino
    {
        private readonly MinoType _minoType;
        private readonly IBlockGroup _blockGroup;
        private readonly CubePosition _offset;
        private readonly PivotPosition _pivot;

        public ActiveMino(MinoType minoType, IBlockGroup blockGroup, CubePosition offset, PivotPosition pivot)
        {
            _minoType = minoType;
            _blockGroup = blockGroup ?? throw new ArgumentNullException(nameof(blockGroup));
            _offset = offset;
            _pivot = pivot;
        }

        public MinoType MinoType => _minoType;

        public IBlockGroup BlockGroup => _blockGroup;

        public CubePosition Offset => _offset;

        public PivotPosition Pivot => _pivot;

        // BlockPosition（相対）にオフセットを加えたフィールド絶対座標。非整数は最寄りの整数に丸める
        public IEnumerable<CubePosition> AbsolutePositions()
        {
            foreach (var kv in _blockGroup.Blocks)
                yield return Combine(_offset, kv.Key);
        }

        public ActiveMino WithOffset(CubePosition offset)
        {
            return new ActiveMino(_minoType, _blockGroup, offset, _pivot);
        }

        public ActiveMino WithBlockGroup(IBlockGroup blockGroup)
        {
            return new ActiveMino(_minoType, blockGroup ?? throw new ArgumentNullException(nameof(blockGroup)), _offset, _pivot);
        }

        public ActiveMino WithPivot(PivotPosition pivot)
        {
            return new ActiveMino(_minoType, _blockGroup, _offset, pivot);
        }

        // Z=Field.MinZ（プレイ平面）上の絶対セルのみ判定。それ以外の Z はテトリス衝突の対象外
        public bool IsColliding(Field field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            foreach (var p in AbsolutePositions())
            {
                if (p.Z != Field.MinZ)
                    continue;
                if (!Field.Contains(p))
                    return true;
                if (field.TryGetBlock(p, out _))
                    return true;
            }

            return false;
        }

        private static CubePosition Combine(CubePosition offset, BlockPosition local)
        {
            return new CubePosition(
                offset.X + ToGrid(local.X),
                offset.Y + ToGrid(local.Y),
                offset.Z + ToGrid(local.Z));
        }

        private static int ToGrid(float value)
        {
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}
