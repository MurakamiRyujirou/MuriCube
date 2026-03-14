using System;
using System.Collections.Generic;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Cube.Enums;

namespace Domain.Cube
{
    // 単一キューブ。6面の配色を保持し、軸指定で面のスワップを行う
    public sealed class Cube : IBlock
    {
        private readonly Dictionary<BlockFace, BlockColor> _faceColors;

        public Cube(IReadOnlyDictionary<BlockFace, BlockColor> faceColors)
        {
            foreach (BlockFace face in Enum.GetValues(typeof(BlockFace)))
            {
                if (faceColors == null || !faceColors.ContainsKey(face))
                    throw new ArgumentException("6面すべての色が定義されている必要があります。", nameof(faceColors));
            }
            _faceColors = new Dictionary<BlockFace, BlockColor>(faceColors);
        }

        public BlockColor GetColor(BlockFace face)
        {
            return _faceColors[face];
        }

        // 指定軸で90度回転した新しいキューブを返す（不変操作）。TechSpecs 3.2 のスワップロジックに準拠
        public Cube Rotate(RotateAxis axis)
        {
            var next = new Dictionary<BlockFace, BlockColor>(_faceColors);

            switch (axis)
            {
                case RotateAxis.X:
                    // Front -> Down, Down -> Back, Back -> Up, Up -> Front (L/R不変)
                    next[BlockFace.Down] = _faceColors[BlockFace.Front];
                    next[BlockFace.Back] = _faceColors[BlockFace.Down];
                    next[BlockFace.Up] = _faceColors[BlockFace.Back];
                    next[BlockFace.Front] = _faceColors[BlockFace.Up];
                    break;
                case RotateAxis.Y:
                    // Front -> Left, Left -> Back, Back -> Right, Right -> Front (U/D不変)
                    next[BlockFace.Left] = _faceColors[BlockFace.Front];
                    next[BlockFace.Back] = _faceColors[BlockFace.Left];
                    next[BlockFace.Right] = _faceColors[BlockFace.Back];
                    next[BlockFace.Front] = _faceColors[BlockFace.Right];
                    break;
                case RotateAxis.Z:
                    // Up -> Right, Right -> Down, Down -> Left, Left -> Up (F/B不変)
                    next[BlockFace.Right] = _faceColors[BlockFace.Up];
                    next[BlockFace.Down] = _faceColors[BlockFace.Right];
                    next[BlockFace.Left] = _faceColors[BlockFace.Down];
                    next[BlockFace.Up] = _faceColors[BlockFace.Left];
                    break;
            }

            return new Cube(next);
        }
    }
}
