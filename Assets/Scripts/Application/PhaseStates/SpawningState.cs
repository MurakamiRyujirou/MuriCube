using Application;

namespace Application.PhaseStates
{
    // 後続タスクでスポーン・遷移を実装。現状はスタブ
    public sealed class SpawningState : IGamePhaseState
    {
        public GamePhase Phase => GamePhase.Spawning;

        public (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState)
        {
            return (this, gameState);
        }
    }
}
