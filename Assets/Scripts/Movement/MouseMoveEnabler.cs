using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MouseMoveEnabler : IDisposable
{
    public event Action Enable;
    public event Action Disable;

    public bool CanMouseMove { get; private set; }

    private InputSystem _input;
    
    public MouseMoveEnabler()
    {
        _input = InputSystemHolder.Instance;

        _input.Default.MouseMoveEnable.performed += OnMouseEnablePerformed;
        _input.Default.MouseMoveEnable.canceled += OnMouseEnableCanceled;
    }

    public void Dispose()
    {
        _input.Default.MouseMoveEnable.performed -= OnMouseEnablePerformed;
        _input.Default.MouseMoveEnable.canceled -= OnMouseEnableCanceled;
    }

    private void OnMouseEnablePerformed(InputAction.CallbackContext ctx)
    {
        if (HoverController.IsEnteredObject is false)
        {
            CanMouseMove = true;
            Enable?.Invoke();
        }
    }

    private void OnMouseEnableCanceled(InputAction.CallbackContext ctx)
    {
        CanMouseMove = false;
        Disable?.Invoke();
    }
}
