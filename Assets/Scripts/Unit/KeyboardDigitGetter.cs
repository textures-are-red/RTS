using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardDigitGetter : InputValueProvider<byte>
{
    public KeyboardDigitGetter(InputAction action) : base(action) {}

    protected override byte ReadValue(InputAction.CallbackContext ctx)
    {
        byte.TryParse(ctx.control.name, out byte digit);        
        return digit;
    }
}
