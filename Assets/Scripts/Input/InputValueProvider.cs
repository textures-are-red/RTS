using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InputValueProvider<T>
{
    public event Action<T> InputProcessed;
    private InputSystem _input;
    protected InputAction _action;

    public InputValueProvider(InputAction action)
    {
        _input = InputSystemHolder.Instance;
        _action = action;
    }

    public virtual void Enable()
    {
        _action.Enable();
        _action.performed += OnInputPerformed;
    }

    public virtual void Disable()
    {
        _action.performed -= OnInputPerformed;
        _action.Disable();
    }

    protected void OnInputPerformed(InputAction.CallbackContext ctx)
    {
        T processedInput = ReadValue(ctx);
        InputProcessed?.Invoke(processedInput);
    }

    protected abstract T ReadValue(InputAction.CallbackContext ctx);
}
