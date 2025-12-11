using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Tooltip("Скорость передвижения камеры (мышь)")]
    [Min(0)]
    [SerializeField] private float _moveMouseSpeed = 1.25f;

    [Tooltip("Скорость передвижения камеры (клава)")]
    [Min(0)]
    [SerializeField] private float _moveKeyboardSpeed = 5f;

    [Tooltip("Скорость передвижения камеры (геймпад)")]
    [Min(0)]
    [SerializeField] private float _moveGamepadSpeed = 5f;

    [Tooltip("Время сглаживания движения")]
    [Min(0)]
    [SerializeField] private float _moveSmoothTime = 0.3f;

    private InputSystem _inputSystem;
    private InputDevice _device;
    private Camera _mainCamera;

    private Transform _mainCameraTransform;

    private Vector2 _inputVector;
    private Vector3 _moveVector;
    private Vector3 _currentSmoothMoveVelocity;

    private bool _mouseMoveAllowed;

    private bool _isInputFromMouse;
    private bool _isInputFromKeyboard;
    private bool _isInputFromGamepad;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _mainCameraTransform = _mainCamera.GetComponent<Transform>();
    }

    private void FixedUpdate() => Move();

    private void Move()
    {
        Vector3 flatInputVector = new Vector3(_inputVector.x, 0, _inputVector.y);
        _moveVector = Vector3.SmoothDamp(_moveVector, flatInputVector, ref _currentSmoothMoveVelocity, _moveSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);

        /*float finalMoveSpeed = _device switch
        {
            Mouse => _moveMouseSpeed,
            Keyboard => _moveKeyboardSpeed,
            Gamepad => _moveGamepadSpeed,
            _ => _moveKeyboardSpeed
        };*/

        _mainCameraTransform.position += Time.fixedDeltaTime * /*finalMoveSpeed*/_moveMouseSpeed * _moveVector;
    }

    private void OnMouseMovePerformed(InputAction.CallbackContext ctx)
    {
        if (_isInputFromKeyboard || _isInputFromGamepad) return;

        if (_mouseMoveAllowed is false)
        {
            _inputVector = Vector2.zero; return;
        }

        _inputVector = ctx.ReadValue<Vector2>();
        _isInputFromMouse = true;
    }

    private void OnMouseMoveCanceled(InputAction.CallbackContext ctx) 
    {
        if (_isInputFromMouse)
        {
            _inputVector = Vector2.zero;
            _isInputFromMouse = false;
        }
    }

    private void OnMouseMoveEnableStarted(InputAction.CallbackContext ctx) => _mouseMoveAllowed = true;
    private void OnMouseMoveEnableCanceled(InputAction.CallbackContext ctx) => _mouseMoveAllowed = false;

    private void OnDefautMovePerformed(InputAction.CallbackContext ctx)
    {
        if (_isInputFromMouse) return;

        _isInputFromKeyboard = ctx.control.device is Keyboard;
        _isInputFromGamepad = ctx.control.device is Gamepad;

        _inputVector = ctx.ReadValue<Vector2>();
    }

    private void OnDefautMoveCanceled(InputAction.CallbackContext ctx)
    {
        if (_isInputFromKeyboard || _isInputFromGamepad)
        {
            _inputVector = Vector2.zero;
            _isInputFromKeyboard = _isInputFromGamepad = false;
        }
    }

    private void OnEnable()
    {
        _inputSystem ??= new InputSystem();

        _inputSystem.Enable();

        var defMap = _inputSystem.Default;

        defMap.MouseMoveEnable.started += OnMouseMoveEnableStarted;
        defMap.MouseMoveEnable.canceled += OnMouseMoveEnableCanceled;

        defMap.MouseMove.performed += OnMouseMovePerformed;
        defMap.MouseMove.canceled += OnMouseMoveCanceled;

        defMap.DefaultMove.performed += OnDefautMovePerformed;
        defMap.DefaultMove.canceled += OnDefautMoveCanceled;
    }

    private void OnDisable()
    {
        var defMap = _inputSystem.Default;

        defMap.MouseMoveEnable.started -= OnMouseMoveEnableStarted;
        defMap.MouseMoveEnable.canceled -= OnMouseMoveEnableCanceled;

        defMap.MouseMove.performed -= OnMouseMovePerformed;
        defMap.MouseMove.canceled -= OnMouseMoveCanceled;

        defMap.DefaultMove.performed -= OnDefautMovePerformed;
        defMap.DefaultMove.canceled -= OnDefautMoveCanceled;

        _inputSystem.Disable();
    }
}
