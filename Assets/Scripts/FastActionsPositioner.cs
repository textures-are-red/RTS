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

    [Space(15)]

    [SerializeField] private RectTransform _upButtonTransform;
    [SerializeField] private RectTransform _downButtonTransform;
    [SerializeField] private RectTransform _rightButtonTransform;
    [SerializeField] private RectTransform _leftButtonTransform;

    public bool ButtonsAreShown { get; private set; }
    public Transform CurrentNodeTransform => _nodeTransform;

    private Camera _mainCamera;

    /*private Button _upButton;
    private Button _downButton;
    private Button _rightButton;
    private Button _leftButton;*/

    private Transform _nodeTransform;
    private Coroutine _showTransition;
    private Coroutine _hideTransition;

    private Coroutine _updateButtonsPosition;

    private void Awake()
    {
        _mainCamera = Camera.main;

        /*_upButton = _upButtonTransform.gameObject.GetComponent<Button>();
        _downButton = _downButtonTransform.gameObject.GetComponent<Button>();
        _rightButton = _rightButtonTransform.gameObject.GetComponent<Button>();
        _leftButton = _leftButtonTransform.gameObject.GetComponent<Button>();*/

        ForceHide();
    }

    public void ShowButtons(Transform nodeTransform)
    {
        if (ButtonsAreShown) ForceHide();

        //ChangeButtonComponentState(false);

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

        _hideTransition ??= StartCoroutine(HideTransition());

        //ChangeButtonComponentState(false);
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

        //ChangeButtonComponentState(false);
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
            Vector2 center = CorrectCenterPosition(GetCanvasPositionForObject(_nodeTransform));

            Vector2 targetUpPosition = center + Vector2.up * _distanceFromCenterWhenShown;
            Vector2 targetDownPosition = center + Vector2.down * _distanceFromCenterWhenShown;
            Vector2 targetRightPosition = center + Vector2.right * _distanceFromCenterWhenShown;
            Vector2 targetLeftPosition = center + Vector2.left * _distanceFromCenterWhenShown;
            
            float currentDistance = needToMove ? Mathf.Lerp(_distanceFromCenterWhenHidden, _distanceFromCenterWhenShown, Mathf.Clamp01(elapsed / _whenShowingMovementTime)) :
                _distanceFromCenterWhenShown;

            if (needToMove && elapsed > _whenShowingMovementTime)
            {
                needToMove = false;
                currentDistance = _distanceFromCenterWhenShown;
            }

            _upButtonTransform.anchoredPosition = center + Vector2.up * currentDistance;
            _downButtonTransform.anchoredPosition = center + Vector2.down * currentDistance;
            _rightButtonTransform.anchoredPosition = center + Vector2.right * currentDistance;
            _leftButtonTransform.anchoredPosition = center + Vector2.left * currentDistance;

            elapsed += Time.deltaTime;
            yield return null;
        }

        _updateButtonsPosition = null;
        _nodeTransform = null;

        //ChangeButtonComponentState(true);
    }

    private Vector2 CorrectCenterPosition(Vector2 center)
    {
        Vector2 minPosition = _canvasRect.rect.min + _buttonsSize * 0.5f + Vector2.one * (_distanceFromCenterWhenShown + _distanceFromScreenBounds);
        Vector2 maxPosition = _canvasRect.rect.max - _buttonsSize * 0.5f - Vector2.one * (_distanceFromCenterWhenShown + _distanceFromScreenBounds);

        float clampedX = Mathf.Clamp(center.x, minPosition.x, maxPosition.x);
        float clampedY = Mathf.Clamp(center.y, minPosition.y, maxPosition.y);

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

    /*private void ChangeButtonComponentState(bool state)
    {
        _upButton.enabled = state;
        _downButton.enabled = state;
        _rightButton.enabled = state;
        _leftButton.enabled = state;
    }*/
}
