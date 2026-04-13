using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadNodeNavigator : MonoBehaviour
{
    public bool _enableGamepadCenter = false;

    [SerializeField] private FastActionsPositioner _fastActionPositioner;
    [SerializeField] private CameraMovementSettings _settings;

    [Space(15)]

    [SerializeField] private GamepadOnNodeCenterer _gamepadOnNodeCenterer;
    [SerializeField] private CameraInputHandler _inputHandler;

    public Node NodeToCenter { get; private set; }

    private CameraNodeSelectionHandler _nodeSelector;
    private Transform _cameraTransform;

    public void Initialize(CameraNodeSelectionHandler nodeSelector)
    {
        _nodeSelector = nodeSelector;
        _cameraTransform = Camera.main.transform;
    }

    public Vector3 GetMovementVector()
    {
        if (_enableGamepadCenter && _inputHandler.DefaultInput == Vector2.zero && _inputHandler.CurrentDefaultDevice is Gamepad && _inputHandler.MouseInput == Vector2.zero)
        {
            return _gamepadOnNodeCenterer.CenterCameraOnNode(NodeToCenter);
        }

        return Vector3.zero;
    }

    private void Update()
    {
        bool isGamepadMode = _inputHandler.CurrentDefaultDevice is Gamepad && _nodeSelector.ClickedNode is null;

        if (isGamepadMode && (_inputHandler.MouseInput == Vector2.zero || _fastActionPositioner.ButtonsAreShown))
        {
            NodeToCenter = _gamepadOnNodeCenterer.FindNearestNode(Mathf.Max(_settings.NodeSearchRadiusMultiplier(_cameraTransform.position), 1f));
            CheckForClosestNodeGamepad();
        }
        else
        {
            NodeToCenter = null;

            if (_inputHandler.CurrentDefaultDevice is Gamepad)
                _inputHandler.ResetDefaultDevice();
        }
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
        else if (NodeToCenter is null && _fastActionPositioner.ButtonsAreShown)
            _fastActionPositioner.HideButtons();
    }
}
