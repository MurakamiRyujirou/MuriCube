using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Domain.Common;
using Domain.Cube;
using Domain.Cube.Enums;
using UnityEngine;

namespace Presentation.Views.Gameplay
{
    // IBlockGroup を視覚化し、DOTween で回転アニメーションを行う
    public sealed class CubeUIView : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _blocksRoot;
        [SerializeField] private BlockUIView _blockPrefab;
        [SerializeField] private Material _pivotAxisLineMaterial;

        private readonly List<BlockUIView> _views = new List<BlockUIView>();
        private LineRenderer _pivotAxisLineX;
        private LineRenderer _pivotAxisLineY;
        private LineRenderer _pivotAxisLineZ;
        private const float PivotAxisLineHalfLength = 2.5f;
        private Dictionary<BlockPosition, BlockUIView> _positionToView = new Dictionary<BlockPosition, BlockUIView>();
        private bool _isRotating;

        // 回転中かどうか（多重入力を防ぐために外部から参照可能）
        public bool IsRotating => _isRotating;

        private void Awake()
        {
            if (_visualRoot == null) throw new MissingReferenceException($"{nameof(CubeUIView)}: VisualRoot is not assigned.");
            if (_pivot == null) throw new MissingReferenceException($"{nameof(CubeUIView)}: Pivot is not assigned.");
            if (_blocksRoot == null) throw new MissingReferenceException($"{nameof(CubeUIView)}: BlocksRoot is not assigned.");
            if (_blockPrefab == null) throw new MissingReferenceException($"{nameof(CubeUIView)}: BlockPrefab is not assigned.");
        }

        /// <summary>
        /// Pivot を通る赤い X/Y/Z 軸線を描画する（回転しない）。Build 後にコントローラから呼ぶ。
        /// </summary>
        public void SetPivotAxisLine(float pivotX, float pivotY, float pivotZ)
        {
            if (_pivotAxisLineY == null)
                CreatePivotAxisLines();
            if (_pivotAxisLineX == null || _pivotAxisLineY == null || _pivotAxisLineZ == null) return;

            var p = new Vector3(pivotX, pivotY, pivotZ);
            var h = PivotAxisLineHalfLength;

            _pivotAxisLineX.SetPosition(0, p + new Vector3(-h, 0f, 0f));
            _pivotAxisLineX.SetPosition(1, p + new Vector3(h, 0f, 0f));
            _pivotAxisLineX.enabled = true;

            _pivotAxisLineY.SetPosition(0, p + new Vector3(0f, -h, 0f));
            _pivotAxisLineY.SetPosition(1, p + new Vector3(0f, h, 0f));
            _pivotAxisLineY.enabled = true;

            _pivotAxisLineZ.SetPosition(0, p + new Vector3(0f, 0f, -h));
            _pivotAxisLineZ.SetPosition(1, p + new Vector3(0f, 0f, h));
            _pivotAxisLineZ.enabled = true;
        }

        private void CreatePivotAxisLines()
        {
            var material = _pivotAxisLineMaterial;
            if (material == null)
            {
                var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
                if (shader != null)
                    material = new Material(shader) { color = Color.red };
            }

            _pivotAxisLineX = CreateAxisLineRenderer("PivotAxisLineX", material);
            _pivotAxisLineY = CreateAxisLineRenderer("PivotAxisLineY", material);
            _pivotAxisLineZ = CreateAxisLineRenderer("PivotAxisLineZ", material);
        }

        private LineRenderer CreateAxisLineRenderer(string goName, Material material)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(_visualRoot, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            lr.startWidth = 0.03f;
            lr.endWidth = 0.03f;
            lr.startColor = Color.red;
            lr.endColor = Color.red;
            if (material != null)
                lr.material = material;
            lr.sortingOrder = 0;
            return lr;
        }

        // 最初のブロック生成。既存の子はクリアしてから生成する
        public void Build(IBlockGroup group)
        {
            if (_isRotating) throw new InvalidOperationException($"{nameof(CubeUIView)}: Cannot Build while rotating.");

            ClearBlocks();

            var sorted = group.Blocks
                .OrderBy(kv => kv.Key.X)
                .ThenBy(kv => kv.Key.Y)
                .ThenBy(kv => kv.Key.Z)
                .ToList();

            foreach (var kv in sorted)
            {
                var pos = kv.Key;
                var block = kv.Value;
                var view = Instantiate(_blockPrefab, _blocksRoot);
                view.transform.position = DomainToWorld(pos);
                view.transform.rotation = Quaternion.identity;
                view.UpdateView(block);

                _views.Add(view);
                _positionToView[pos] = view;
            }
        }

