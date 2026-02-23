using UnityEngine;
using UnityEngine.Events;

public class Field : MonoBehaviour, IClickable
{
    [SerializeField] private UnityEvent _onClick;

    public void OnClick()
    {
        _onClick?.Invoke();
    }
}
