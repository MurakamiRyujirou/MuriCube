using Domain.Cube.Enums;

namespace Presentation.InputDetectors
{
    public class CubeRotation
    {
        public CubeOperation Operation { get; }

        public CubeRotation(CubeOperation operation)
        {
            Operation = operation;
        }
    }
}
