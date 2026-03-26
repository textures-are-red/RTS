using UnityEngine;
using UnityEngine.UI;

public class ItemDragPreviewService : MonoBehaviour
{
    [SerializeField] private ItemCard _itemCardDragPreviewPrefab;
    [SerializeField] private Canvas _canvas;

    public static ItemDragPreviewService Instance { get; private set; }

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
            _preview.SetAsPreview();
            _preview.ItemDragPreviewService = this;
            _preview.Canvas = _canvas;
            _preview.gameObject.SetActive(false);
        }
    }

    public void ShowPreview(ItemCard source)
    {
        if (_preview is null || source is null) return;

        _preview.Item = source.Item;

        _preview.RectTransform.SetParent(_canvas.transform, true);
        _preview.RectTransform.position = source.RectTransform.position;

        _preview.gameObject.SetActive(true);
    }

    public void UpdatePosition(Vector2 delta)
    {
        if (_preview is null) return;
        _preview.RectTransform.anchoredPosition += delta;
    }

    public void HidePreview()
    {
        if (_preview is not null)
            _preview.gameObject.SetActive(false);
    }
}
