using System;
using System.Collections;
using Application;
using Application.UseCases;
using MoveDirection = Application.UseCases.MoveDirection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation.Views.Gameplay
{
    // テトリス操作ボタン（左右・ソフト・ハードドロップ）を処理する
    public sealed class TetrisInputView : MonoBehaviour
    {
        [SerializeField] private Button _moveLeftButton;
        [SerializeField] private Button _moveRightButton;
        [SerializeField] private Button _softDropButton;
        [SerializeField] private Button _hardDropButton;
        [SerializeField] private float _moveRepeatInterval = 0.1f;
        [SerializeField] private float _softDropInterval = 0.05f;

        private GameStateMachine _stateMachine;
        private Coroutine _moveLeftRepeat;
        private Coroutine _moveRightRepeat;
        private Coroutine _softDropRepeat;

        private void Awake()
        {
            if (_moveLeftButton == null)
                throw new MissingReferenceException($"{nameof(TetrisInputView)}: MoveLeftButton is not assigned.");
            if (_moveRightButton == null)
                throw new MissingReferenceException($"{nameof(TetrisInputView)}: MoveRightButton is not assigned.");
            if (_softDropButton == null)
                throw new MissingReferenceException($"{nameof(TetrisInputView)}: SoftDropButton is not assigned.");
            if (_hardDropButton == null)
                throw new MissingReferenceException($"{nameof(TetrisInputView)}: HardDropButton is not assigned.");

            SetupPointerBridge(_moveLeftButton, OnMoveLeftPointerDown, OnMoveLeftPointerUp);
            SetupPointerBridge(_moveRightButton, OnMoveRightPointerDown, OnMoveRightPointerUp);
            SetupPointerBridge(_softDropButton, OnSoftDropPointerDown, OnSoftDropPointerUp);
            SetupPointerBridge(_hardDropButton, OnHardDropPointerDown, null);
        }

        public void Initialize(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        }

        private void OnDisable()
        {
            StopMoveRepeats();
        }

        private void OnDestroy()
        {
            StopMoveRepeats();
        }

        private static void SetupPointerBridge(Button button, Action onDown, Action onUp)
        {
            var go = button.gameObject;
            var bridge = go.GetComponent<TetrisInputPointerBridge>();
            if (bridge == null)
                bridge = go.AddComponent<TetrisInputPointerBridge>();
            bridge.Setup(onDown, onUp);
        }

        private void OnMoveLeftPointerDown()
        {
            StopMoveLeftRepeat();
            _moveLeftRepeat = StartCoroutine(RepeatMoveLeft());
        }

        private void OnMoveLeftPointerUp()
        {
            StopMoveLeftRepeat();
        }

        private IEnumerator RepeatMoveLeft()
        {
            while (true)
            {
                ExecuteMove(MoveDirection.Left);
                yield return new WaitForSeconds(_moveRepeatInterval);
            }
        }

        private void OnMoveRightPointerDown()
        {
            StopMoveRightRepeat();
            _moveRightRepeat = StartCoroutine(RepeatMoveRight());
        }

        private void OnMoveRightPointerUp()
        {
            StopMoveRightRepeat();
        }

        private IEnumerator RepeatMoveRight()
        {
            while (true)
            {
                ExecuteMove(MoveDirection.Right);
                yield return new WaitForSeconds(_moveRepeatInterval);
            }
        }

        private void OnSoftDropPointerDown()
        {
            StopSoftDropRepeat();
            _softDropRepeat = StartCoroutine(RepeatSoftDrop());
        }

        private void OnSoftDropPointerUp()
        {
            StopSoftDropRepeat();
        }

        private IEnumerator RepeatSoftDrop()
        {
            while (true)
            {
                ExecuteSoftDrop();
                yield return new WaitForSeconds(_softDropInterval);
            }
        }

        private void OnHardDropPointerDown()
        {
            ExecuteHardDrop();
        }

        private void ExecuteMove(MoveDirection direction)
        {
            if (!CanInput())
                return;

            var next = MoveMinoUseCase.Execute(_stateMachine.GameStateObservable.CurrentValue, direction);
            _stateMachine.ApplyGameState(next);
        }

        private void ExecuteSoftDrop()
        {
            if (!CanInput())
                return;

            var next = DropMinoUseCase.Execute(_stateMachine.GameStateObservable.CurrentValue, DropType.Soft);
            _stateMachine.ApplyGameState(next);
        }

        private void ExecuteHardDrop()
        {
            if (!CanInput())
                return;

            var next = DropMinoUseCase.Execute(_stateMachine.GameStateObservable.CurrentValue, DropType.Hard);
            _stateMachine.ApplyGameState(next);
        }

        private bool CanInput()
        {
            if (_stateMachine == null)
                return false;

            var state = _stateMachine.GameStateObservable.CurrentValue;
            if (state.IsGameOver)
                return false;
            if (state.ScramblingMoves.Count > 0)
                return false;

            return true;
        }

        private void StopMoveLeftRepeat()
        {
            if (_moveLeftRepeat == null)
                return;
            StopCoroutine(_moveLeftRepeat);
            _moveLeftRepeat = null;
        }

        private void StopMoveRightRepeat()
        {
            if (_moveRightRepeat == null)
                return;
            StopCoroutine(_moveRightRepeat);
            _moveRightRepeat = null;
        }

        private void StopSoftDropRepeat()
        {
            if (_softDropRepeat == null)
                return;
            StopCoroutine(_softDropRepeat);
            _softDropRepeat = null;
        }

        private void StopMoveRepeats()
        {
            StopMoveLeftRepeat();
            StopMoveRightRepeat();
            StopSoftDropRepeat();
        }
    }

    internal sealed class TetrisInputPointerBridge : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Action _onDown;
        private Action _onUp;

        public void Setup(Action onDown, Action onUp)
        {
            _onDown = onDown;
            _onUp = onUp;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _onDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _onUp?.Invoke();
        }
    }
}
