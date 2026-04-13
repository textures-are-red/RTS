using UnityEngine;
using UnityEngine.UI;

public class ItemDragPreviewService : MonoBehaviour
{
    [SerializeField] private ItemCard _itemCardDragPreviewPrefab;
    [SerializeField] private Canvas _canvas;

    public static ItemDragPreviewService Instance { get; private set; }

    public bool IsShown => _preview.gameObject.activeSelf;

    private ItemCard _preview;

    private void Awake()
    {
        if (Instance is not null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (_itemCardDragPreviewPrefab is not null)
        {
            _preview = Instantiate(_itemCardDragPreviewPrefab);
            _preview.name = "Preview";
            _preview.SetAsPreview(_canvas);
            _preview.gameObject.SetActive(false);
        }
    }

    public void ShowPreview(ItemDisplay display)
    {
        if (_preview is null || display is null) return;

        _preview.Item = display.Item;

        _preview.RectTransform.SetParent(_canvas.transform, true);
        _preview.RectTransform.position = display.RectTransform.position;

        _preview.gameObject.SetActive(true);
    }

    public void UpdatePosition(Vector2 delta)
    {
        if (_preview is null) return;
        _preview.RectTransform.anchoredPosition += delta;
    }

    public void HidePreview()
    {
        _preview?.gameObject.SetActive(false);
    }
}
