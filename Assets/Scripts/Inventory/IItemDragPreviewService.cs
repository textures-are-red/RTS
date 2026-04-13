using UnityEngine;

public interface IItemDragPreviewService
{
    void ShowPreview(ItemCard card);
    void UpdatePosition(Vector2 delta);
    void HidePreview();
}
