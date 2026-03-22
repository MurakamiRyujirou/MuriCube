using Domain.Cube.Enums;
using UnityEngine;

namespace Presentation.InputDetectors
{
    [System.Serializable]
    public class TapConfig
    {
        public bool enableSingleTap = false;
        public CubeOperation singleTap = CubeOperation.F;

        public bool enableDoubleTap = false;
        public CubeOperation doubleTap = CubeOperation.Fi;
    }
}
