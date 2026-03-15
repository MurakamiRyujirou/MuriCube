using Domain.Common;
using Domain.Common.Enums;
using UnityEngine;

namespace Presentation.Views.Gameplay
{
    // ブロックの見た目を担当。IBlock の配色を 6 面の MeshRenderer に反映する
    public sealed class BlockUIView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _up;
        [SerializeField] private MeshRenderer _down;
        [SerializeField] private MeshRenderer _left;
        [SerializeField] private MeshRenderer _right;
        [SerializeField] private MeshRenderer _front;
        [SerializeField] private MeshRenderer _back;

        [SerializeField] private Material _materialRed;
        [SerializeField] private Material _materialOrange;
        [SerializeField] private Material _materialWhite;
        [SerializeField] private Material _materialYellow;
        [SerializeField] private Material _materialGreen;
        [SerializeField] private Material _materialBlue;

        private void Awake()
        {
            if (_up == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: Up is not assigned.");
            if (_down == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: Down is not assigned.");
            if (_left == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: Left is not assigned.");
            if (_right == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: Right is not assigned.");
            if (_front == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: Front is not assigned.");
            if (_back == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: Back is not assigned.");

            if (_materialRed == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: MaterialRed is not assigned.");
            if (_materialOrange == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: MaterialOrange is not assigned.");
            if (_materialWhite == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: MaterialWhite is not assigned.");
            if (_materialYellow == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: MaterialYellow is not assigned.");
            if (_materialGreen == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: MaterialGreen is not assigned.");
            if (_materialBlue == null) throw new MissingReferenceException($"{nameof(BlockUIView)}: MaterialBlue is not assigned.");
        }

        // 指定面の表示色を指定色のマテリアルに差し替える
        public void SetColor(BlockFace face, BlockColor color)
        {
            var renderer = GetRenderer(face);
            var material = GetMaterial(color);
            renderer.sharedMaterial = material;
        }

        // ドメインモデル IBlock を受け取り、全 6 面の色を一括更新する
        public void UpdateView(IBlock block)
        {
            SetColor(BlockFace.Up, block.GetColor(BlockFace.Up));
            SetColor(BlockFace.Down, block.GetColor(BlockFace.Down));
            SetColor(BlockFace.Left, block.GetColor(BlockFace.Left));
            SetColor(BlockFace.Right, block.GetColor(BlockFace.Right));
            SetColor(BlockFace.Front, block.GetColor(BlockFace.Front));
            SetColor(BlockFace.Back, block.GetColor(BlockFace.Back));
        }

        private MeshRenderer GetRenderer(BlockFace face)
        {
            return face switch
            {
                BlockFace.Up => _up,
                BlockFace.Down => _down,
                BlockFace.Left => _left,
                BlockFace.Right => _right,
                BlockFace.Front => _front,
                BlockFace.Back => _back,
                _ => throw new System.ArgumentOutOfRangeException(nameof(face), face, null)
            };
        }

        private Material GetMaterial(BlockColor color)
        {
            return color switch
            {
                BlockColor.Red => _materialRed,
                BlockColor.Orange => _materialOrange,
                BlockColor.White => _materialWhite,
                BlockColor.Yellow => _materialYellow,
                BlockColor.Green => _materialGreen,
                BlockColor.Blue => _materialBlue,
                _ => throw new System.ArgumentOutOfRangeException(nameof(color), color, null)
            };
        }
    }
}
