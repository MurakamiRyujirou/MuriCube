using System;
using Application;
using R3;
using TMPro;
using UnityEngine;

namespace Presentation.Views.Gameplay
{
    // GameState のスコア・レベル・消去ライン数を表示する
    public sealed class ScoreUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _linesText;

        private IDisposable _subscription;

        private void Awake()
        {
            if (_scoreText == null)
                throw new MissingReferenceException($"{nameof(ScoreUIView)}: ScoreText is not assigned.");
            if (_levelText == null)
                throw new MissingReferenceException($"{nameof(ScoreUIView)}: LevelText is not assigned.");
            if (_linesText == null)
                throw new MissingReferenceException($"{nameof(ScoreUIView)}: LinesText is not assigned.");
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

            _scoreText.text = $"SCORE: {state.Score}";
            _levelText.text = $"LEVEL: {state.Level}";
            _linesText.text = $"LINES: {state.ClearedLineCount}";
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }
    }
}
