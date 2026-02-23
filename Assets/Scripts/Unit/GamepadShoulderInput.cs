using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadShoulderInput : InputValueProvider<sbyte>
{
    public GamepadShoulderInput(InputAction action) : base(action) {}

    protected override sbyte ReadValue(InputAction.CallbackContext ctx)
    {
        if (ctx.control.name[0] is 'r') return 1;
        return -1;
    }
}
