using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDisplay : MonoBehaviour
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

    public Item Item
    {
        get => _item;
        set
        {
            if (_item is not null) _item.ItemChanged -= UpdateInfo;
            _item = value;
            if (_item is not null) _item.ItemChanged += UpdateInfo;
            
            UpdateInfo();
        }
    }

    public RectTransform RectTransform { get; private set; }

    public CanvasGroup CanvasGroup => _canvasGroup;

    private Item _item;

    private void Awake()
    {
        RectTransform = transform as RectTransform;
    }

    public void SetAsPreview()
    {
        if (_canvasGroup is not null)
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
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

    private void OnDisable()
    {
        if (_item is not null) _item.ItemChanged -= UpdateInfo;
    }
}
