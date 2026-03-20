using System;
using System.Collections.Generic;
using Domain.Common;
using Domain.Common.Enums;

namespace Domain.Tetris
{
    // 接地確定ブロックを保持し、ライン消去と落下シフトを不変で提供する
    public sealed class Field
    {
        public const int MinX = 0;
        public const int MaxX = 9;
        public const int MinY = 0;
        public const int MaxY = 19;
        public const int MinZ = 0;
        public const int MaxZ = 1;

        private readonly Dictionary<CubePosition, IBlock> _blocks;

        public Field()
        {
            _blocks = new Dictionary<CubePosition, IBlock>();
        }

        private Field(Dictionary<CubePosition, IBlock> blocks)
        {
            _blocks = blocks;
        }

        public IReadOnlyDictionary<CubePosition, IBlock> Blocks => _blocks;

        // ウェルの形状（静的ルール）を判定する。占有状態はインスタンスメソッドで問い合わせること。
        public static bool Contains(CubePosition position)
        {
            return position.X >= MinX && position.X <= MaxX &&
                   position.Y >= MinY && position.Y <= MaxY &&
                   position.Z >= MinZ && position.Z <= MaxZ;
        }

        public bool TryGetBlock(CubePosition position, out IBlock block)
        {
            return _blocks.TryGetValue(position, out block);
        }

        // 指定座標にブロックを配置した新しいフィールドを返す
        public Field WithCell(CubePosition position, IBlock block)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));
            if (!Contains(position))
                throw new ArgumentException("座標がフィールド範囲外です。", nameof(position));

            var next = CloneBlocks();
            next[position] = block;
            return new Field(next);
        }

        // 指定座標のブロックを除いた新しいフィールドを返す
        public Field WithoutCell(CubePosition position)
        {
            var next = CloneBlocks();
            next.Remove(position);
            return new Field(next);
        }

        // Z=0 の全 X が埋まり、正面の色がすべて一致するときのみ true（Z=1 は判定に含めない）
        public bool IsLineClearable(int y)
        {
            if (y < MinY || y > MaxY)
                return false;

            if (!TryGetBlock(new CubePosition(MinX, y, MinZ), out var anchor))
                return false;

            var requiredColor = anchor.GetColor(BlockFace.Front);

            for (int x = MinX + 1; x <= MaxX; x++)
            {
                if (!TryGetBlock(new CubePosition(x, y, MinZ), out var cell))
                    return false;
                if (cell.GetColor(BlockFace.Front) != requiredColor)
                    return false;
            }

            return true;
        }

        // 現在状態で消去可能な全 Y を一度に消し、消えた行数だけ上段を Y 減方向へシフトする
        public Field ClearCompletedLines()
        {
            var clearedYs = new List<int>();
            for (int y = MinY; y <= MaxY; y++)
            {
                if (IsLineClearable(y))
                    clearedYs.Add(y);
            }

            // clearedYs は MinY→MaxY の順で Add しているため昇順が保証されている

            if (clearedYs.Count == 0)
                return this;

            var clearedSet = new HashSet<int>(clearedYs);
            var next = new Dictionary<CubePosition, IBlock>();

            foreach (var kv in _blocks)
            {
                var pos = kv.Key;
                if (clearedSet.Contains(pos.Y))
                    continue;

                int dropBy = 0;
                foreach (var cy in clearedYs)
                {
                    if (cy < pos.Y)
                        dropBy++;
                }

                if (dropBy == 0)
                    next[pos] = kv.Value;
                else
                    next[new CubePosition(pos.X, pos.Y - dropBy, pos.Z)] = kv.Value;
            }

            return new Field(next);
        }

        private Dictionary<CubePosition, IBlock> CloneBlocks()
        {
            return new Dictionary<CubePosition, IBlock>(_blocks);
        }
    }
}
