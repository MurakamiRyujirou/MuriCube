using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Cube;
using Domain.Cube.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Presentation.Views.Gameplay
{
    // CubeUIView の動作確認用デバッグコンポーネント。3x3x3 キューブを表示し、R/U/F で回転する
    public sealed class CubeViewTestController : MonoBehaviour
    {
        [SerializeField] private CubeUIView _cubeUIView;
        [SerializeField] private float _rotateDuration = 0.3f;

        private Cube _cube;
        // 3x3x3 の面回転用 Pivot：R=x=2 の面中心, U=y=2 の面中心, F=z=0 の面中心（各 9 ブロックが回る）
        private static readonly (float X, float Y, float Z) PivotR = (2f, 1f, 1f); // 右面中心 → x=2 の層
        private static readonly (float X, float Y, float Z) PivotU = (1f, 2f, 1f); // 上面中心 → y=2 の層
        private static readonly (float X, float Y, float Z) PivotF = (1f, 1f, 0f); // 正面中心 → z=0 の層

        private void Start()
        {
            _cube = CreateStandard3x3x3Cube();
            _cubeUIView.Build(_cube.BlockGroup);
            LogDomainPositions("初期化後");
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (_cubeUIView.IsRotating) return;

            if (keyboard.rKey.wasPressedThisFrame)
                ExecuteRotateAsync(RotateAxis.X, CubeTurn.Clockwise).Forget();
            else if (keyboard.uKey.wasPressedThisFrame)
                ExecuteRotateAsync(RotateAxis.Y, CubeTurn.Clockwise).Forget();
            else if (keyboard.fKey.wasPressedThisFrame)
                ExecuteRotateAsync(RotateAxis.Z, CubeTurn.Clockwise).Forget();
        }

        private async UniTaskVoid ExecuteRotateAsync(RotateAxis axis, CubeTurn turn)
        {
            if (_cubeUIView.IsRotating) return;

            var (px, py, pz) = axis switch
            {
                RotateAxis.X => PivotR,
                RotateAxis.Y => PivotU,
                RotateAxis.Z => PivotF,
                _ => (1f, 1f, 1f)
            };
            var pivot = new PivotPosition(px, py, pz);
            await _cubeUIView.RotateAsync(axis, turn, pivot, _rotateDuration);

            _cube = _cube.Rotate(axis, turn, pivot);
            _cubeUIView.Refresh(_cube.BlockGroup);

            var axisLabel = axis switch { RotateAxis.X => "R", RotateAxis.Y => "U", RotateAxis.Z => "F", _ => "?" };
            LogDomainPositions($"{axisLabel}回転後");
        }

        private static Cube CreateStandard3x3x3Cube()
        {
            var faceColors = new Dictionary<BlockFace, BlockColor>
            {
                [BlockFace.Up] = BlockColor.White,
                [BlockFace.Down] = BlockColor.Yellow,
                [BlockFace.Front] = BlockColor.Green,
                [BlockFace.Back] = BlockColor.Blue,
                [BlockFace.Left] = BlockColor.Orange,
                [BlockFace.Right] = BlockColor.Red
            };

            var block = new Block(faceColors);
            var blocks = new Dictionary<BlockPosition, Block>();

            for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
            for (var z = 0; z < 3; z++)
                blocks[new BlockPosition(x, y, z)] = block;

            var group = new BlockGroup(blocks);
            return new Cube(group);
        }

        private void LogDomainPositions(string label)
        {
            var positions = new List<string>();
            foreach (var kv in _cube.BlockGroup.Blocks)
            {
                var p = kv.Key;
                positions.Add($"({p.X},{p.Y},{p.Z})");
            }
            positions.Sort();
            Debug.Log($"[CubeViewTest] {label} ドメイン座標 ({positions.Count} blocks): {string.Join(" ", positions)}");
        }
    }
}
