using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private CanvasGroup _canvasGroup;

    [Space(15)]

    [SerializeField] private Image _itemBackground;
    
    [Space(15)]

    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _stackCount;

    [Space(15)]

    [SerializeField] private Image _infoBackground;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _description;

    public Canvas Canvas { private get; set; }
    public bool IsPreview { get; private set; }

    public SlotCard SlotCard
    {
        get => _slotCard;
        set
        {
            _slotCard = value;
            if (_slotCard is not null)
                _slotCardRectTransform = _slotCard.transform as RectTransform;
        }
    }

    public Item Item
    {
        get => _item;
        set
        {
            _item = value;
            UpdateInfo();
        }
    }

    public Image ItemBackground => _itemBackground;
    
    public Image ItemImage => _itemImage;
    public TextMeshProUGUI StackCount => _stackCount;

    public Image InfoBackground => _infoBackground;
    public TextMeshProUGUI Name => _name;
    public TextMeshProUGUI Description => _description;

    public ItemDragPreviewService ItemDragPreviewService { private get; set; }
    public RectTransform RectTransform { get; private set; }

    private SlotCard _slotCard;
    private Item _item;

    private RectTransform _slotCardRectTransform;

    public void SetAsPreview()
    {
        IsPreview = true;
        _slotCard = null;
        _slotCardRectTransform = null;

        if (_canvasGroup is not null)
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
    }

    public void ResetAfterDrag()
    {
        if (IsPreview) return;

        if (_canvasGroup is not null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        ItemDragPreviewService?.HidePreview();
    }

    public void UpdateInfo()
    {
        if (_item is not null)
        {
            _itemBackground.enabled = true;
            _itemImage.enabled = true;
            _itemImage.sprite = Item.Icon;

            _name.text = Item.Name;
            _description.text = Item.Description;
            _infoBackground.enabled = true;

            if (_item is IStackable stackable)
            {
                _stackCount.text = stackable.CurrentCount.ToString();
            }
            else
            {
                _stackCount.text = string.Empty;
            }
        }
        else
        {
            _itemBackground.enabled = false;
            _itemImage.enabled = false;
            _itemImage.sprite = null;

            _name.text = string.Empty;
            _description.text = string.Empty;
            _infoBackground.enabled = false;

            _stackCount.text = string.Empty;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsPreview) return;

        _canvasGroup.alpha = 0.6f;
        _canvasGroup.blocksRaycasts = false;

        ItemDragPreviewService.ShowPreview(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsPreview) return;
        ItemDragPreviewService?.UpdatePosition(eventData.delta / (Canvas is null ? 1f : Canvas.scaleFactor));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsPreview) return;

        ResetAfterDrag();
    }

    private void Awake()
    {
        RectTransform = transform as RectTransform;
    }
}
