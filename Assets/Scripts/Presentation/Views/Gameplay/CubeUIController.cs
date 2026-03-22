using System;
using Application;
using Application.UseCases;
using Cysharp.Threading.Tasks;
using Domain.Cube;
using Domain.Cube.Enums;
using Domain.Tetris;
using R3;
using UnityEngine;

namespace Presentation.Views.Gameplay
{
    // GameStateMachine と CubeUIView を接続し、スポーン時に Build、回転時にアニメ→Refresh→ユースケース反映を行う
    public sealed class CubeUIController : MonoBehaviour
    {
        [SerializeField] private CubeUIView _cubeUIView;
        [SerializeField] private float _rotateDuration = 0.3f;
        [SerializeField] private float _scramblingRotateDuration = 0.15f;

        private GameStateMachine _stateMachine;
        private IDisposable _subscription;
        private ActiveMino _lastActiveMinoRef;
        private int _previousScramblingMovesCount;
        private bool _scramblePlaybackRunning;
        private bool _buildRetryScheduled;

        public void Initialize(GameStateMachine stateMachine)
        {
            if (stateMachine == null)
                throw new ArgumentNullException(nameof(stateMachine));
            if (_cubeUIView == null)
                throw new MissingReferenceException($"{nameof(CubeUIController)}: {nameof(_cubeUIView)} is not assigned.");

            _stateMachine = stateMachine;
            _subscription?.Dispose();
            _subscription = _stateMachine.GameStateObservable.Subscribe(OnGameStateChanged);

            var current = _stateMachine.GameStateObservable.CurrentValue;
            _previousScramblingMovesCount = current.ScramblingMoves.Count;
            TryBuildFromState(current, _previousScramblingMovesCount);
        }

        public async UniTask ExecuteRotateAsync(CubeOperation operation, float? durationOverride = null)
        {
            await ExecuteRotateCoreAsync(operation, durationOverride);
        }

        private async UniTask ExecuteRotateCoreAsync(CubeOperation operation, float? durationOverride = null)
        {
            if (_cubeUIView == null || _stateMachine == null)
                return;
            if (_cubeUIView.IsRotating)
                return;

            var activeMino = _stateMachine.GameStateObservable.CurrentValue.ActiveMino;
            if (activeMino == null)
                return;

            var cube = new Cube(new BlockGroup(activeMino.BlockGroup.Blocks));
            if (!cube.CanRotate(operation, activeMino.Pivot))
                return;

            var (axis, turn) = CubeOperationRotation.ToAxisAndTurn(operation);
            var duration = durationOverride ?? _rotateDuration;
            var affected = cube.GetAffectedBlocks(operation, activeMino.Pivot);
            await _cubeUIView.RotateAsync(axis, turn, activeMino.Pivot, duration, affected);

            var positionMap = cube.GetPositionMap(operation, activeMino.Pivot);
            var rotatedCube = cube.Rotate(operation, activeMino.Pivot);
            _cubeUIView.Refresh(rotatedCube, positionMap);

            var newGameState = RotateMinoUseCase.Execute(_stateMachine.GameStateObservable.CurrentValue, operation);
            _lastActiveMinoRef = newGameState.ActiveMino;
            _stateMachine.ApplyGameState(newGameState);
        }

        private void OnGameStateChanged(GameState state)
        {
            var prevScramble = _previousScramblingMovesCount;
            TryBuildFromState(state, prevScramble);

            if (state.ScramblingMoves.Count > 0 && prevScramble == 0 && !_scramblePlaybackRunning)
                PlayScramblingAsync(state).Forget();

            _previousScramblingMovesCount = state.ScramblingMoves.Count;
        }

        private async UniTaskVoid PlayScramblingAsync(GameState stateWithScramble)
        {
            if (_stateMachine == null || _cubeUIView == null)
                return;

            _scramblePlaybackRunning = true;
            try
            {
                var moves = stateWithScramble.ScramblingMoves;
                foreach (var move in moves)
                    await ExecuteRotateCoreAsync(move.Operation, _scramblingRotateDuration);

                var current = _stateMachine.GameStateObservable.CurrentValue;
                _stateMachine.ApplyGameState(current with { ScramblingMoves = Array.Empty<ScramblingMove>() });
            }
            finally
            {
                _scramblePlaybackRunning = false;
            }
        }

        private void TryBuildFromState(GameState state, int previousScramblingMovesCount)
        {
            if (state == null)
                return;

            var active = state.ActiveMino;
            if (active == null)
            {
                _lastActiveMinoRef = null;
                return;
            }

            if (ReferenceEquals(_lastActiveMinoRef, active))
                return;

            // 回転アニメ中は Build できない。落下などで ActiveMino が先に変わった場合は終了後に再試行する。
            if (_cubeUIView.IsRotating)
            {
                ScheduleTryBuildWhenRotationIdle();
                return;
            }

            if (state.ScramblingMoves.Count > 0 && previousScramblingMovesCount == 0)
            {
                _cubeUIView.Build(active.BlockGroup);
                _cubeUIView.SetPivotAxisLine(active.Pivot.X, active.Pivot.Y, active.Pivot.Z);
                _lastActiveMinoRef = active;
                return;
            }

            if (state.ScramblingMoves.Count == 0)
            {
                _cubeUIView.Build(active.BlockGroup);
                _cubeUIView.SetPivotAxisLine(active.Pivot.X, active.Pivot.Y, active.Pivot.Z);
                _lastActiveMinoRef = active;
            }
        }

        private void ScheduleTryBuildWhenRotationIdle()
        {
            if (_buildRetryScheduled)
                return;
            _buildRetryScheduled = true;
            WaitAndRetryTryBuildFromStateAsync().Forget();
        }

        private async UniTaskVoid WaitAndRetryTryBuildFromStateAsync()
        {
            try
            {
                await UniTask.WaitUntil(() => _cubeUIView == null || !_cubeUIView.IsRotating);
            }
            finally
            {
                _buildRetryScheduled = false;
            }

            if (_stateMachine == null || _cubeUIView == null)
                return;

            TryBuildFromState(_stateMachine.GameStateObservable.CurrentValue, _previousScramblingMovesCount);
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
