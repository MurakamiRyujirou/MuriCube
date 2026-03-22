using Domain.Cube;
using Domain.Cube.Enums;

namespace Presentation
{
    public static class CubeOperationExtensions
    {
        public static (RotateAxis axis, CubeTurn turn) ToAxisAndTurn(this CubeOperation op) =>
            CubeOperationRotation.ToAxisAndTurn(op);
    }
}
