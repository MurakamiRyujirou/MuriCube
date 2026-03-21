using System;
using Application;
using Application.UseCases;

namespace Application.PhaseStates
{
    // 自然落下タイマーとソフトドロップ1段、接地で LockDown へ（Application_GamePhaseState.md §3）
    public sealed class FallingState : IGamePhaseState
    {
        private readonly Random _random;
        private float _timer;

        public FallingState(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public GamePhase Phase => GamePhase.Falling;

        public (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState, float deltaTime)
        {
            if (gameState == null)
                throw new ArgumentNullException(nameof(gameState));

            _timer += deltaTime;
            var interval = GetFallInterval(gameState.Level);
            if (_timer < interval)
                return (this, gameState);

            _timer -= interval;
            var newGameState = DropMinoUseCase.Execute(gameState, DropType.Soft);
            if (ReferenceEquals(newGameState, gameState))
                return (new LockDownState(_random), gameState);
            return (this, newGameState);
        }

        private static float GetFallInterval(int level) =>
            Math.Max(0.1f, 1.0f - level * 0.1f);
    }
}
