using System;

namespace Domain.Common
{
    // グリッド上の整数座標 (x, y, z) を表す値オブジェクト
    public readonly struct BlockPosition : IEquatable<BlockPosition>
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public BlockPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(BlockPosition other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj) => obj is BlockPosition other && Equals(other);

        public override int GetHashCode() => (X, Y, Z).GetHashCode();

        public static bool operator ==(BlockPosition left, BlockPosition right) => left.Equals(right);

        public static bool operator !=(BlockPosition left, BlockPosition right) => !left.Equals(right);
    }
}
