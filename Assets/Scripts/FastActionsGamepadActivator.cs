using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FastActionsGamepadActivator : MonoBehaviour
{
    [SerializeField] private Button _upButton;
    [SerializeField] private Button _downButton;
    [SerializeField] private Button _rightButton;
    [SerializeField] private Button _leftButton;

    private FastActionsGamepadInput _fastActionsGamepadInput;
    private InputSystem _input;

    private void Awake()
    {
        _fastActionsGamepadInput = new(InputSystemHolder.Instance.Default.FastActions);
        _fastActionsGamepadInput.Enable();
        _fastActionsGamepadInput.InputProcessed += ActivateActions;
    }

    public void ActivateActions(Vector2Int actionsToActivate)
    {
        if (actionsToActivate.y is -1) _downButton.onClick.Invoke();
        if (actionsToActivate.y is 1) _upButton.onClick.Invoke();

        if (actionsToActivate.x is -1) _leftButton.onClick.Invoke();
        if (actionsToActivate.x is 1) _rightButton.onClick.Invoke();
    }

    private void OnDestroy()
    {
        _fastActionsGamepadInput.Disable();
        _fastActionsGamepadInput.InputProcessed -= ActivateActions;
    }
}
