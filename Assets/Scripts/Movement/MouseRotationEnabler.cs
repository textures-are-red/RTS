using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseRotationEnabler : MonoBehaviour
{
    [SerializeField] private float _toEnableMouseRotate = 0.5f;

    public event Action Enable;
    public event Action Disable;

    public bool CanMouseRotate { get; private set; }

    private InputSystem _input;
    private Camera _mainCamera;
    
    private Coroutine _mouseRotateEnableCoroutine;
    private float _elapsedFromRotateEnableStarted;

    public void Awake()
    {
        _input = InputSystemHolder.Instance;
        _mainCamera = Camera.main;

        _input.Default.MouseRotateEnable.performed += OnMouseEnablePerformed;
        _input.Default.MouseRotateEnable.canceled += OnMouseEnableCanceled;
    }

    private void OnMouseEnablePerformed(InputAction.CallbackContext ctx) => _mouseRotateEnableCoroutine ??= StartCoroutine(WaitForEnable());

    private IEnumerator WaitForEnable()
    {
        while(_elapsedFromRotateEnableStarted < _toEnableMouseRotate)
        {
            _elapsedFromRotateEnableStarted += Time.deltaTime;
            yield return null;
        }

        CanMouseRotate = true;
        _mouseRotateEnableCoroutine = null;
        
        Enable?.Invoke();
    }

    private void OnMouseEnableCanceled(InputAction.CallbackContext ctx)
    {
        CanMouseRotate = false;
        _elapsedFromRotateEnableStarted = 0f;

        if (_mouseRotateEnableCoroutine is not null)
        {
            StopCoroutine(_mouseRotateEnableCoroutine);
            _mouseRotateEnableCoroutine = null;
        }

        Disable?.Invoke();
    }

    private void OnDisable()
    {
        if (_mouseRotateEnableCoroutine is not null)
        {
            StopCoroutine(_mouseRotateEnableCoroutine);
            _mouseRotateEnableCoroutine = null;
        }

        if (_input is null) return;

        _input.Default.MouseRotateEnable.performed -= OnMouseEnablePerformed;
        _input.Default.MouseRotateEnable.canceled -= OnMouseEnableCanceled;
    }
}
