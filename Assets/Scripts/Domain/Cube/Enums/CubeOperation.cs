namespace Domain.Cube.Enums
{
    public enum CubeOperation
    {
        R,   // X軸 Clockwise
        Ri,  // X軸 CounterClockwise（R'）
        L,   // X軸 CounterClockwise
        Li,  // X軸 Clockwise（L'）
        U,   // Y軸 Clockwise
        Ui,  // Y軸 CounterClockwise（U'）
        D,   // Y軸 CounterClockwise
        Di,  // Y軸 Clockwise（D'）
        F,   // Z軸 Clockwise
        Fi,  // Z軸 CounterClockwise（F'）
        B,   // Z軸 CounterClockwise
        Bi,  // Z軸 Clockwise（B'）
    }
}
