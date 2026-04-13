using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMover : IDisposable
{
    private CameraInputHandler _inputHandler;
    private CameraMovementSettings _settings;

    private MovementMouseDragPointHandler _pointHandler;

    private Transform _flatForward;
    
    private Transform _cameraTransform;

    private Vector3 _currnetDefaultVelocity;

    private float _currentHeightChange;
    private float _currentHeightVelocity;

    public CameraMover(CameraInputHandler inputHandler, CameraMovementSettings settings, Transform faltForwar)
    {
        _inputHandler = inputHandler;
        _settings = settings;
        _flatForward = faltForwar;

        _cameraTransform = Camera.main.transform;

        _pointHandler = new(_inputHandler);
        _pointHandler.OnInputHandleEnabled = true;
    }

    public Vector3 GetMouseInputDelta()
    {
        Vector3? currentPoint = _pointHandler.GetMousePositionOnField();
        bool hasData = currentPoint.HasValue && _pointHandler.DragPoint.HasValue;

        if (hasData)
            return _pointHandler.DragPoint.Value - currentPoint.Value;

        return Vector3.zero;
    }

    public Vector3 GetDefaultInput()
    {
        bool defaultDeviceIsKeyboardOrNull = _inputHandler.CurrentDefaultDevice is Keyboard or null;

        Vector2 defaultInput = _inputHandler.DefaultInput;
        Vector3 defaultFlatInput = new Vector3(defaultInput.x, 0f, defaultInput.y) * (defaultDeviceIsKeyboardOrNull ? _settings.KeyboardSpeed : _settings.GamepadSpeed);

        return Vector3.SmoothDamp(Vector3.zero, defaultFlatInput, ref _currnetDefaultVelocity,
            defaultDeviceIsKeyboardOrNull ? _settings.KeyboardSmoothTime : _settings.GamepadSmoothTime,
            defaultDeviceIsKeyboardOrNull ? _settings.KeyboardMaxSpeed : _settings.GamepadMaxSpeed,
            Time.deltaTime) * _settings.DefaultSpeedMultiplier(_cameraTransform.position);
    }

    public Vector3 ApplyHeight()
    {
        float targetDeltaHeight = HoverController.IsEnteredObject ? 0f : _inputHandler.HeightInput;

        _currentHeightChange = Mathf.SmoothDamp(_currentHeightChange, targetDeltaHeight, ref _currentHeightVelocity,
            _inputHandler.CurrentHeightDevice is Mouse ? _settings.MouseWheelHeightSmoothTime : _settings.GamepadHeightSmoothTime,
            _inputHandler.CurrentHeightDevice is Mouse ? _settings.MouseWheelHeightMaxSpeed : _settings.GamepadHeightMaxSpeed, Time.deltaTime);

        return _cameraTransform.forward * _currentHeightChange *
               (_inputHandler.CurrentHeightDevice is Mouse ? _settings.MouseHeightSpeed : _settings.GamepadHeightSpeed);
    }

    public void MoveTheMouseReferencePoint(Vector3 delta)
    {
        _pointHandler.DragPoint += delta;
    }

    public void Dispose()
    {
        _pointHandler.Dispose();
    }
}
