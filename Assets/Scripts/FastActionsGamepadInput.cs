using UnityEngine;
using UnityEngine.InputSystem;

public class FastActionsGamepadInput : InputValueProvider<Vector2Int>
{
    public FastActionsGamepadInput(InputAction action) : base(action) {}

    protected override Vector2Int ReadValue(InputAction.CallbackContext ctx)
    {
        return ctx.control.name[6] switch 
        {
            'N' => new Vector2Int(0, 1),
            'S' => new Vector2Int(0, -1),
            'W' => new Vector2Int(-1, 0),
            'E' => new Vector2Int(1, 0),
            _ => Vector2Int.zero
        };
    }
}
