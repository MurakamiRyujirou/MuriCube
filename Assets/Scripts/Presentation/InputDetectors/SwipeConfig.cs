using Domain.Cube.Enums;
using UnityEngine;

namespace Presentation.InputDetectors
{
    [System.Serializable]
    public class SwipeConfig
    {
        public bool enableUpSwipe = false;
        public CubeOperation upSwipe = CubeOperation.U;

        public bool enableDownSwipe = false;
        public CubeOperation downSwipe = CubeOperation.D;

        public bool enableLeftSwipe = false;
        public CubeOperation leftSwipe = CubeOperation.L;

        public bool enableRightSwipe = false;
        public CubeOperation rightSwipe = CubeOperation.R;
    }
}
