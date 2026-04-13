using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMover : IDisposable
{
    private const float _temporaryFieldHeight = 0f;

    private Transform _flatForward;
    private LayerMask _fieldMask;

    private CameraInputHandler _inputHandler;
    private CameraMovementSettings _settings;

    private Transform _cameraTransform;

    private Vector3 _moveVector;

    private Vector3? _dragStartWorldPoint;

    private Vector3 _mouseSmoothDelta;
    private Vector3 _currnetMouseVelocity;

    private Vector3 _currnetDefaultVelocity;

    private float _currentHeightChange;
    private float _currentHeightVelocity;

    public CameraMover(CameraInputHandler inputHandler, CameraMovementSettings settings, Transform faltForwar, LayerMask fieldMask)
    {
        _inputHandler = inputHandler;
        _settings = settings;
        _cameraTransform = Camera.main.transform;
        _flatForward = faltForwar;

        _inputHandler.MouseMoveEnabled += OnMouseInputEnabled;
        _inputHandler.MouseMoveDisabled += OnMouseInputDisabled;
    }

    public void ApplyMovement(Vector3 additionalMovementFromGamepad = default)
    {
        _moveVector = CalculateFlatMoveVector() + ApplyHeight() + additionalMovementFromGamepad;
        _cameraTransform.position += _moveVector;
    }

    private Vector3 CalculateFlatMoveVector()
    {    
        Vector3 mouseDelta = GetMouseInputDelta();
        Vector3 defaultDelta = GetDefaultInput();      

        return _flatForward.TransformDirection(defaultDelta + mouseDelta);
    }

    private Vector3 GetMouseInputDelta()
    {
        Vector3? currentPoint = GetMousePositionOnField();
        bool hasData = currentPoint.HasValue && _dragStartWorldPoint.HasValue;

        if (hasData)
        {
            Vector3 delta = _dragStartWorldPoint.Value - currentPoint.Value;
            /*_mouseSmoothDelta = Vector3.SmoothDamp(_mouseSmoothDelta, delta, ref _currnetMouseVelocity, _settings.MouseSmoothTime,
                _settings.MouseMaxSpeed, Time.deltaTime);*/
            /*return Vector3.SmoothDamp(Vector3.zero, worldDelta, ref _currnetMouseVelocity,
                _settings.MouseSmoothTime, _settings.MouseMaxSpeed, Time.deltaTime) * _settings.MouseSpeedMultiplier(_cameraTransform.position);*/
            return delta;
        }

        return Vector3.zero;
    }

    private Vector3 GetDefaultInput()
    {
        bool defaultDeviceIsKeyboardOrNull = _inputHandler.CurrentDefaultDevice is Keyboard or null;

        Vector2 defaultInput = _inputHandler.DefaultInput;
        Vector3 defaultFlatInput = new Vector3(defaultInput.x, 0f, defaultInput.y) * (defaultDeviceIsKeyboardOrNull ? _settings.KeyboardSpeed : _settings.GamepadSpeed);

        return Vector3.SmoothDamp(Vector3.zero, defaultFlatInput, ref _currnetDefaultVelocity,
            defaultDeviceIsKeyboardOrNull ? _settings.KeyboardSmoothTime : _settings.GamepadSmoothTime,
            defaultDeviceIsKeyboardOrNull ? _settings.KeyboardMaxSpeed : _settings.GamepadMaxSpeed,
            Time.deltaTime) * _settings.DefaultSpeedMultiplier(_cameraTransform.position);
    }

    private Vector3? GetMousePositionOnField()
    {
        if (Mouse.current is null)
            return null;

        Vector3 groundPoint = new Vector3(0f, _temporaryFieldHeight, 0f);
        Plane groundPlane = new Plane(Vector3.up, groundPoint);

        Vector2 screenPos = Mouse.current.position.ReadValue();

        if (MouseOutOfScreen(screenPos))
            return null;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            return hitPoint;
        }
        
        return null;
    }

    private Vector3 ApplyHeight()
    {
        float targetDeltaHeight = HoverController.IsEnteredObject ? 0f : _inputHandler.HeightInput;

        _currentHeightChange = Mathf.SmoothDamp(_currentHeightChange, targetDeltaHeight, ref _currentHeightVelocity,
            _inputHandler.CurrentHeightDevice is Mouse ? _settings.MouseWheelHeightSmoothTime : _settings.GamepadHeightSmoothTime,
            _inputHandler.CurrentHeightDevice is Mouse ? _settings.MouseWheelHeightMaxSpeed : _settings.GamepadHeightMaxSpeed, Time.deltaTime);

        return _cameraTransform.forward * _currentHeightChange *
               (_inputHandler.CurrentHeightDevice is Mouse ? _settings.MouseHeightSpeed : _settings.GamepadHeightSpeed);
    }

    private bool MouseOutOfScreen(Vector2 screenPos) => screenPos.x < 0f || screenPos.x > Screen.width || screenPos.y < 0f || screenPos.y > Screen.height;

    private void OnMouseInputEnabled()
    {
        Vector3? point = GetMousePositionOnField();

        _dragStartWorldPoint = point.HasValue ? point.Value : null;
        _mouseSmoothDelta = Vector3.zero;
        _currnetMouseVelocity = Vector3.zero;
    }

    private void OnMouseInputDisabled()
    {
        _dragStartWorldPoint = null;
    }

    public void Dispose()
    {
        _inputHandler.MouseMoveEnabled -= OnMouseInputEnabled;
        _inputHandler.MouseMoveDisabled -= OnMouseInputDisabled;
    }
}
