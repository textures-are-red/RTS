using System;

public class Slot
{
    public event Action<Slot> SlotChanged;

    public Item Item
    {
        get => _item;
        set
        {
            _item = value;
            SlotChanged?.Invoke(this);
        }
    }

    public bool Available
    {
        get => _available;
        set
        {
            _available = value;
            SlotChanged?.Invoke(this);
        }
    }

    private Item _item;
    private bool _available = true;

    public static bool TrySwapItems(Slot first, Slot second, bool swapAnyway = false)
    {
        if (first is null || second is null || first == second) return false;
        if (first.Available is false || second.Available is false)
        {
            if (swapAnyway is false) return false;
        }

        Item sourceItem = first.Item;
        Item destinationItem = second.Item;
        
        if (ItemStacker.TryMerge(first, second))
            return true;

        (first.Item, second.Item) = (second.Item, first.Item);
        return true;
    }
}
