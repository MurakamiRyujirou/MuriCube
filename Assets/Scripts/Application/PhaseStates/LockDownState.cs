using Application;

namespace Application.PhaseStates
{
    // 後続タスクでロックダウン猶予を実装。現状はスタブ
    public sealed class LockDownState : IGamePhaseState
    {
        public GamePhase Phase => GamePhase.LockDown;

        public (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState)
        {
            return (this, gameState);
        }
    }
}
