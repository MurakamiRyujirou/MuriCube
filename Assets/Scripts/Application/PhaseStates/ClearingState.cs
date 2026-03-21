using System;
using Application;

namespace Application.PhaseStates
{
    // 消去アニメ用の待機フェーズ。現状は即 Spawning へ（Application_GamePhaseState.md §3）
    public sealed class ClearingState : IGamePhaseState
    {
        private readonly Random _random;

        public ClearingState(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public GamePhase Phase => GamePhase.Clearing;

        public (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState, float deltaTime)
        {
            // deltaTime は将来、アニメーション待機用タイマーで使用する
            return (new SpawningState(_random), gameState);
        }
    }
}
