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

        private GameStateMachine _stateMachine;
        private IDisposable _subscription;
        private ActiveMino _lastActiveMinoRef;

        public void Initialize(GameStateMachine stateMachine)
        {
            if (stateMachine == null)
                throw new ArgumentNullException(nameof(stateMachine));
            if (_cubeUIView == null)
                throw new MissingReferenceException($"{nameof(CubeUIController)}: {nameof(_cubeUIView)} is not assigned.");

            _stateMachine = stateMachine;
            _subscription?.Dispose();
            _subscription = _stateMachine.GameStateObservable.Subscribe(OnGameStateChanged);

            TryBuildFromState(_stateMachine.GameStateObservable.CurrentValue);
        }

        public async UniTaskVoid ExecuteRotateAsync(RotateAxis axis, CubeTurn turn)
        {
            if (_cubeUIView == null || _stateMachine == null)
                return;
            if (_cubeUIView.IsRotating)
                return;

            var activeMino = _stateMachine.GameStateObservable.CurrentValue.ActiveMino;
            if (activeMino == null)
                return;

            var cube = new Cube(new BlockGroup(activeMino.BlockGroup.Blocks));
            if (!cube.CanRotate(axis, turn, activeMino.Pivot))
                return;

            var affected = cube.GetAffectedBlocks(axis, turn, activeMino.Pivot);
            await _cubeUIView.RotateAsync(axis, turn, activeMino.Pivot, _rotateDuration, affected);

            var positionMap = cube.GetPositionMap(axis, turn, activeMino.Pivot);
            var rotatedCube = cube.Rotate(axis, turn, activeMino.Pivot);
            _cubeUIView.Refresh(rotatedCube, positionMap);

            var newGameState = RotateMinoUseCase.Execute(_stateMachine.GameStateObservable.CurrentValue, axis, turn);
            _lastActiveMinoRef = newGameState.ActiveMino;
            _stateMachine.ApplyGameState(newGameState);
        }

        private void OnGameStateChanged(GameState state)
        {
            TryBuildFromState(state);
        }

        private void TryBuildFromState(GameState state)
        {
            if (state == null)
                return;

            var active = state.ActiveMino;
            if (active == null)
            {
                _lastActiveMinoRef = null;
                return;
            }

            if (!ReferenceEquals(_lastActiveMinoRef, active))
            {
                _cubeUIView.Build(active.BlockGroup);
                _lastActiveMinoRef = active;
            }
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
