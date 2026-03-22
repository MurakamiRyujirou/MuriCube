using System;
using Application;
using Application.UseCases;
using Cysharp.Threading.Tasks;
using Domain.Cube.Enums;
using Domain.Tetris;
using Presentation.Views.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Presentation.InputDetectors
{
    // Input Action Reference を購読し、回転・移動・落下を委譲する（ゲームパッド）
    public sealed class GamepadInputDetector : MonoBehaviour
    {
        [SerializeField] private CubeUIController _cubeUIController;
        [SerializeField] private InputActionReference _rotateUAction;
        [SerializeField] private InputActionReference _rotateRAction;
        [SerializeField] private InputActionReference _rotateFAction;
        [SerializeField] private InputActionReference _rotateLAction;
        [SerializeField] private InputActionReference _rotateDAction;
        [SerializeField] private InputActionReference _rotateBAction;
        [SerializeField] private InputActionReference _counterClockwiseModifierAction;
        [SerializeField] private InputActionReference _moveLeftAction;
        [SerializeField] private InputActionReference _moveRightAction;
        [SerializeField] private InputActionReference _softDropAction;
        [SerializeField] private InputActionReference _hardDropAction;

        private GameStateMachine _stateMachine;
        private bool _isCounterClockwiseModifierActive;

        public void Initialize(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        }

        private void OnEnable()
        {
            EnableAndSubscribe(_rotateUAction, OnRotateUPerformed);
            EnableAndSubscribe(_rotateRAction, OnRotateRPerformed);
            EnableAndSubscribe(_rotateFAction, OnRotateFPerformed);
            EnableAndSubscribe(_rotateLAction, OnRotateLPerformed);
            EnableAndSubscribe(_rotateDAction, OnRotateDPerformed);
            EnableAndSubscribe(_rotateBAction, OnRotateBPerformed);
            var mod = _counterClockwiseModifierAction.action;
            mod.performed += OnModifierPerformed;
            mod.canceled += OnModifierCanceled;
            if (!mod.enabled) mod.Enable();
            EnableAndSubscribe(_moveLeftAction, OnMoveLeftPerformed);
            EnableAndSubscribe(_moveRightAction, OnMoveRightPerformed);
            EnableAndSubscribe(_softDropAction, OnSoftDropPerformed);
            EnableAndSubscribe(_hardDropAction, OnHardDropPerformed);
        }

        private void OnDisable()
        {
            DisableAndUnsubscribe(_rotateUAction, OnRotateUPerformed);
            DisableAndUnsubscribe(_rotateRAction, OnRotateRPerformed);
            DisableAndUnsubscribe(_rotateFAction, OnRotateFPerformed);
            DisableAndUnsubscribe(_rotateLAction, OnRotateLPerformed);
            DisableAndUnsubscribe(_rotateDAction, OnRotateDPerformed);
            DisableAndUnsubscribe(_rotateBAction, OnRotateBPerformed);
            var mod = _counterClockwiseModifierAction.action;
            mod.performed -= OnModifierPerformed;
            mod.canceled -= OnModifierCanceled;
            if (mod.enabled) mod.Disable();
            DisableAndUnsubscribe(_moveLeftAction, OnMoveLeftPerformed);
            DisableAndUnsubscribe(_moveRightAction, OnMoveRightPerformed);
            DisableAndUnsubscribe(_softDropAction, OnSoftDropPerformed);
            DisableAndUnsubscribe(_hardDropAction, OnHardDropPerformed);
        }

        private void OnModifierPerformed(InputAction.CallbackContext _) =>
            _isCounterClockwiseModifierActive = true;

        private void OnModifierCanceled(InputAction.CallbackContext _) =>
            _isCounterClockwiseModifierActive = false;

        private static void EnableAndSubscribe(InputActionReference actionRef, Action<InputAction.CallbackContext> handler)
        {
            var a = actionRef.action;
            a.performed += handler;
            if (!a.enabled) a.Enable();
        }

        private static void DisableAndUnsubscribe(InputActionReference actionRef, Action<InputAction.CallbackContext> handler)
        {
            var a = actionRef.action;
            a.performed -= handler;
            if (a.enabled) a.Disable();
        }

        // RotateU → Button North（Y）
        private void OnRotateUPerformed(InputAction.CallbackContext _) =>
            HandleRotate(_isCounterClockwiseModifierActive ? CubeOperation.Ui : CubeOperation.U);

        // RotateR → Button East（B）
        private void OnRotateRPerformed(InputAction.CallbackContext _) =>
            HandleRotate(_isCounterClockwiseModifierActive ? CubeOperation.Ri : CubeOperation.R);

        // RotateF → Button South（A）
        private void OnRotateFPerformed(InputAction.CallbackContext _) =>
            HandleRotate(_isCounterClockwiseModifierActive ? CubeOperation.Fi : CubeOperation.F);

        // RotateD → DPad Down
        private void OnRotateDPerformed(InputAction.CallbackContext _) =>
            HandleRotate(_isCounterClockwiseModifierActive ? CubeOperation.Di : CubeOperation.D);

        // RotateL → DPad Left
        private void OnRotateLPerformed(InputAction.CallbackContext _) =>
            HandleRotate(_isCounterClockwiseModifierActive ? CubeOperation.Li : CubeOperation.L);

        // RotateB → DPad Up
        private void OnRotateBPerformed(InputAction.CallbackContext _) =>
            HandleRotate(_isCounterClockwiseModifierActive ? CubeOperation.Bi : CubeOperation.B);

        private void HandleRotate(CubeOperation operation)
        {
            if (!TryConsumeGameplayInput())
                return;

            if (_cubeUIController == null)
                throw new MissingReferenceException($"{nameof(GamepadInputDetector)}: {nameof(_cubeUIController)} is not assigned.");

            _cubeUIController.ExecuteRotateAsync(operation).Forget();
        }

        private void OnMoveLeftPerformed(InputAction.CallbackContext _)
        {
            if (!TryConsumeGameplayInput())
                return;

            var next = MoveMinoUseCase.Execute(_stateMachine.GameStateObservable.CurrentValue, MoveDirection.Left);
            _stateMachine.ApplyGameState(next);
        }

        private void OnMoveRightPerformed(InputAction.CallbackContext _)
        {
            if (!TryConsumeGameplayInput())
                return;

            var next = MoveMinoUseCase.Execute(_stateMachine.GameStateObservable.CurrentValue, MoveDirection.Right);
            _stateMachine.ApplyGameState(next);
        }

        private void OnSoftDropPerformed(InputAction.CallbackContext _)
        {
            if (!TryConsumeGameplayInput())
                return;

            var next = DropMinoUseCase.Execute(_stateMachine.GameStateObservable.CurrentValue, DropType.Soft);
            _stateMachine.ApplyGameState(next);
        }

        private void OnHardDropPerformed(InputAction.CallbackContext _)
        {
            if (!TryConsumeGameplayInput())
                return;

            var next = DropMinoUseCase.Execute(_stateMachine.GameStateObservable.CurrentValue, DropType.Hard);
            _stateMachine.ApplyGameState(next);
        }

        // IsGameOver 時は false。_stateMachine が未設定なら MissingReferenceException
        private bool TryConsumeGameplayInput()
        {
            if (_stateMachine == null)
                throw new MissingReferenceException($"{nameof(GamepadInputDetector)}: {nameof(_stateMachine)} is not assigned.");

            if (_stateMachine.GameStateObservable.CurrentValue.IsGameOver)
                return false;

            if (_stateMachine.GameStateObservable.CurrentValue.ScramblingMoves.Count > 0)
                return false;

            return true;
        }
    }
}
