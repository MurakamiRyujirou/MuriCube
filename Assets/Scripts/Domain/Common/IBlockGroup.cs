using System.Collections.Generic;

namespace Domain.Common
{
    // ブロックの集合体。位置とブロックの対応を保持する
    public interface IBlockGroup
    {
        // ブロックの配置集合（位置 → ブロック）
        IReadOnlyDictionary<BlockPosition, IBlock> Blocks { get; }
    }
}
