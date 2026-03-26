using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class UIShower : MonoBehaviour
{
    [SerializeField] private UnityEvent OnShow;
    [SerializeField] private UnityEvent OnHide;

    [Space(15)]

    [SerializeField] private GameObject _buttonsGameObject;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private List<MonoBehaviour> _copmonentsToDisable;

    [Space(15)]

    [SerializeField] private float _showTime = 0.15f;
    [SerializeField] private float _hideTime = 0.15f;

    public bool Shown { get; private set; }

    private InputSystem _input;

    private Coroutine _showCoroutine;
    private Coroutine _hideCoroutine;

    private void Awake()
    {
        _input = InputSystemHolder.Instance;
        ForceHide();
    }

    public void Toggle(InputAction.CallbackContext ctx)
    {
        if (Shown) Hide();
        else Show();
    }

    public void Show()
    {
        ForceHide();
        _buttonsGameObject.SetActive(true);

        _showCoroutine ??= StartCoroutine(ShowCoroutine());

        Shown = true;
        SetButtonsState(true);
        OnShow.Invoke();
    }

    public void Hide()
    {
        ForceHide(false);

        _hideCoroutine ??= StartCoroutine(HideCoroutine());

        Shown = false;
    }

    public void ForceHide(bool disactivateButtonsGameObject = true)
    {
        if (_showCoroutine is not null)
        {
            StopCoroutine(_showCoroutine);
            _showCoroutine = null;
        }

        if (_hideCoroutine is not null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }

        _canvasGroup.alpha = 0f;
        Shown = false;

        if (disactivateButtonsGameObject)
            _buttonsGameObject.SetActive(false);

        SetButtonsState(false);
        OnHide.Invoke();
    }

    private void SetButtonsState(bool state)
    {
        foreach (var component in _copmonentsToDisable)
            component.enabled = state;
    }

    private IEnumerator ShowCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < _showTime)
        {
            _canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / _showTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _showCoroutine = null;
    }

    private IEnumerator HideCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < _hideTime)
        {
            _canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / _hideTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _hideCoroutine = null;
        SetButtonsState(false);
        _buttonsGameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _input.Default.ToggleInventory.performed += Toggle;
    }

    private void OnDisable()
    {
        _input.Default.ToggleInventory.performed -= Toggle;
    }
}
