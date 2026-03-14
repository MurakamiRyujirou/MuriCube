namespace Domain.Cube
{
    // 回転の中心となる空間座標（ブロック中央が整数座標のため、格子点は 0.5 刻みになる）
    // 例: 4x4x4 右端列の格子点軸 = (3.0f, 0.5f, 0.5f)
    public readonly struct PivotPosition
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public PivotPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}