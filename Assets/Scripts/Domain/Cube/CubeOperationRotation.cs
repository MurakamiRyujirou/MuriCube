using Domain.Common;
using Domain.Cube.Enums;

namespace Domain.Cube
{
    // ルービック記法の1手: レイヤーは操作記号で決まり、向きは CubeTurn で公転・自転に渡す
    public static class CubeOperationRotation
    {
        public static (RotateAxis axis, CubeTurn turn) ToAxisAndTurn(CubeOperation op)
        {
            return op switch
            {
                CubeOperation.R  => (RotateAxis.X, CubeTurn.Clockwise),
                CubeOperation.Ri => (RotateAxis.X, CubeTurn.CounterClockwise),
                CubeOperation.L  => (RotateAxis.X, CubeTurn.CounterClockwise),
                CubeOperation.Li => (RotateAxis.X, CubeTurn.Clockwise),
                CubeOperation.U  => (RotateAxis.Y, CubeTurn.Clockwise),
                CubeOperation.Ui => (RotateAxis.Y, CubeTurn.CounterClockwise),
                CubeOperation.D  => (RotateAxis.Y, CubeTurn.CounterClockwise),
                CubeOperation.Di => (RotateAxis.Y, CubeTurn.Clockwise),
                CubeOperation.F  => (RotateAxis.Z, CubeTurn.Clockwise),
                CubeOperation.Fi => (RotateAxis.Z, CubeTurn.CounterClockwise),
                CubeOperation.B  => (RotateAxis.Z, CubeTurn.CounterClockwise),
                CubeOperation.Bi => (RotateAxis.Z, CubeTurn.Clockwise),
                _ => throw new System.ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        /// <summary>
        /// 回転対象レイヤー（Pivot から見た正負側）。R/R' は同じ右レイヤー、L/L' は同じ左レイヤー。
        /// </summary>
        public static bool IsAffected(BlockPosition pos, CubeOperation op, PivotPosition pivot)
        {
            return op switch
            {
                CubeOperation.R or CubeOperation.Ri => pos.X > pivot.X,
                CubeOperation.L or CubeOperation.Li => pos.X < pivot.X,
                CubeOperation.U or CubeOperation.Ui => pos.Y > pivot.Y,
                CubeOperation.D or CubeOperation.Di => pos.Y < pivot.Y,
                CubeOperation.F or CubeOperation.Fi => pos.Z < pivot.Z,
                CubeOperation.B or CubeOperation.Bi => pos.Z > pivot.Z,
                _ => false
            };
        }
    }
}
