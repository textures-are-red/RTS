using System;
using UnityEngine;
using UnityEngine.Events;

public class Field : MonoBehaviour
{
    public static event Action OnBackgroundClicked;
    [SerializeField] private UnityEvent _onClick;

    public void OnMouseDown()
    {
        if (HoverController.IsEnteredObject is false)
        {
            _onClick?.Invoke();
            OnBackgroundClicked?.Invoke();
        }
    }
}
