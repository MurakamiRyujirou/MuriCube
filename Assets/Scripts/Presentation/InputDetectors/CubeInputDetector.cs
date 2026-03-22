using System.Collections;
using Application;
using Cysharp.Threading.Tasks;
using Presentation.Views.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Presentation.InputDetectors
{
    // CubeGuide レイヤーへのレイキャストでエリア検出し、スワイプ・タップをキューブ回転に変換する（UI_Layout §6.1）
    public sealed class CubeInputDetector : MonoBehaviour
    {
        [SerializeField] private Camera _cubeCamera;
        [SerializeField] private CubeUIController _cubeUIController;
        [SerializeField] private GameStateMachine _stateMachine;

        [SerializeField] private GameObject _rightArea;
        [SerializeField] private GameObject _leftArea;
        [SerializeField] private GameObject _upArea;
        [SerializeField] private GameObject _downArea;
        [SerializeField] private GameObject _frontArea;
        [SerializeField] private GameObject _backArea;

        [SerializeField] private SwipeConfig _rightAreaConfig = new SwipeConfig();
        [SerializeField] private SwipeConfig _leftAreaConfig = new SwipeConfig();
        [SerializeField] private SwipeConfig _upAreaConfig = new SwipeConfig();
        [SerializeField] private SwipeConfig _downAreaConfig = new SwipeConfig();
        [SerializeField] private SwipeConfig _frontAreaConfig = new SwipeConfig();
        [SerializeField] private SwipeConfig _backAreaConfig = new SwipeConfig();

        [SerializeField] private TapConfig _frontAreaTapConfig = new TapConfig();
        [SerializeField] private TapConfig _backAreaTapConfig = new TapConfig();

        [SerializeField] private float _swipeThresholdPixels = 30f;
        [SerializeField] private float _tapMaxMovePixels = 20f;
        [SerializeField] private float _tapMaxDurationSeconds = 0.25f;
        [SerializeField] private float _doubleTapMaxInterval = 0.3f;

        private int _cubeGuideLayerMask;

        private bool _tracking;
        private Vector2 _pointerStartScreen;
        private float _pointerDownUnscaledTime;
        private GameObject _pointerDownArea;

        private Coroutine _frontPendingSingleTapCoroutine;
        private Coroutine _backPendingSingleTapCoroutine;

        private void Awake()
        {
            if (_cubeCamera == null)
                throw new MissingReferenceException($"{nameof(CubeInputDetector)}: {nameof(_cubeCamera)} is not assigned.");
            if (_cubeUIController == null)
                throw new MissingReferenceException($"{nameof(CubeInputDetector)}: {nameof(_cubeUIController)} is not assigned.");
            if (_rightArea == null || _leftArea == null || _upArea == null || _downArea == null || _frontArea == null || _backArea == null)
                throw new MissingReferenceException($"{nameof(CubeInputDetector)}: one or more area GameObjects are not assigned.");

            var layer = LayerMask.NameToLayer("CubeGuide");
            if (layer < 0)
                Debug.LogWarning($"{nameof(CubeInputDetector)}: Layer \"CubeGuide\" is not defined. Raycasts will miss.");
            _cubeGuideLayerMask = layer >= 0 ? 1 << layer : 0;
        }

        private void Update()
        {
            if (!TryReadUnifiedPointer(out var screenPos, out var pressedThisFrame, out var releasedThisFrame))
                return;

            if (pressedThisFrame && !_tracking)
            {
                if (!CanProcessInput())
                    return;

                var area = GetTouchedArea(screenPos);
                if (area == null)
                    return;

                _tracking = true;
                _pointerStartScreen = screenPos;
                _pointerDownUnscaledTime = Time.unscaledTime;
                _pointerDownArea = area;
            }

            if (releasedThisFrame && _tracking)
            {
                OnPointerUp(screenPos);
                _tracking = false;
                _pointerDownArea = null;
            }
        }

        private bool CanProcessInput()
        {
            if (_stateMachine == null)
                return true;
            return _stateMachine.GameStateObservable.CurrentValue.ScramblingMoves.Count == 0;
        }

        // タッチ中はタッチ優先。それ以外はマウス（Editor のみマウスがある想定）
        private static bool TryReadUnifiedPointer(out Vector2 screenPos, out bool pressedThisFrame, out bool releasedThisFrame)
        {
            screenPos = default;
            pressedThisFrame = false;
            releasedThisFrame = false;

            var ts = Touchscreen.current;
            var m = Mouse.current;
            var touchHeld = ts != null && ts.primaryTouch.press.isPressed;

            if (m != null && !touchHeld)
            {
                screenPos = m.position.ReadValue();
                pressedThisFrame = m.leftButton.wasPressedThisFrame;
                releasedThisFrame = m.leftButton.wasReleasedThisFrame;
                return true;
            }

            if (ts != null)
            {
                var p = ts.primaryTouch;
                screenPos = p.position.ReadValue();
                pressedThisFrame = p.press.wasPressedThisFrame;
                releasedThisFrame = p.press.wasReleasedThisFrame;
                return true;
            }

            return false;
        }

        private GameObject GetTouchedArea(Vector2 screenPosition)
        {
            var ray = _cubeCamera.ScreenPointToRay(screenPosition);
            if (_cubeGuideLayerMask == 0 || !Physics.Raycast(ray, out var hit, float.PositiveInfinity, _cubeGuideLayerMask))
                return null;

            return GetRegisteredArea(hit.collider.gameObject);
        }

        private GameObject GetRegisteredArea(GameObject hitObject)
        {
            if (hitObject == null)
                return null;
            if (IsUnderArea(hitObject, _rightArea)) return _rightArea;
            if (IsUnderArea(hitObject, _leftArea)) return _leftArea;
            if (IsUnderArea(hitObject, _upArea)) return _upArea;
            if (IsUnderArea(hitObject, _downArea)) return _downArea;
            if (IsUnderArea(hitObject, _frontArea)) return _frontArea;
            if (IsUnderArea(hitObject, _backArea)) return _backArea;
            return null;
        }

        private static bool IsUnderArea(GameObject hit, GameObject areaRoot)
        {
            if (areaRoot == null)
                return false;
            return hit == areaRoot || hit.transform.IsChildOf(areaRoot.transform);
        }

        private bool IsSwipeOnlyArea(GameObject area) =>
            area == _upArea || area == _downArea || area == _leftArea || area == _rightArea;

        private bool IsTapOnlyArea(GameObject area) =>
            area == _frontArea || area == _backArea;

        private void OnPointerUp(Vector2 releaseScreenPos)
        {
            if (!CanProcessInput())
                return;

            var delta = releaseScreenPos - _pointerStartScreen;
            var duration = Time.unscaledTime - _pointerDownUnscaledTime;
            var area = _pointerDownArea;
            if (area == null)
                return;

            if (IsSwipeOnlyArea(area))
            {
                if (delta.sqrMagnitude < _swipeThresholdPixels * _swipeThresholdPixels)
                    return;
                TryApplySwipe(area, delta);
                return;
            }

            if (IsTapOnlyArea(area))
            {
                if (delta.sqrMagnitude > _tapMaxMovePixels * _tapMaxMovePixels)
                    return;
                if (duration > _tapMaxDurationSeconds)
                    return;

                if (area == _frontArea)
                    RegisterFrontTap();
                else if (area == _backArea)
                    RegisterBackTap();
            }
        }

        private void RegisterFrontTap()
        {
            var config = _frontAreaTapConfig;
            if (_frontPendingSingleTapCoroutine != null)
            {
                StopCoroutine(_frontPendingSingleTapCoroutine);
                _frontPendingSingleTapCoroutine = null;
                if (config.enableDoubleTap)
                    OnRotationDetected(new CubeRotation(config.doubleTap));
                return;
            }

            if (config.enableSingleTap || config.enableDoubleTap)
                _frontPendingSingleTapCoroutine = StartCoroutine(FrontPendingSingleTapRoutine());
        }

        private void RegisterBackTap()
        {
            var config = _backAreaTapConfig;
            if (_backPendingSingleTapCoroutine != null)
            {
                StopCoroutine(_backPendingSingleTapCoroutine);
                _backPendingSingleTapCoroutine = null;
                if (config.enableDoubleTap)
                    OnRotationDetected(new CubeRotation(config.doubleTap));
                return;
            }

            if (config.enableSingleTap || config.enableDoubleTap)
                _backPendingSingleTapCoroutine = StartCoroutine(BackPendingSingleTapRoutine());
        }

        private IEnumerator FrontPendingSingleTapRoutine()
        {
            yield return new WaitForSecondsRealtime(_doubleTapMaxInterval);
            _frontPendingSingleTapCoroutine = null;
            if (_frontAreaTapConfig.enableSingleTap)
                OnRotationDetected(new CubeRotation(_frontAreaTapConfig.singleTap));
        }

        private IEnumerator BackPendingSingleTapRoutine()
        {
            yield return new WaitForSecondsRealtime(_doubleTapMaxInterval);
            _backPendingSingleTapCoroutine = null;
            if (_backAreaTapConfig.enableSingleTap)
                OnRotationDetected(new CubeRotation(_backAreaTapConfig.singleTap));
        }

        private void TryApplySwipe(GameObject area, Vector2 deltaPixels)
        {
            var config = GetSwipeConfigForArea(area);
            if (config == null)
                return;

            if (area == _upArea || area == _downArea)
            {
                if (Mathf.Abs(deltaPixels.x) < Mathf.Abs(deltaPixels.y))
                    return;
                if (deltaPixels.x < 0f && config.enableLeftSwipe)
                    OnRotationDetected(new CubeRotation(config.leftSwipe));
                else if (deltaPixels.x > 0f && config.enableRightSwipe)
                    OnRotationDetected(new CubeRotation(config.rightSwipe));
                return;
            }

            if (area == _leftArea || area == _rightArea)
            {
                if (Mathf.Abs(deltaPixels.y) < Mathf.Abs(deltaPixels.x))
                    return;
                if (deltaPixels.y > 0f && config.enableUpSwipe)
                    OnRotationDetected(new CubeRotation(config.upSwipe));
                else if (deltaPixels.y < 0f && config.enableDownSwipe)
                    OnRotationDetected(new CubeRotation(config.downSwipe));
            }
        }

        private SwipeConfig GetSwipeConfigForArea(GameObject area)
        {
            if (area == _rightArea) return _rightAreaConfig;
            if (area == _leftArea) return _leftAreaConfig;
            if (area == _upArea) return _upAreaConfig;
            if (area == _downArea) return _downAreaConfig;
            if (area == _frontArea) return _frontAreaConfig;
            if (area == _backArea) return _backAreaConfig;
            return null;
        }

        private void OnRotationDetected(CubeRotation rotation)
        {
            _cubeUIController.ExecuteRotateAsync(rotation.Operation).Forget();
        }
    }
}
