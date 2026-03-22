using System;
using Application;
using Application.PhaseStates;
using Presentation.InputDetectors;
using Presentation.Views.Gameplay;
using R3;
using UnityEngine;

namespace Presentation.GameLoopDebug
{
    // ゲームループの動作確認用。製品コードには含めない想定。
    public sealed class GameLoopDebugRunner : MonoBehaviour
    {
        [SerializeField] private float _timeScale = 1f;
        [SerializeField] private FieldUIView _fieldUIView;
        [SerializeField] private ScoreUIView _scoreUIView;
        [SerializeField] private CubeUIController _cubeUIController;
        [SerializeField] private KeyboardInputDetector _keyboardInputDetector;
        [SerializeField] private GamepadInputDetector _gamepadInputDetector;

        private GameStateMachine _stateMachine;
        private IDisposable _subscription;
        private GamePhase _lastPhase;

        private void Start()
        {
            _stateMachine = new GameStateMachine();
            _lastPhase = _stateMachine.CurrentPhase;

            _subscription = _stateMachine.GameStateObservable.Subscribe(OnGameStateChanged);

            if (_fieldUIView != null)
                _fieldUIView.Initialize(_stateMachine);

            _scoreUIView?.Initialize(_stateMachine);

            _cubeUIController?.Initialize(_stateMachine);

            _keyboardInputDetector?.Initialize(_stateMachine);
            _gamepadInputDetector?.Initialize(_stateMachine);
        }

        private void Update()
        {
            _stateMachine.OnUpdate(Time.deltaTime * _timeScale);
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }

        private void OnGameStateChanged(GameState state)
        {
            var currentPhase = _stateMachine.CurrentPhase;
            if (currentPhase != _lastPhase)
            {
                // UnityEngine.Debug.Log($"[GameLoop] Phase: {_lastPhase} → {currentPhase}");
                _lastPhase = currentPhase;
            }

            // if (state.ActiveMino != null)
            //     UnityEngine.Debug.Log($"[GameLoop] ActiveMino: {state.ActiveMino.MinoType} offset={state.ActiveMino.Offset} fieldBlocks={state.Field.Blocks.Count}");

            // UnityEngine.Debug.Log($"[GameLoop] Score={state.Score} Level={state.Level} Lines={state.ClearedLineCount}");

            if (state.IsGameOver)
                UnityEngine.Debug.Log("[GameLoop] GameOver");
        }
    }
}

/*
 * 使い方
 *
 * - 空の GameObject に GameLoopDebugRunner をアタッチして Play する
 * - _timeScale を大きくすると高速動作確認できる（例: 10 にすると 10 倍速）
 * - コンソールに Phase: Spawning → Falling などのログが流れることを確認する
 * - Task 031: _fieldUIView に FieldUIView を割り当てるとフィールド表示を確認できる
 */
