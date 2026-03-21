using Application;

namespace Application.PhaseStates
{
    public interface IGamePhaseState
    {
        GamePhase Phase { get; }

        (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState, float deltaTime);
    }
}
