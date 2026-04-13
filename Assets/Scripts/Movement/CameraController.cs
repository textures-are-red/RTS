using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _flatForward;
    [SerializeField] private CameraMovementSettings _settings;

    [Space(15)]

    [SerializeField] private CameraInputHandler _inputHandler;
    [SerializeField] private GamepadNodeNavigator _gamepadNavigator;

    public Node ClickedNode => _nodeSelector.ClickedNode;
    public Node NodeToCenter => _gamepadNavigator.NodeToCenter;

    private CameraMover _mover;
    private CameraNodeSelectionHandler _nodeSelector;

    private Transform _cameraTransform;

    private void Awake()
    {
        _mover = new(_inputHandler, _settings, _flatForward);
        _nodeSelector = new(_inputHandler);
        _gamepadNavigator.Initialize(_nodeSelector);

        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector3 gamepadCenterMovement = _gamepadNavigator.GetMovementVector();

        Vector3 defaultMovement = _mover.GetDefaultInput();
        _mover.MoveTheMouseReferencePoint(defaultMovement);       
        Vector3 mouseMovement = _mover.GetMouseInputDelta();

        Vector3 heightMovement = _mover.ApplyHeight();

        _cameraTransform.position += _flatForward.TransformDirection(defaultMovement + mouseMovement) + gamepadCenterMovement + heightMovement;
    }

    private void OnEnable()
    {
        _nodeSelector.Enable();
    }

    private void OnDisable()
    {
        _nodeSelector.Disable();
    }

    private void OnDestroy()
    {
        _mover.Dispose();
    }
}
