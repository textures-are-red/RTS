using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInputHandler : MonoBehaviour
{
    public event Action DefaultDeviceChanged;

    public event Action MouseMoveEnabled;
    public event Action MouseMoveDisabled;

    public Vector2 DefaultInput { get; private set; }
    public Vector2 MouseInput { get; private set; }

    public float HeightInput { get; private set; }
    
    public InputDevice CurrentDefaultDevice { get; private set; }
    public InputDevice CurrentHeightDevice { get; private set; }

    private InputSystem _input;
    private InputDevice _lastDefaultDevice;
    private Coroutine _mouseMoveCoroutine;
    private Coroutine _defaultMoveCoroutine;

    private MouseMoveEnabler _mouseMoveEnabler;

    public void ResetDefaultDevice() => CurrentDefaultDevice = null; 

    private void Awake()
    {
        _input = InputSystemHolder.Instance;
        _mouseMoveEnabler = new();
    }

    private void OnMouseMoveEnable()
    {
        _mouseMoveCoroutine ??= StartCoroutine(MouseInputRead());
        MouseMoveEnabled?.Invoke();
    }

    private IEnumerator MouseInputRead()
    {
        while (_mouseMoveEnabler.CanMouseMove)
        {
            MouseInput = _input.Default.MouseMove.ReadValue<Vector2>();
            yield return null;
        }

        MouseInput = Vector2.zero;
        _mouseMoveCoroutine = null;
    }

    private void OnMouseMoveDisable()
    {
        if (_mouseMoveCoroutine is not null)
        {
            StopCoroutine(_mouseMoveCoroutine);
            _mouseMoveCoroutine = null;
            MouseInput = Vector2.zero;
        }

        MouseMoveDisabled?.Invoke();
    }

    private void OnDefaultMoveStarted(InputAction.CallbackContext ctx) => _defaultMoveCoroutine ??= StartCoroutine(DefaultMoveRead(ctx));

    private IEnumerator DefaultMoveRead(InputAction.CallbackContext ctx)
    {
        while (true)
        {
            DefaultInput = ctx.ReadValue<Vector2>();
            _lastDefaultDevice = CurrentDefaultDevice;
            CurrentDefaultDevice = ctx.control.device;

            if (_lastDefaultDevice != CurrentDefaultDevice) DefaultDeviceChanged?.Invoke();

            yield return null;
        }
    }

    private void OnDefaultMoveCanceled(InputAction.CallbackContext ctx)
    {
        if (_defaultMoveCoroutine is not null)
        {
            StopCoroutine(_defaultMoveCoroutine);
            _defaultMoveCoroutine = null;
            DefaultInput = Vector2.zero;
        }
    }

    private void OnHeightPerformed(InputAction.CallbackContext ctx)
    {
        HeightInput = ctx.ReadValue<float>();
        CurrentHeightDevice = ctx.control.device;

        if (CurrentHeightDevice is Mouse) HeightInput = Mathf.Sign(HeightInput) * 1f;
    }

    private void OnHeightCanceled(InputAction.CallbackContext ctx) => HeightInput = 0f;

    private void OnEnable()
    {
        if (_input.asset.enabled is false)
            _input.Enable();
        
        _mouseMoveEnabler.Enable += OnMouseMoveEnable;
        _mouseMoveEnabler.Disable += OnMouseMoveDisable;

        _input.Default.DefaultMove.started += OnDefaultMoveStarted;
        _input.Default.DefaultMove.canceled += OnDefaultMoveCanceled;

        _input.Default.Height.performed += OnHeightPerformed;
        _input.Default.Height.canceled += OnHeightCanceled;
    }

    private void OnDisable()
    {
        _input.Disable();
        
        _mouseMoveEnabler.Enable -= OnMouseMoveEnable;
        _mouseMoveEnabler.Disable -= OnMouseMoveDisable;

        _input.Default.DefaultMove.started -= OnDefaultMoveStarted;
        _input.Default.DefaultMove.canceled -= OnDefaultMoveCanceled;

        _input.Default.Height.performed -= OnHeightPerformed;
        _input.Default.Height.canceled -= OnHeightCanceled;
    }
}