        // 指定軸・方向・Pivot で回転アニメーション。affectedPositions はドメインの GetAffectedBlocks で取得すること。完了後は呼び出し側で Refresh を呼ぶこと
        public async UniTask RotateAsync(RotateAxis axis, CubeTurn turn, PivotPosition pivot, float duration,
            IReadOnlyCollection<BlockPosition> affectedPositions)
        {
            if (_isRotating) throw new InvalidOperationException($"{nameof(CubeUIView)}: Rotation already in progress.");
            if (_positionToView.Count == 0) return;
            if (affectedPositions == null || affectedPositions.Count == 0) return;

            _isRotating = true;

            try
            {
                var targetViews = new List<BlockUIView>();
                foreach (var pos in affectedPositions)
                {
                    if (_positionToView.TryGetValue(pos, out var view))
                        targetViews.Add(view);
                }

                if (targetViews.Count == 0)
                {
                    _isRotating = false;
                    return;
                }

                _pivot.position = DomainToWorld(pivot.X, pivot.Y, pivot.Z);
                _pivot.rotation = Quaternion.identity;

                foreach (var view in targetViews)
                    view.transform.SetParent(_pivot, true);

                var angleDeg = GetRotationAngleDegrees(turn);
                // Z軸は Unity の forward が奥向きのため、正面から見て時計回りにするには角度を反転する
                if (axis == RotateAxis.Z) angleDeg = -angleDeg;
                var axisVector = GetAxisVector(axis);

                var tween = _pivot.DORotate(
                    axisVector * angleDeg,
                    duration,
                    RotateMode.WorldAxisAdd
                ).SetEase(Ease.OutQuad);
                await tween.AsyncWaitForCompletion();

                foreach (var view in targetViews)
                    view.transform.SetParent(_blocksRoot, true);
            }
            finally
            {
                _isRotating = false;
            }
        }

        // 全ブロックの位置・回転・色を group に合わせて同期する。
        // group.Blocks を正とし、positionMap で「旧座標→新座標」から View の割り当てを解決する。
        public void Refresh(IBlockGroup group, IReadOnlyDictionary<BlockPosition, BlockPosition> positionMap)
        {
            if (_isRotating) throw new InvalidOperationException($"{nameof(CubeUIView)}: Cannot Refresh while rotating.");
            if (group == null) throw new ArgumentNullException(nameof(group));
            if (positionMap == null) throw new ArgumentNullException(nameof(positionMap));

            var newPosToOldPos = new Dictionary<BlockPosition, BlockPosition>(positionMap.Count);
            foreach (var kv in positionMap)
                newPosToOldPos[kv.Value] = kv.Key;

            var newPositionToView = new Dictionary<BlockPosition, BlockUIView>(_positionToView.Count);

            foreach (var kv in group.Blocks)
            {
                var pos = kv.Key;
                var block = kv.Value;

                var view = newPosToOldPos.TryGetValue(pos, out var oldPos) && _positionToView.TryGetValue(oldPos, out var v)
                    ? v
                    : _positionToView.TryGetValue(pos, out var v2) ? v2 : null;

                if (view == null)
                    continue;

                view.transform.SetParent(_blocksRoot, true);
                view.transform.SetPositionAndRotation(DomainToWorld(pos), Quaternion.identity);
                view.UpdateView(block);
                newPositionToView[pos] = view;
            }

            _positionToView = newPositionToView;
        }

        public void LogBlockPositions(string label)
        {
            foreach (var kv in _positionToView)
            {
                var wp = kv.Value.transform.position;
                Debug.Log($"[{label}] domain({kv.Key.X},{kv.Key.Y},{kv.Key.Z}) " +
                        $"world({wp.x:F2},{wp.y:F2},{wp.z:F2})");
            }
        }
        
        private void ClearBlocks()
        {
            foreach (var view in _views)
            {
                if (view != null && view.gameObject != null)
                    Destroy(view.gameObject);
            }
            _views.Clear();
            _positionToView.Clear();
        }

        private static float GetRotationAngleDegrees(CubeTurn turn)
        {
            return turn switch
            {
                CubeTurn.Clockwise => 90f,
                CubeTurn.CounterClockwise => -90f,
                CubeTurn.HalfTurn => 180f,
                _ => 0f
            };
        }

        private static Vector3 GetAxisVector(RotateAxis axis)
        {
            return axis switch
            {
                RotateAxis.X => Vector3.right,
                RotateAxis.Y => Vector3.up,
                RotateAxis.Z => Vector3.forward,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
        }

        private Vector3 DomainToWorld(BlockPosition pos)
        {
            return DomainToWorld(pos.X, pos.Y, pos.Z);
        }

        private Vector3 DomainToWorld(float x, float y, float z)
        {
            return _visualRoot.TransformPoint(x, y, z);
        }
    }
}
