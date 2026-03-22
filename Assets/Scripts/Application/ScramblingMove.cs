using Domain.Cube.Enums;

namespace Application
{
    // スクランブル演出用の 1 回転分（SpawnMinoUseCase が記録）
    public readonly struct ScramblingMove
    {
        public CubeOperation Operation { get; }

        public ScramblingMove(CubeOperation operation)
        {
            Operation = operation;
        }
    }
}
