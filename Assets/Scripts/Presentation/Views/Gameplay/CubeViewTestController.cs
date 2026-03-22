using System;
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
    // CubeUIView の動作確認用。1〜7でテトリミノ切替、R/L/U/D/F/B で各面回転
    public sealed class CubeViewTestController : MonoBehaviour
    {
        [SerializeField] private CubeUIView _cubeUIView;
        [SerializeField] private float _rotateDuration = 0.3f;

        private Cube _cube;
        private int _currentPieceIndex;
        private (float X, float Y, float Z) _currentPivot;

        // ピース定義: z=0 の (x,y) のみ。z=1 は同じ (x,y) で z=1 を追加して2層構成
        private readonly struct PieceDef
        {
            public readonly string Name;
            public readonly (float X, float Y)[] Z0Cells;
            public readonly (float X, float Y, float Z) Pivot;

            public PieceDef(string name, (float, float)[] z0Cells, (float, float, float) pivot)
            {
                Name = name;
                Z0Cells = z0Cells;
                Pivot = pivot;
            }
        }

        private static readonly PieceDef[] Pieces =
        {
            new PieceDef("Z型", new[] { (0f, 1f), (1f, 1f), (1f, 0f), (2f, 0f) }, (1.0f, 0.5f, 0.5f)),
            new PieceDef("S型", new[] { (0f, 0f), (1f, 0f), (1f, 1f), (2f, 1f) }, (1.0f, 0.5f, 0.5f)),
            new PieceDef("L型", new[] { (0f, 0f), (1f, 0f), (2f, 0f), (2f, 1f) }, (1.0f, 0.5f, 0.5f)),
            new PieceDef("J型", new[] { (0f, 1f), (0f, 0f), (1f, 0f), (2f, 0f) }, (1.0f, 0.5f, 0.5f)),
            new PieceDef("T型", new[] { (0f, 0f), (1f, 0f), (2f, 0f), (1f, 1f) }, (1.0f, 0.5f, 0.5f)),
            new PieceDef("I型", new[] { (0f, 0f), (1f, 0f), (2f, 0f), (3f, 0f) }, (1.5f, 0.5f, 0.5f)),
            new PieceDef("O型", new[] { (0f, 0f), (1f, 0f), (0f, 1f), (1f, 1f) }, (0.5f, 0.5f, 0.5f)),
        };

        private void Start()
        {
            SwitchToPiece(0);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (_cubeUIView.IsRotating) return;

            // 1〜7: テトリミノ切替
            if (keyboard.digit1Key.wasPressedThisFrame) SwitchToPiece(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) SwitchToPiece(1);
            else if (keyboard.digit3Key.wasPressedThisFrame) SwitchToPiece(2);
            else if (keyboard.digit4Key.wasPressedThisFrame) SwitchToPiece(3);
            else if (keyboard.digit5Key.wasPressedThisFrame) SwitchToPiece(4);
            else if (keyboard.digit6Key.wasPressedThisFrame) SwitchToPiece(5);
            else if (keyboard.digit7Key.wasPressedThisFrame) SwitchToPiece(6);
            // R/L/U/D/F/B: 回転
            else if (keyboard.rKey.wasPressedThisFrame)
                ExecuteRotateAsync(CubeOperation.R, _currentPivot, "R").Forget();
            else if (keyboard.lKey.wasPressedThisFrame)
                ExecuteRotateAsync(CubeOperation.L, _currentPivot, "L").Forget();
            else if (keyboard.uKey.wasPressedThisFrame)
                ExecuteRotateAsync(CubeOperation.U, _currentPivot, "U").Forget();
            else if (keyboard.dKey.wasPressedThisFrame)
                ExecuteRotateAsync(CubeOperation.D, _currentPivot, "D").Forget();
            else if (keyboard.fKey.wasPressedThisFrame)
                ExecuteRotateAsync(CubeOperation.F, _currentPivot, "F").Forget();
            else if (keyboard.bKey.wasPressedThisFrame)
                ExecuteRotateAsync(CubeOperation.B, _currentPivot, "B").Forget();
        }

        private void SwitchToPiece(int index)
        {
            _currentPieceIndex = Math.Clamp(index, 0, Pieces.Length - 1);
            var def = Pieces[_currentPieceIndex];
            _currentPivot = def.Pivot;
            _cube = CreateCubeFromPiece(def);
            _cubeUIView.Build(_cube);
            _cubeUIView.SetPivotAxisLine(_currentPivot.X, _currentPivot.Y, _currentPivot.Z);
            Debug.Log($"[CubeViewTest] ピース切替: {def.Name} (キー {_currentPieceIndex + 1})");
            LogDomainPositions("初期化後");
        }

        private async UniTaskVoid ExecuteRotateAsync(CubeOperation op, (float X, float Y, float Z) pivotTuple, string faceLabel)
        {
            if (_cubeUIView.IsRotating) return;

            var pivot = new PivotPosition(pivotTuple.X, pivotTuple.Y, pivotTuple.Z);
            if (!_cube.CanRotate(op, pivot))
            {
                Debug.Log($"[CubeViewTest] {faceLabel}: 回転後に座標衝突が発生するため操作を無視");
                return;
            }
            var (axis, turn) = CubeOperationRotation.ToAxisAndTurn(op);
            var affected = _cube.GetAffectedBlocks(op, pivot);
            await _cubeUIView.RotateAsync(axis, turn, pivot, _rotateDuration, affected);

            // Refresh 前のブロック位置をログ
            _cubeUIView.LogBlockPositions("Refresh前");

            var positionMap = _cube.GetPositionMap(op, pivot);
            foreach (var kv in positionMap)
                Debug.Log($"[CubeViewTest] positionMap: ({kv.Key.X},{kv.Key.Y},{kv.Key.Z}) -> ({kv.Value.X},{kv.Value.Y},{kv.Value.Z})");
            _cube = _cube.Rotate(op, pivot);
            _cubeUIView.Refresh(_cube, positionMap);

            // Refresh 後のブロック位置をログ
            _cubeUIView.LogBlockPositions("Refresh後");

            LogDomainPositions($"{faceLabel}回転後");
        }

        // ピース定義から z=0 / z=1 の2層で Cube を生成
        private static Cube CreateCubeFromPiece(PieceDef def)
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
            foreach (var (x, y) in def.Z0Cells)
            {
                blocks[new BlockPosition(x, y, 0f)] = block;
                blocks[new BlockPosition(x, y, 1f)] = block;
            }
            return new Cube(new BlockGroup(blocks));
        }

        private void LogDomainPositions(string label)
        {
            var positions = new List<string>();
            foreach (var kv in _cube.Blocks)
            {
                var p = kv.Key;
                positions.Add($"({p.X},{p.Y},{p.Z})");
            }
            positions.Sort();
            Debug.Log($"[CubeViewTest] {label} ドメイン座標 ({positions.Count} blocks): {string.Join(" ", positions)}");
        }
    }
}
