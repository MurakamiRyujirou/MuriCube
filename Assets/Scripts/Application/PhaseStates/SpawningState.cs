using System;
using Application;
using Application.UseCases;

namespace Application.PhaseStates
{
    // ミノ生成後に Falling または GameOver へ遷移する（Application_GamePhaseState.md §3）
    public sealed class SpawningState : IGamePhaseState
    {
        private readonly Random _random;

        public SpawningState(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public GamePhase Phase => GamePhase.Spawning;

        public (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState, float deltaTime)
        {
            var newGameState = SpawnMinoUseCase.Execute(gameState, _random);
            if (newGameState.IsGameOver)
                return (new GameOverState(), newGameState);
            return (new FallingState(_random), newGameState);
        }
    }
}
