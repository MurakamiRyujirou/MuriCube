using System;

namespace Domain.Tetris
{
    // フィールド上の絶対座標 (X, Y, Z)。X:0〜9, Y:0〜19, Z:0〜1
    public readonly struct CubePosition : IEquatable<CubePosition>
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public CubePosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(CubePosition other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj) => obj is CubePosition other && Equals(other);

        public override int GetHashCode() => (X, Y, Z).GetHashCode();

        public override string ToString() => $"({X}, {Y}, {Z})";

        public static bool operator ==(CubePosition left, CubePosition right) => left.Equals(right);

        public static bool operator !=(CubePosition left, CubePosition right) => !left.Equals(right);
    }
}
