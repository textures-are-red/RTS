using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<HoverDetector> OnEnter;
    public event Action<HoverDetector> OnExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnExit?.Invoke(this);
    }

    private void OnEnable()
    {
        HoverController.AddDetector(this);
    }

    private void OnDisable()
    {
        HoverController.RemoveDetector(this);
    }
}