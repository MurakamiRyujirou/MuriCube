using System;
using Application;
using Domain.Common;
using Domain.Tetris;
using R3;
using UnityEngine;

namespace Presentation.Views.Gameplay
{
    // GameState を購読し Field / ActiveMino の手前側（絶対 Z 最小）の層を平面表示する（Presentation_Views_FieldUIView.md）
    public sealed class FieldUIView : MonoBehaviour
    {
        private const int FieldPoolSize = 200;
        private const int ActiveMinoPoolSize = 4;

        [SerializeField] private BlockUIView _blockPrefab;
        [SerializeField] private Transform _fieldRoot;
        [SerializeField] private Transform _activeMinoRoot;
        [SerializeField] private float _cellSize = 1f;

        private BlockUIView[] _fieldPool;
        private BlockUIView[] _activeMinoPool;
        private IDisposable _subscription;

        private void Awake()
        {
            if (_blockPrefab == null)
                throw new MissingReferenceException($"{nameof(FieldUIView)}: BlockPrefab is not assigned.");
            if (_fieldRoot == null)
                throw new MissingReferenceException($"{nameof(FieldUIView)}: FieldRoot is not assigned.");
            if (_activeMinoRoot == null)
                throw new MissingReferenceException($"{nameof(FieldUIView)}: ActiveMinoRoot is not assigned.");

            _fieldPool = new BlockUIView[FieldPoolSize];
            for (var i = 0; i < FieldPoolSize; i++)
            {
                var view = Instantiate(_blockPrefab, _fieldRoot);
                view.gameObject.SetActive(false);
                _fieldPool[i] = view;
            }

            _activeMinoPool = new BlockUIView[ActiveMinoPoolSize];
            for (var i = 0; i < ActiveMinoPoolSize; i++)
            {
                var view = Instantiate(_blockPrefab, _activeMinoRoot);
                view.gameObject.SetActive(false);
                _activeMinoPool[i] = view;
            }
        }

        public void Initialize(GameStateMachine stateMachine)
        {
            if (stateMachine == null)
                throw new ArgumentNullException(nameof(stateMachine));

            _subscription?.Dispose();
            _subscription = stateMachine.GameStateObservable.Subscribe(OnGameStateChanged);
            var current = stateMachine.GameStateObservable.CurrentValue;
            Refresh(current);
            if (current.IsGameOver)
            {
                _subscription?.Dispose();
                _subscription = null;
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            Refresh(state);
            if (state.IsGameOver)
            {
                _subscription?.Dispose();
                _subscription = null;
            }
        }

        public void Refresh(GameState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            foreach (var view in _fieldPool)
                view.gameObject.SetActive(false);

            var fieldIndex = 0;
            foreach (var kv in state.Field.Blocks)
            {
                if (kv.Key.Z != Field.MinZ)
                    continue;
                if (fieldIndex >= FieldPoolSize)
                    break;

                var cell = _fieldPool[fieldIndex++];
                cell.transform.position = DomainToWorld(kv.Key);
                cell.UpdateView(kv.Value);
                cell.gameObject.SetActive(true);
            }

            foreach (var view in _activeMinoPool)
                view.gameObject.SetActive(false);

            var mino = state.ActiveMino;
            if (mino == null)
                return;

            var offset = mino.Offset;

            // ミノ内で最も手前のZ絶対座標を求める
            var minAbsZ = float.MaxValue;
            foreach (var kv in mino.BlockGroup.Blocks)
            {
                var absZ = offset.Z + kv.Key.Z;
                if (absZ < minAbsZ)
                    minAbsZ = absZ;
            }

            // offset と全セルの absZ を出力
            UnityEngine.Debug.Log($"[FieldUIView] offset={mino.Offset} minAbsZ={minAbsZ}");
            foreach (var kv in mino.BlockGroup.Blocks)
            {
                var absZ = mino.Offset.Z + kv.Key.Z;
                UnityEngine.Debug.Log($"[FieldUIView] local={kv.Key} absZ={absZ} pass={Mathf.Abs(absZ - minAbsZ) <= 0.01f}");
            }

            // 最も手前のZ層のセルのみ表示する
            var activeIndex = 0;
            foreach (var kv in mino.BlockGroup.Blocks)
            {
                var local = kv.Key;
                var absX = offset.X + local.X;
                var absY = offset.Y + local.Y;
                var absZ = offset.Z + local.Z;
                if (Mathf.Abs(absZ - minAbsZ) > 0.01f)
                    continue;
                if (activeIndex >= ActiveMinoPoolSize)
                    break;

                var cell = _activeMinoPool[activeIndex++];
                cell.transform.position = new Vector3(absX * _cellSize, absY * _cellSize, 0f);
                cell.UpdateView(kv.Value);
                cell.gameObject.SetActive(true);
            }

            // 表示したセル数を出力
            UnityEngine.Debug.Log($"[FieldUIView] activeIndex={activeIndex} (表示セル数)");
        }

        private Vector3 DomainToWorld(CubePosition pos) =>
            new Vector3(pos.X * _cellSize, pos.Y * _cellSize, 0f);

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }
    }
}

/*
 * シーンセットアップ手順（Task 031）
 *
 * 1. 空の GameObject を作成し GameLoopDebugRunner をアタッチ
 * 2. 別の GameObject を作成し FieldUIView をアタッチ
 * 3. BlockUIView を持つ Cube プレハブを作成し _blockPrefab に設定
 * 4. _fieldRoot 用・_activeMinoRoot 用の空 GameObject を子として作成し設定
 * 5. GameLoopDebugRunner の _fieldUIView に FieldUIView をアサイン
 * 6. カメラを Orthographic に設定し、フィールド全体（10×20）が映るよう調整
 * 7. Play して ミノが落下・積み上がる様子を確認する
 */
