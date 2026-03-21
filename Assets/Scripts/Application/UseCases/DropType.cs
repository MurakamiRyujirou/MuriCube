namespace Application.UseCases
{
    // ドロップ種別（UseCase_DropMino.md §3）
    public enum DropType
    {
        Soft,   // Y - 1 を1回試みる
        Hard,   // 最下段まで即座に落下
    }
}
