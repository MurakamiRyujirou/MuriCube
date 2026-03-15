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

        private readonly List<BlockUIView> _views = new List<BlockUIView>();
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

        // 指定軸・方向・Pivot で回転アニメーション。完了後は呼び出し側で Refresh(回転後の IBlockGroup) を呼ぶこと
        public async UniTask RotateAsync(RotateAxis axis, CubeTurn turn, PivotPosition pivot, float duration)
        {
            if (_isRotating) throw new InvalidOperationException($"{nameof(CubeUIView)}: Rotation already in progress.");
            if (_positionToView.Count == 0) return;

            _isRotating = true;

            try
            {
                var targetViews = GetViewsOnRotationLayer(axis, pivot);
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
                var startRot = _pivot.rotation;
                var endRot = startRot * Quaternion.AngleAxis(angleDeg, axisVector);

                var tween = _pivot.DORotateQuaternion(endRot, duration).SetEase(Ease.OutQuad);
                await tween.AsyncWaitForCompletion();

                foreach (var view in targetViews)
                    view.transform.SetParent(_blocksRoot, true);
            }
            finally
            {
                _isRotating = false;
            }
        }

        // 全ブロックの位置・回転・色を group に合わせて同期する
        public void Refresh(IBlockGroup group)
        {
            if (_isRotating) throw new InvalidOperationException($"{nameof(CubeUIView)}: Cannot Refresh while rotating.");
            if (_views.Count != group.Blocks.Count)
                throw new InvalidOperationException($"{nameof(CubeUIView)}: Block count mismatch. Call Build first.");

            var sorted = group.Blocks
                .OrderBy(kv => kv.Key.X)
                .ThenBy(kv => kv.Key.Y)
                .ThenBy(kv => kv.Key.Z)
                .ToList();

            _positionToView.Clear();

            for (var i = 0; i < sorted.Count; i++)
            {
                var pos = sorted[i].Key;
                var block = sorted[i].Value;
                var view = _views[i];

                view.transform.SetParent(_blocksRoot, true);
                view.transform.position = DomainToWorld(pos);
                view.transform.rotation = Quaternion.identity;
                view.UpdateView(block);

                _positionToView[pos] = view;
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

        // 回転軸と Pivot が含まれるレイヤーにいる BlockUIView を返す
        private List<BlockUIView> GetViewsOnRotationLayer(RotateAxis axis, PivotPosition pivot)
        {
            var layerValue = axis switch
            {
                RotateAxis.X => Mathf.RoundToInt(pivot.X),
                RotateAxis.Y => Mathf.RoundToInt(pivot.Y),
                RotateAxis.Z => Mathf.RoundToInt(pivot.Z),
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };

            var list = new List<BlockUIView>();
            foreach (var kv in _positionToView)
            {
                var onLayer = axis switch
                {
                    RotateAxis.X => kv.Key.X == layerValue,
                    RotateAxis.Y => kv.Key.Y == layerValue,
                    RotateAxis.Z => kv.Key.Z == layerValue,
                    _ => false
                };
                if (onLayer) list.Add(kv.Value);
            }
            return list;
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
