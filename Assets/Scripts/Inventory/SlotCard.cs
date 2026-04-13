using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotCard : MonoBehaviour, IDropHandler
{
    [SerializeField] private ItemCard _itemCardPrefab;
    [SerializeField] private Image _unavailableMark;

    public Slot Slot 
    {
        get => _slot;
        set
        {
            if (_slot is not null) _slot.SlotChanged -= OnSlotChanged;
            _slot = value;

            if (_slot is not null)
            {
                _slot.SlotChanged += OnSlotChanged;
                _itemCard.Item = _slot.Item;
            }
            
            UpdateInfo();
        }
    }

    public RectTransform RectTransform { get; private set; }

    private Slot _slot;
    private ItemCard _itemCard;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag is not null && eventData.pointerDrag.TryGetComponent<ItemCard>(out ItemCard droppedItemCard))
        {
            droppedItemCard.BecomeOpaque();
            ItemDragPreviewService.Instance?.HidePreview();

            SlotCard sourceSlotCard = droppedItemCard.SlotCard;
            if (sourceSlotCard == this || sourceSlotCard is null) return;

            if ((_slot.Available && droppedItemCard.SlotCard.Slot.Available) is false) return;
            Slot.TrySwapItems(sourceSlotCard.Slot, _slot);
        }
    }

    public void UpdateInfo()
    {
        if (_slot is null)
        {
            HideItem();
            _unavailableMark.enabled = false;
            return;
        }

        if (_itemCard.Item is null)
            HideItem();
        else
        {
            DisplayItem();
            _itemCard.UpdateInfo();
        }
        
        _unavailableMark.enabled = _slot.Available is false;
        (_unavailableMark.transform as RectTransform).SetAsLastSibling();
    }

    private ItemCard CreateItemCard()
    {
        ItemCard newItemCard = Instantiate(_itemCardPrefab, RectTransform);
        newItemCard.Initialize(this);

        (_unavailableMark.transform as RectTransform).SetAsLastSibling();

        return newItemCard;
    }

    private void DisplayItem()
    {
        _itemCard?.gameObject.SetActive(true);
    }

    private void HideItem()
    {
        _itemCard?.gameObject.SetActive(false);
    }

    private void Awake()
    {
        RectTransform = transform as RectTransform;
        _itemCard = CreateItemCard();
        _itemCard.UpdateInfo();
    }

    private void OnSlotChanged(Slot slot)
    {
        _itemCard.Item = slot?.Item;
        UpdateInfo();
    }

    private void OnDestroy()
    {
        if (_slot is not null) _slot.SlotChanged -= OnSlotChanged;
        Destroy(_itemCard);
    }
}
