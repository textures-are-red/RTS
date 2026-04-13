using UnityEngine;

[CreateAssetMenu(fileName = "CameraMovementSettings", menuName = "Camera Movement Settings")]
public class CameraMovementSettings : ScriptableObject
{
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

    public float MouseSpeedMultiplier(Vector3 cameraPosition) => Mathf.Clamp(cameraPosition.y / ReferenceMouseHeight, ClampReferenceMouseHeight.x, ClampReferenceMouseHeight.y);
    public float DefaultSpeedMultiplier(Vector3 cameraPosition) => Mathf.Clamp(cameraPosition.y / ReferenceDefaultHeight, ClampReferenceDefaultHeight.x, ClampReferenceDefaultHeight.y);
    public float NodeSearchRadiusMultiplier(Vector3 cameraPosition) => Mathf.Clamp(cameraPosition.y / ReferenceNodeSearchRadius, ClampReferenceNodeSearchRadius.x, ClampReferenceNodeSearchRadius.y);

    public float ReferenceMouseHeight => _referenceMouseHeight;
    public Vector2 ClampReferenceMouseHeight => _clampReferenceMouseHeight;
    public float ReferenceDefaultHeight => _referenceDefaultHeight;
    public Vector2 ClampReferenceDefaultHeight => _clampReferenceDefaultHeight;
    public float ReferenceNodeSearchRadius => _referenceNodeSearchRadius;
    public Vector2 ClampReferenceNodeSearchRadius => _clampReferenceNodeSearchRadius;

    //Mouse
    public float MouseSpeed => _mouseSpeed;
    public float MouseSmoothTime => _mouseSmoothTime;
    public float MouseMaxSpeed => _mouseMaxSpeed;

    public float MouseHeightSpeed => _mouseHeightSpeed;
    public float MouseWheelHeightSmoothTime => _mouseWheelHeightSmoothTime;
    public float MouseWheelHeightMaxSpeed => _mouseWheelHeightMaxSpeed;

    //Keyboard
    public float KeyboardSpeed => _keyboardSpeed;
    public float KeyboardSmoothTime => _keyboardSmoothTime;
    public float KeyboardMaxSpeed => _keyboardMaxSpeed;

    //Gamepad
    public float GamepadSpeed => _gamepadSpeed;
    public float GamepadSmoothTime => _gamepadSmoothTime;
    public float GamepadMaxSpeed => _gamepadMaxSpeed;

    public float GamepadHeightSpeed => _gamepadHeightSpeed;
    public float GamepadHeightSmoothTime => _gamepadHeightSmoothTime;
    public float GamepadHeightMaxSpeed => _gamepadHeightMaxSpeed;
}
