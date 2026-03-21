using System;
using Application.PhaseStates;
using R3;

namespace Application
{
    // フェーズの保持・OnUpdate による遷移・GameState の通知を担う
    public sealed class GameStateMachine
    {
        private readonly ReactiveProperty<GameState> _gameState;
        private IGamePhaseState _currentState;

        public GameStateMachine()
            : this(new SpawningState(new Random()))
        {
        }

        public GameStateMachine(IGamePhaseState initialState)
        {
            _gameState = new ReactiveProperty<GameState>(GameState.Initial);
            GameStateObservable = _gameState;
            _currentState = initialState ?? throw new System.ArgumentNullException(nameof(initialState));
        }

        public ReadOnlyReactiveProperty<GameState> GameStateObservable { get; }

        public GamePhase CurrentPhase => _currentState.Phase;

        public void OnUpdate(float deltaTime)
        {
            var (nextState, nextGameState) = _currentState.Execute(_gameState.CurrentValue, deltaTime);
            _currentState = nextState;
            _gameState.Value = nextGameState;
        }

        public void Reset()
        {
            _currentState = new SpawningState(new Random());
            _gameState.Value = GameState.Initial;
        }
    }
}
