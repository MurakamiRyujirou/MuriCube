using System;
using Application;
using Application.UseCases;

namespace Application.PhaseStates
{
    // ロック猶予後に固定・ライン消去し Spawning へ（Application_GamePhaseState.md §3）
    public sealed class LockDownState : IGamePhaseState
    {
        private const float LockDelay = 0.5f;

        private readonly Random _random;
        private float _timer;

        public LockDownState(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public GamePhase Phase => GamePhase.LockDown;

        public (IGamePhaseState nextState, GameState nextGameState) Execute(GameState gameState, float deltaTime)
        {
            if (gameState == null)
                throw new ArgumentNullException(nameof(gameState));

            _timer += deltaTime;
            if (_timer < LockDelay)
                return (this, gameState);

            var lockedState = LockMinoUseCase.Execute(gameState);
            var clearedState = LineClearUseCase.Execute(lockedState);
            return (new SpawningState(_random), clearedState);
        }

        // 将来: 猶予時間内に MoveMinoUseCase / RotateMinoUseCase が成功したら _timer をリセットし
        // (new FallingState(_random), gameState) へ戻す拡張をここに組み込む。
    }
}
