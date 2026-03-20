using System.Collections.Generic;
using Domain.Common;

namespace Domain.Cube
{
    // Block の集合の管理に徹する。座標とブロックの対応を保持する（回転は持たない）
    public sealed class BlockGroup : IBlockGroup
    {
        private readonly Dictionary<BlockPosition, IBlock> _blocks;

        public BlockGroup(IReadOnlyDictionary<BlockPosition, IBlock> blocks)
        {
            _blocks = new Dictionary<BlockPosition, IBlock>();
            if (blocks != null)
            {
                foreach (var kv in blocks)
                    _blocks[kv.Key] = kv.Value;
            }
        }

        // Block 辞書からの構築用（Domain_Cube 内の具象ブロックを直接渡す際の便宜）
        public BlockGroup(IReadOnlyDictionary<BlockPosition, Block> blocks)
        {
            _blocks = new Dictionary<BlockPosition, IBlock>();
            if (blocks != null)
            {
                foreach (var kv in blocks)
                    _blocks[kv.Key] = kv.Value;
            }
        }

        public IReadOnlyDictionary<BlockPosition, IBlock> Blocks => _blocks;
    }
}
