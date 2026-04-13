using System;
using UnityEngine;
using UnityEngine.Events;

public class NodeClickHandler : MonoBehaviour
{
    public static event Action<Node> OnAnyNodeClicked;

    [SerializeField] private UnityEvent<Transform> _onClick;

    private Node _node;

    private void Awake()
    {
        _node = GetComponent<Node>();
    }

    private void OnMouseUpAsButton()
    {
        if (HoverController.IsEnteredObject is false)
        {
            _onClick?.Invoke(transform);
            OnAnyNodeClicked?.Invoke(_node);
        }
    }
}
