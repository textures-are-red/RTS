using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private ItemDisplay _display;

    [Space (15)]

    [SerializeField, Range(0f, 1f)] private float _transparentFactor = 0.6f;

    public RectTransform RectTransform => _display.RectTransform;

    public SlotCard SlotCard { get; private set; }
    public Inventory Inventory { get; private set; }

    public Item Item
    {
        get => _display.Item;
        set => _display.Item = value;
    }

    private Canvas _canvas;

    public void Initialize(SlotCard slotCard, Inventory inventory)
    {
        SlotCard = slotCard;
        Inventory = inventory;
    }

    public void SetAsPreview(Canvas canvas)
    {
        _display.SetAsPreview();
        _canvas = canvas;
    }

    public void BecomeTransparent()
    {
        if (_display.CanvasGroup is null) return;
        
        _display.CanvasGroup.alpha = _transparentFactor;
        _display.CanvasGroup.blocksRaycasts = false;
    }

    public void BecomeOpaque()
    {
        if (_display.CanvasGroup is null) return;
        
        _display.CanvasGroup.alpha = 1f;
        _display.CanvasGroup.blocksRaycasts = true;
    }

    public void UpdateInfo() => _display.UpdateInfo();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_display?.Item is not null && Inventory is not null)
            InventoryDisplaysHolder.Instance.TransferItemBetweenDisplays(_display.Item, Inventory);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BecomeTransparent();

        if (ItemDragPreviewService.Instance?.IsShown is false)
            ItemDragPreviewService.Instance?.ShowPreview(_display);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ItemDragPreviewService.Instance?.UpdatePosition(eventData.delta / (_canvas is null ? 1f : _canvas.scaleFactor));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        BecomeOpaque();
        ItemDragPreviewService.Instance?.HidePreview();
    }
}
