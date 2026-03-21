namespace Application.UseCases
{
    // ミノの1セル移動方向（UseCase_MoveMino.md §3）
    public enum MoveDirection
    {
        Left,   // X - 1
        Right,  // X + 1
        Down,   // Y - 1（上がプラスのため下はマイナス）
    }
}
