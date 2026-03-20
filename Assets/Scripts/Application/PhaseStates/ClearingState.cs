using Application;

namespace Application.PhaseStates
{
    // 後続タスクでライン消去・スコア更新を実装。現状はスタブ
    public sealed class ClearingState : IGamePhaseState
    {
        public GamePhase Phase => GamePhase.Clearing;

        public (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState)
        {
            return (this, gameState);
        }
    }
}
