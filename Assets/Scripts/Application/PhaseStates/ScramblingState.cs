using System;
using Application;

namespace Application.PhaseStates
{
    // スクランブル演出完了（ScramblingMoves が空）まで待ち、Falling へ遷移する
    public sealed class ScramblingState : IGamePhaseState
    {
        private readonly Random _random;

        public ScramblingState(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public GamePhase Phase => GamePhase.Scrambling;

        public (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState, float deltaTime)
        {
            if (gameState.ScramblingMoves.Count == 0)
                return (new FallingState(_random), gameState);

            return (this, gameState);
        }
    }
}
