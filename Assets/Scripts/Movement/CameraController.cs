using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MouseMoveEnable))]
public class CameraController : MonoBehaviour
{
    public bool _enableCenter = false;
    [SerializeField] private FastActionsPositioner _fastActionPositioner;

    [Space(15)]

    [SerializeField] private Transform _flatForward;
    [SerializeField] private float _referenceMouseHeight = 30f;
    [SerializeField] private Vector2 _clampReferenceMouseHeight;
    [SerializeField] private float _referenceDefaultHeight = 30f;
    [SerializeField] private Vector2 _clampReferenceDefaultHeight;
    [SerializeField] private float _referenceNodeSearchRadius = 20f;
    [SerializeField] private Vector2 _clampReferenceNodeSearchRadius;

    [Header("Mouse")]
    [SerializeField] private float _mouseSpeed = 2f;
    [SerializeField] private float _mouseSmoothTime = 0.1f;
    [SerializeField] private float _mouseMaxSpeed = 50f;

    [Space(15)]

    [SerializeField] private float _mouseHeightSpeed = 4f;
    [SerializeField] private float _mouseWheelHeightSmoothTime = 0.1f;
    [SerializeField] private float _mouseWheelHeightMaxSpeed = 50f;

    [Header("Keyboard")]
    [SerializeField] private float _keyboardSpeed = 5f;
    [SerializeField] private float _keyboardSmoothTime = 0.1f;
    [SerializeField] private float _keyboardMaxSpeed = 50f;

    [Header("Gamepad")]
    [SerializeField] private float _gamepadSpeed = 5f;
    [SerializeField] private float _gamepadSmoothTime = 0.1f;
    [SerializeField] private float _gamepadMaxSpeed = 50f;

    [Space(15)]

    [SerializeField] private float _gamepadHeightSpeed = 4f;
    [SerializeField] private float _gamepadHeightSmoothTime = 0.1f;
    [SerializeField] private float _gamepadHeightMaxSpeed = 50f;

    public event Action DefaultDeviceChanged;
    public Node NodeToCenter { get; private set; }

    private float _mouseSpeedMultiplier => Mathf.Clamp(_cameraTransform.position.y / _referenceMouseHeight, _clampReferenceMouseHeight.x, _clampReferenceMouseHeight.y);
    private float _defaultSpeedMultiplier => Mathf.Clamp(_cameraTransform.position.y / _referenceDefaultHeight, _clampReferenceDefaultHeight.x, _clampReferenceDefaultHeight.y);
    private float _nodeSearchRadiusMultiplier => Mathf.Clamp(_cameraTransform.position.y / _referenceNodeSearchRadius, _clampReferenceNodeSearchRadius.x, _clampReferenceNodeSearchRadius.y);

    private InputSystem _input;
    private InputDevice _currentDefaultDevice;
    private InputDevice _lastDefaultDevice;
    private InputDevice _currentHeightDevice;

    private Transform _cameraTransform;

    private MouseMoveEnable _mouseMoveEnabler;
    private GamepadOnNodeCenterer _gamepadOnNodeCenterer;

    private Coroutine _mouseMoveCoroutine;
    private Coroutine _defaultMoveCoroutine;

    private Vector3 _moveVector;
    private Vector2 _defaultInput;
    private Vector2 _mouseInput;

    private float _heightInput;
    private float _currentHeightChange;

    private Vector3 _currnetMouseVelocity;
    private Vector3 _currnetDefaultVelocity;
    private float _currentHeightVelocity;

    private void Awake()
    {
        _input = InputSystemHolder.Instance;
        _cameraTransform = Camera.main.transform;

        _mouseMoveEnabler = GetComponent<MouseMoveEnable>();
        _gamepadOnNodeCenterer = GetComponent<GamepadOnNodeCenterer>();
    }

    private void Update()
    {
        if (_currentDefaultDevice is Gamepad && _mouseInput == Vector2.zero && _mouseMoveEnabler.ClickedNode is null)
            NodeToCenter = _gamepadOnNodeCenterer.FindNearestNode(Mathf.Max(_nodeSearchRadiusMultiplier, 1f));
        else
        {
            NodeToCenter = null;
            
            if (_currentDefaultDevice is Gamepad)
                _currentDefaultDevice = null;
        }

        Move();

        if (_currentDefaultDevice is Gamepad)
            CheckForClosestNodeGamepad();
    }

    private void Move()
    {
        if (_enableCenter && _defaultInput == Vector2.zero && _currentDefaultDevice is Gamepad && _mouseInput == Vector2.zero)
            _moveVector = _gamepadOnNodeCenterer.CenterCameraOnNode(NodeToCenter) + ApplyHeight();
        else
            _moveVector = CalculateFlatMoveVector() + ApplyHeight();

        _cameraTransform.position += _moveVector;
    }

