using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementMouseDragPointHandler : IDisposable
{
    private const float _temporaryFieldHeight = 0f;

    public bool OnInputHandleEnabled;
    public Vector3? DragPoint;

    private CameraInputHandler _inputHandler;
    private Camera _cameraMain;

    public MovementMouseDragPointHandler(CameraInputHandler inputHandler)
    {
        _inputHandler = inputHandler;
        _cameraMain = Camera.main;

        _inputHandler.MouseMoveEnabled += OnMouseInputEnabled;
        _inputHandler.MouseMoveDisabled += OnMouseInputDisabled;
    }

    public Vector3? GetMousePositionOnField()
    {
        if (Mouse.current is null)
            return null;

        Vector3 groundPoint = new Vector3(0f, _temporaryFieldHeight, 0f);
        Plane groundPlane = new Plane(Vector3.up, groundPoint);

        Vector2 screenPos = Mouse.current.position.ReadValue();

        if (MouseOutOfScreen(screenPos))
            return null;

        Ray ray = _cameraMain.ScreenPointToRay(screenPos);

        if (groundPlane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);
        
        return null;
    }

    public void Dispose()
    {
        _inputHandler.MouseMoveEnabled -= OnMouseInputEnabled;
        _inputHandler.MouseMoveDisabled -= OnMouseInputDisabled;
    }

    private bool MouseOutOfScreen(Vector2 screenPos) => screenPos.x < 0f || screenPos.x > Screen.width || screenPos.y < 0f || screenPos.y > Screen.height;

    private void OnMouseInputEnabled()
    {
        Vector3? point = GetMousePositionOnField();
        DragPoint = point.HasValue ? point.Value : null;
    }

    private void OnMouseInputDisabled()
    {
        DragPoint = null;
    }
}
