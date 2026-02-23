using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FastActionsPositioner : MonoBehaviour
{
    [SerializeField] private RectTransform _canvasRect;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Space(15)]

    [SerializeField] private float _showTime = 0.1f;
    [SerializeField] private float _hideTime = 0.05f;
    [SerializeField] private float _whenShowingMovementTime = 0.05f;

    [Space(15)]

    [SerializeField] private float _distanceFromCenterWhenShown = 100f;
    [SerializeField] private float _distanceFromCenterWhenHidden = 45f;
    [SerializeField] private Vector2 _buttonsSize;
    [SerializeField] private float _distanceFromScreenBounds;
    [SerializeField] private Vector2 _margin;

    [Space(15)]

    [SerializeField] private Button _upButton;
    [SerializeField] private Button _downButton;
    [SerializeField] private Button _rightButton;
    [SerializeField] private Button _leftButton;

    private RectTransform _upButtonTransform;
    private RectTransform _downButtonTransform;
    private RectTransform _rightButtonTransform;
    private RectTransform _leftButtonTransform;

    public bool ButtonsAreShown { get; private set; }
    public Transform CurrentNodeTransform => _nodeTransform;

    private Camera _mainCamera;

    private Transform _nodeTransform;
    private Coroutine _showTransition;
    private Coroutine _hideTransition;

    private Coroutine _updateButtonsPosition;

    private void Awake()
    {
        _mainCamera = Camera.main;

        _upButtonTransform = _upButton.transform as RectTransform;
        _downButtonTransform = _downButton.transform as RectTransform;
        _rightButtonTransform = _rightButton.transform as RectTransform;
        _leftButtonTransform = _leftButton.transform as RectTransform;

        ForceHide();
    }

    public void ShowButtons(Transform nodeTransform)
    {
        if (ButtonsAreShown && _nodeTransform == nodeTransform || IsObjectOutOfScreen(nodeTransform))
        {
            HideButtons();
            return;
        }

        if (ButtonsAreShown) ForceHide();

        _nodeTransform = nodeTransform;
        _showTransition ??= StartCoroutine(ShowTransition());

        _updateButtonsPosition ??= StartCoroutine(UpdateButtonsPosition());

        ButtonsAreShown = true;
    }

    public void HideButtons()
    {
        if (_showTransition is not null)
        {
            StopCoroutine(_showTransition);
            _showTransition = null;
        }

        SetButtonsState(false);

        _hideTransition ??= StartCoroutine(HideTransition());
    }

    public void ForceHide()
    {
        if (_showTransition is not null)
        {
            StopCoroutine(_showTransition);
            _showTransition = null;
        }

        if (_hideTransition is not null)
        {
            StopCoroutine(_hideTransition);
            _hideTransition = null;
        }

        if (_updateButtonsPosition is not null)
        {
            StopCoroutine(_updateButtonsPosition);
            _updateButtonsPosition = null;
        }

        ButtonsAreShown = false;
        _canvasGroup.alpha = 0f;
        _nodeTransform = null;

        SetButtonsState(false);
    }

    private IEnumerator UpdateButtonsPosition()
    {
        Vector2 startPosition = GetCanvasPositionForObject(_nodeTransform);

        _upButtonTransform.anchoredPosition = new Vector2(startPosition.x, startPosition.y + _distanceFromCenterWhenHidden);
        _downButtonTransform.anchoredPosition = new Vector2(startPosition.x, startPosition.y - _distanceFromCenterWhenHidden);
        _rightButtonTransform.anchoredPosition = new Vector2(startPosition.x + _distanceFromCenterWhenHidden, startPosition.y);
        _leftButtonTransform.anchoredPosition = new Vector2(startPosition.x - _distanceFromCenterWhenHidden, startPosition.y);

        yield return null;

        bool needToMove = true;
        float elapsed = 0f;

        while (_canvasGroup.alpha > 0f && _nodeTransform is not null)
        {
            Vector2 center = GetCanvasPositionForObject(_nodeTransform);

            if (IsObjectOutOfScreen(_nodeTransform))
            {
                _updateButtonsPosition = null;
                _nodeTransform = null;

                HideButtons();

                yield break;
            }

            Vector2 correctedCenter = CorrectCenterPosition(center);

            Vector2 targetUpPosition = correctedCenter + Vector2.up * _distanceFromCenterWhenShown;
            Vector2 targetDownPosition = correctedCenter + Vector2.down * _distanceFromCenterWhenShown;
            Vector2 targetRightPosition = correctedCenter + Vector2.right * _distanceFromCenterWhenShown;
            Vector2 targetLeftPosition = correctedCenter + Vector2.left * _distanceFromCenterWhenShown;
            
            float currentDistance = needToMove ? Mathf.Lerp(_distanceFromCenterWhenHidden, _distanceFromCenterWhenShown, Mathf.Clamp01(elapsed / _whenShowingMovementTime)) :
                _distanceFromCenterWhenShown;

            if (needToMove && elapsed > _whenShowingMovementTime)
            {
                needToMove = false;
                currentDistance = _distanceFromCenterWhenShown;
                SetButtonsState(true);
            }

            _upButtonTransform.anchoredPosition = correctedCenter + Vector2.up * currentDistance;
            _downButtonTransform.anchoredPosition = correctedCenter + Vector2.down * currentDistance;
            _rightButtonTransform.anchoredPosition = correctedCenter + Vector2.right * currentDistance;
            _leftButtonTransform.anchoredPosition = correctedCenter + Vector2.left * currentDistance;

            elapsed += Time.deltaTime;
            yield return null;
        }

        _updateButtonsPosition = null;
        _nodeTransform = null;
    }

    private bool IsObjectOutOfScreen(Transform transform)
    {
        Vector3 screenPoint = _mainCamera.WorldToScreenPoint(transform.position);
        
        if (screenPoint.z < 0) return true;
        
        Vector2 viewportPoint = new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
        Vector2 normalizedMargin = new Vector2(_margin.x / Screen.width, _margin.y / Screen.height);
        
        return viewportPoint.x < -normalizedMargin.x || 
            viewportPoint.x > 1 + normalizedMargin.x || 
            viewportPoint.y < -normalizedMargin.y || 
            viewportPoint.y > 1 + normalizedMargin.y;
    }

    private Vector2 CorrectCenterPosition(Vector2 correctedCenter)
    {
        Vector2 minPosition = _canvasRect.rect.min + _buttonsSize * 0.5f + Vector2.one * (_distanceFromCenterWhenShown + _distanceFromScreenBounds);
        Vector2 maxPosition = _canvasRect.rect.max - _buttonsSize * 0.5f - Vector2.one * (_distanceFromCenterWhenShown + _distanceFromScreenBounds);

        float clampedX = Mathf.Clamp(correctedCenter.x, minPosition.x, maxPosition.x);
        float clampedY = Mathf.Clamp(correctedCenter.y, minPosition.y, maxPosition.y);

        return new Vector2(clampedX, clampedY);
    }

    private Vector2 GetCanvasPositionForObject(Transform otherTransform)
    {
        Vector3 screenPoint = _mainCamera.WorldToScreenPoint(otherTransform.position);
        Vector2 canvasPosition;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPoint, null, out canvasPosition);
        
        return canvasPosition;
    }

    private IEnumerator ShowTransition()
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < _showTime)
        {
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, Mathf.Clamp01(elapsed / _showTime));
            elapsed += Time.deltaTime;
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _showTransition = null;
    }

    private IEnumerator HideTransition()
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < _hideTime)
        {
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / _hideTime));
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_updateButtonsPosition is not null)
        {
            StopCoroutine(_updateButtonsPosition);
            _updateButtonsPosition = null;
        }

        ButtonsAreShown = false;
        _canvasGroup.alpha = 0f;
        _hideTransition = null;

        _nodeTransform = null;
    }

    private void SetButtonsState(bool state)
    {
        _upButton.enabled = state;
        _downButton.enabled = state;
        _leftButton.enabled = state;
        _rightButton.enabled = state;
    }
}
