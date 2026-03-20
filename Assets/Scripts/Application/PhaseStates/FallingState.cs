using Application;

namespace Application.PhaseStates
{
    // 後続タスクで落下・入力を実装。現状はスタブ
    public sealed class FallingState : IGamePhaseState
    {
        public GamePhase Phase => GamePhase.Falling;

        public (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState)
        {
            return (this, gameState);
        }
    }
}
