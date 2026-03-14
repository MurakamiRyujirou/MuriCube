using System.Collections.Generic;
using Domain.Common;

namespace Domain.Cube
{
    // Block の集合の管理に徹する。座標と Block の対応を保持し、回転は持たない
    public sealed class BlockGroup : IBlockGroup
    {
        private readonly Dictionary<BlockPosition, Block> _blocks;
        private readonly IReadOnlyDictionary<BlockPosition, IBlock> _blocksReadOnly;

        public BlockGroup(IReadOnlyDictionary<BlockPosition, Block> blocks)
        {
            _blocks = blocks != null ? new Dictionary<BlockPosition, Block>(blocks) : new Dictionary<BlockPosition, Block>();
            var asIBlock = new Dictionary<BlockPosition, IBlock>(_blocks.Count);
            foreach (var kv in _blocks)
                asIBlock[kv.Key] = kv.Value;
            _blocksReadOnly = asIBlock;
        }

        public IReadOnlyDictionary<BlockPosition, IBlock> Blocks => _blocksReadOnly;
    }
}
