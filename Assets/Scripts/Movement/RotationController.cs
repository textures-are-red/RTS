using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotationController : MonoBehaviour
{
    private static readonly Vector3 _cameraCenter = new Vector3(0.5f, 0.5f, 0);

    [Header("Mouse")]
    [SerializeField] private float _mouseRotationSpeed = 5f;
    [SerializeField] private float _mouseRotationMaxSpeed = 10f;
    [SerializeField] private float _mouseRotationSmoothTime = 0.05f;

    [Header("Keyboard")]
    [SerializeField] private float _keyboardRotationSpeed = 5f;
    [SerializeField] private float _keyboardRotationMaxSpeed = 10f;
    [SerializeField] private float _keyboardRotationSmoothTime = 0.05f;

    [Header("Gamepad")]
    [SerializeField] private float _gamepadRotationSpeed = 5f;
    [SerializeField] private float _gamepadRotationMaxSpeed = 10f;
    [SerializeField] private float _gamepadRotationSmoothTime = 0.05f;

    [Space(15)]

    [SerializeField] private float _rotationOriginRayRange = 100f;
    [SerializeField] private LayerMask _rotationOriginRayMask;

    private InputSystem _input;
    private InputDevice _currentDefaultRotationDevice;

    private MouseRotationEnabler _mouseRotationEnabler;

    private Camera _cameraMain;
    private Transform _cameraTransform;
    private Coroutine _mouseInputRead;

    private float _defaultRotationInput;
    private float _smoothedDefaultRotationInput;
    private float _currentDefaultRotationVelocity;

    private float _mouseRotationInput;
    private float _smoothedMouseRotationInput;
    private float _currentMouseRotationVelocity;

    public void Awake()
    {
        _mouseRotationEnabler = GetComponent<MouseRotationEnabler>();
        //_mouseRotationEnabler.Initialize();

        _cameraMain = Camera.main;
        _cameraTransform = _cameraMain.transform;

        _input = InputSystemHolder.Instance;
    }

    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        Ray fromCameraCenter = _cameraMain.ViewportPointToRay(_cameraCenter);
        RaycastHit hitFromCameraCenter;
        Physics.Raycast(fromCameraCenter, out hitFromCameraCenter, _rotationOriginRayRange, _rotationOriginRayMask, QueryTriggerInteraction.Ignore);

        float currentDefaultSpeed = CurrentDefaultSpeed(_currentDefaultRotationDevice);
        float currentDefaultSmoothTime = CurrentDefaultSmoothTime(_currentDefaultRotationDevice);
        float currentDefaultMaxSpeed = CurrentDefaultMaxSpeed(_currentDefaultRotationDevice);

        _smoothedDefaultRotationInput = Mathf.SmoothDamp(_smoothedDefaultRotationInput, _defaultRotationInput, ref _currentDefaultRotationVelocity, currentDefaultSmoothTime, currentDefaultMaxSpeed, Time.deltaTime);
        _smoothedMouseRotationInput = Mathf.SmoothDamp(_smoothedMouseRotationInput, _mouseRotationInput, ref _currentMouseRotationVelocity, _mouseRotationSmoothTime, _mouseRotationMaxSpeed, Time.deltaTime);

        float currentSmoothInput = _smoothedDefaultRotationInput * currentDefaultSpeed + _smoothedMouseRotationInput * _mouseRotationSpeed;

        _cameraTransform.RotateAround(hitFromCameraCenter.point, Vector3.up, currentSmoothInput * Time.deltaTime);
    }

    private float CurrentDefaultSmoothTime(InputDevice device) => device switch
        {
            Keyboard => _keyboardRotationSmoothTime,
            //Mouse => _mouseRotationSmoothTime,
            Gamepad => _gamepadRotationSmoothTime,
            _ => _keyboardRotationSmoothTime
        };
    
    private float CurrentDefaultMaxSpeed(InputDevice device) => device switch
        {
            Keyboard => _keyboardRotationMaxSpeed,
            //Mouse => _mouseRotationMaxSpeed,
            Gamepad => _gamepadRotationMaxSpeed,
            _ => _keyboardRotationMaxSpeed
        };
    
    private float CurrentDefaultSpeed(InputDevice device) => device switch
        {
            Keyboard => _keyboardRotationSpeed,
            //Mouse => _mouseRotationSpeed,
            Gamepad => _gamepadRotationSpeed,
            _ => _keyboardRotationSpeed
        };

    private void OnRotatePerformed(InputAction.CallbackContext ctx)
    {
        _defaultRotationInput = ctx.ReadValue<float>();
        _currentDefaultRotationDevice = ctx.control.device;
    }

    private void OnRotateCanceled(InputAction.CallbackContext ctx) => _defaultRotationInput = 0f;

    private void OnMouseRotationEnable() => _mouseInputRead = StartCoroutine(MouseInputRead());

    private IEnumerator MouseInputRead()
    {
        while (true)
        {
            _mouseRotationInput = _input.Default.MouseRotate.ReadValue<float>();
            yield return null;
        }
    }

    private void OnMouseRotationDisable()
    {
        if (_mouseInputRead is not null)
        {
            StopCoroutine(_mouseInputRead);
            _mouseInputRead = null;
        }

        _mouseRotationInput = 0f;
    }

    private void OnEnable()
    {
        _mouseRotationEnabler.Enable += OnMouseRotationEnable;
        _mouseRotationEnabler.Disable += OnMouseRotationDisable;
        
        _input.Default.Rotate.performed += OnRotatePerformed;
        _input.Default.Rotate.canceled += OnRotateCanceled;
    }

    private void OnDisable()
    {
        _mouseRotationEnabler.Enable -= OnMouseRotationEnable;
        _mouseRotationEnabler.Disable -= OnMouseRotationDisable;

        _input.Default.Rotate.performed -= OnRotatePerformed;
        _input.Default.Rotate.canceled -= OnRotateCanceled;
    }
}