    private void CheckForClosestNodeGamepad()
    {
        if (NodeToCenter is not null && _fastActionPositioner.ButtonsAreShown is false)
            _fastActionPositioner.ShowButtons(NodeToCenter.transform);
        else if (NodeToCenter is not null && _fastActionPositioner.ButtonsAreShown && NodeToCenter.transform != _fastActionPositioner.CurrentNodeTransform)
        {
            _fastActionPositioner.ForceHide();
            _fastActionPositioner.ShowButtons(NodeToCenter.transform);
        }
        else if (NodeToCenter is null && _fastActionPositioner.ButtonsAreShown || NodeToCenter is null)
            _fastActionPositioner.HideButtons();
    }

    private Vector3 CalculateFlatMoveVector()
    {
        bool defaultDeviceIsKeyboardOrNull = _currentDefaultDevice is Keyboard or null;

        Vector3 mouseFlatInput = new Vector3(_mouseInput.x, 0f, _mouseInput.y) * _mouseSpeed;
        Vector3 defaultFlatInput = new Vector3(_defaultInput.x, 0f, _defaultInput.y) * (defaultDeviceIsKeyboardOrNull ? _keyboardSpeed : _gamepadSpeed);

        Vector3 smoothedMouseFlatInput = Vector3.SmoothDamp(Vector3.zero, mouseFlatInput, ref _currnetMouseVelocity, _mouseSmoothTime, _mouseMaxSpeed, Time.deltaTime) * _mouseSpeedMultiplier;
        Vector3 smoothedDefaultFlatInput = Vector3.SmoothDamp(Vector3.zero, defaultFlatInput, ref _currnetDefaultVelocity,
            defaultDeviceIsKeyboardOrNull ? _keyboardSmoothTime : _gamepadSmoothTime, defaultDeviceIsKeyboardOrNull ? _keyboardMaxSpeed : _gamepadMaxSpeed, Time.deltaTime) * _defaultSpeedMultiplier;
        
        return _flatForward.TransformDirection(smoothedDefaultFlatInput + smoothedMouseFlatInput);
    }

    private Vector3 ApplyHeight()
    {
        _currentHeightChange = Mathf.SmoothDamp(_currentHeightChange, _heightInput, ref _currentHeightVelocity,
            _currentHeightDevice is Mouse ? _mouseWheelHeightSmoothTime : _gamepadHeightSmoothTime, _currentHeightDevice is Mouse ? _mouseWheelHeightMaxSpeed : _gamepadHeightMaxSpeed, Time.deltaTime);

        return _cameraTransform.forward * _currentHeightChange * (_currentHeightDevice is Mouse ? _mouseHeightSpeed : _gamepadHeightSpeed);
    }

    private void OnMouseMoveEnable() => _mouseMoveCoroutine ??= StartCoroutine(MouseInputRead());

    private IEnumerator MouseInputRead()
    {
        while (_mouseMoveEnabler.CanMouseMove)
        {
            _mouseInput = _input.Default.MouseMove.ReadValue<Vector2>();
            yield return null;
        }

        _mouseInput = Vector2.zero;
        _mouseMoveCoroutine = null;
    }

    private void OnMouseMoveDisable()
    {
        if (_mouseMoveCoroutine is not null)
        {
            StopCoroutine(_mouseMoveCoroutine);
            _mouseMoveCoroutine = null;
            _mouseInput = Vector2.zero;
        }
    }

    private void OnDefaultMoveStarted(InputAction.CallbackContext ctx) => _defaultMoveCoroutine ??= StartCoroutine(DefaultMoveRead(ctx));

    private IEnumerator DefaultMoveRead(InputAction.CallbackContext ctx)
    {
        while (true)
        {
            _defaultInput = ctx.ReadValue<Vector2>();
            _lastDefaultDevice = _currentDefaultDevice;
            _currentDefaultDevice = ctx.control.device;

            if (_lastDefaultDevice != _currentDefaultDevice) DefaultDeviceChanged?.Invoke();

            yield return null;
        }
    }

    private void OnDefaultMoveCanceled(InputAction.CallbackContext ctx)
    {
        if (_defaultMoveCoroutine is not null)
        {
            StopCoroutine(_defaultMoveCoroutine);
            _defaultMoveCoroutine = null;
            _defaultInput = Vector2.zero;
        }
    }

    private void OnHeightPerformed(InputAction.CallbackContext ctx)
    {
        _heightInput = ctx.ReadValue<float>();
        _currentHeightDevice = ctx.control.device;

        if (_currentHeightDevice is Mouse) _heightInput = Mathf.Sign(_heightInput) * 1f;
    }

    private void OnHeightCanceled(InputAction.CallbackContext ctx) => _heightInput = 0f;

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

        DefaultDeviceChanged += _mouseMoveEnabler.ClearClickedNode;
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

        DefaultDeviceChanged -= _mouseMoveEnabler.ClearClickedNode;
    }
}
