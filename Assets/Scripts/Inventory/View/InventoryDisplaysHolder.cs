using System.Linq;
using UnityEngine;

public class InventoryDisplaysHolder : MonoBehaviour
{
    public static InventoryDisplaysHolder Instance => _instance;
    private static InventoryDisplaysHolder _instance;

    [SerializeField] private InventoryDisplay _unitInventoryDisplay;
    [SerializeField] private InventoryDisplay _nodeInventoryDisplay;

    public InventoryDisplay UnitInventoryDisplay => _unitInventoryDisplay;
    public InventoryDisplay NodeInventoryDisplay => _nodeInventoryDisplay;

    private void Awake()
    {
        if (_instance is not null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public void TransferItemBetweenDisplays(Item item, Inventory sourceInventory = null)
    {
        Inventory targetInventory;

        if (sourceInventory is null)
        {
            bool isUnitInventory = UnitInventoryDisplay.Inventory.HasItem(item);
            bool isNodeInventory = NodeInventoryDisplay.Inventory.HasItem(item);

            if (isUnitInventory == isNodeInventory) return;

            sourceInventory = isUnitInventory ? UnitInventoryDisplay.Inventory : NodeInventoryDisplay.Inventory;
            targetInventory = isUnitInventory ? NodeInventoryDisplay.Inventory : UnitInventoryDisplay.Inventory;
        }
        else
            targetInventory = (sourceInventory == UnitInventoryDisplay.Inventory) ? NodeInventoryDisplay.Inventory : UnitInventoryDisplay.Inventory;

        if (item is not IStackable && targetInventory.TryAdd(item))
            sourceInventory.Remove(item);
        else if (item is IStackable stackable)
        {
            Slot slotOfCurrentItem = sourceInventory.Slots.FirstOrDefault(s => s.Item == item);
            if (slotOfCurrentItem.Item is not null)
            {
                if (targetInventory.TryStack(stackable, slotOfCurrentItem))
                    sourceInventory.Remove(item);
            }
        }
    }
}
