using Application;

namespace Application.PhaseStates
{
    // 終端フェーズ。以降の Execute もそのまま維持する想定
    public sealed class GameOverState : IGamePhaseState
    {
        public GamePhase Phase => GamePhase.GameOver;

        public (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState, float deltaTime)
        {
            return (this, gameState);
        }
    }
}
