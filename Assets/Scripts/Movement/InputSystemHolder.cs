using System;
using UnityEngine.InputSystem;

public static class InputSystemHolder
{
    private static readonly Lazy<InputSystem> _lazyInstance = new(() =>
    {
        var instance = new InputSystem();
        instance.Enable();
        return instance;
    });

    public static InputSystem Instance => _lazyInstance.Value;
}
