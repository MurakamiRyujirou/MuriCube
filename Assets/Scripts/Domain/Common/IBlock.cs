namespace Domain.Common
{
    // 単一ブロック。指定した面の色を取得できる
    public interface IBlock
    {
        // 指定した面の色を取得する
        BlockColor GetColor(BlockFace face);
    }
}
