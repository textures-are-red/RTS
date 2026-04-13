using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _flatForward;
    [SerializeField] private CameraMovementSettings _settings;
    [SerializeField] private LayerMask _fieldMask;

    [Space(15)]

    [SerializeField] private CameraInputHandler _inputHandler;
    [SerializeField] private GamepadNodeNavigator _gamepadNavigator;

    public Node ClickedNode => _nodeSelector.ClickedNode;
    public Node NodeToCenter => _gamepadNavigator.NodeToCenter;

    private CameraMover _mover;
    private CameraNodeSelectionHandler _nodeSelector;

    private void Awake()
    {
        _mover = new(_inputHandler, _settings, _flatForward, _fieldMask);
        _nodeSelector = new(_inputHandler);
        _gamepadNavigator.Initialize(_nodeSelector);
    }

    private void Update()
    {
        Vector3 movement = _gamepadNavigator.GetMovementVector();
        _mover.ApplyMovement(movement);
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
