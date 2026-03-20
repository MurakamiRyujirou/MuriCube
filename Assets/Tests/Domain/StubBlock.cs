using System;
using System.Collections.Generic;
using Domain.Common;
using Domain.Common.Enums;

namespace Domain.Tests
{
    // テスト専用のシンプルな IBlock 実装
    internal sealed class StubBlock : IBlock
    {
        private readonly Dictionary<BlockFace, BlockColor> _colors;

        public StubBlock(BlockColor allFaces)
        {
            _colors = new Dictionary<BlockFace, BlockColor>();
            foreach (BlockFace face in Enum.GetValues(typeof(BlockFace)))
                _colors[face] = allFaces;
        }

        public BlockColor GetColor(BlockFace face) => _colors[face];
    }
}
