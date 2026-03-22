using UnityEngine;

namespace Presentation.Views.Gameplay
{
    // フィールド外周に 1×1×1 のブロックプレハブを並べて枠を表示する
    public sealed class FieldBorderView : MonoBehaviour
    {
        [SerializeField] private GameObject _borderBlockPrefab;
        [SerializeField] private float _cellSize = 1f;

        private void Awake()
        {
            if (_borderBlockPrefab == null)
                throw new MissingReferenceException($"{nameof(FieldBorderView)}: {nameof(_borderBlockPrefab)} is not assigned.");

            var s = _cellSize;

            for (var y = 0; y < 20; y++)
                PlaceBlock(-1, y, 0, s);

            for (var y = 0; y < 20; y++)
                PlaceBlock(10, y, 0, s);

            for (var x = 0; x < 10; x++)
                PlaceBlock(x, -1, 0, s);
        }

        private void PlaceBlock(int gridX, int gridY, int gridZ, float cellScale)
        {
            var go = Instantiate(_borderBlockPrefab, transform);
            go.transform.localPosition = new Vector3(gridX * cellScale, gridY * cellScale, gridZ * cellScale);
        }
    }
}
