using System;

namespace Domain.Common
{
    // ブロック中心の座標 (x, y, z)。回転軸が 0.5 刻みの場合は非整数を許容する
    public readonly struct BlockPosition : IEquatable<BlockPosition>
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public BlockPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(BlockPosition other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj) => obj is BlockPosition other && Equals(other);

        public override int GetHashCode() => (X, Y, Z).GetHashCode();

        public override string ToString() => $"({X}, {Y}, {Z})";

        public static bool operator ==(BlockPosition left, BlockPosition right) => left.Equals(right);

        public static bool operator !=(BlockPosition left, BlockPosition right) => !left.Equals(right);
    }
}
