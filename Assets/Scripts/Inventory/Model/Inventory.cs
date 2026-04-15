using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public int SlotsCount => _slots.Count;
    public IReadOnlyList<Slot> Slots => _slots;

    private List<Slot> _slots;

    public Inventory(byte cellsCount, byte availableSlotsCount)
    {
        _slots = new(cellsCount);

        for (int i = 0; i < cellsCount; ++i)
            _slots.Add(new Slot());

        UpdateAvailable(availableSlotsCount);
    }

    public bool TryAdd(Item item, bool addAnyway = false)
    {
        Slot freeSlot = _slots.Find(s => s.Item is null && (s.Available || s.Available is false && addAnyway));

        if (freeSlot is null) return false;

        freeSlot.Item = item;
        return true;
    }

    public bool TryAdd(Item item, Slot slot, bool addAnyway = false)
    {
        if (_slots.Contains(slot) is false || (slot.Available || slot.Available is false && addAnyway) is false) return false;

        slot.Item = item;
        return true;
    }

    public bool TryStack(IStackable sourceStackable, Slot sourceSlot)
    {
        foreach (var slot in _slots)
        {
            if (ItemStacker.TryMerge(sourceSlot, slot))
            {
                if (sourceStackable is null)
                    return true;
            }
        }

        return false;
    }

    public void Remove(Item item, bool removeAnyway = false)
    {
        Slot slotWithItem = _slots.Find(s => s.Item == item);

        if (slotWithItem is null || (slotWithItem.Available || slotWithItem.Available is false && removeAnyway) is false) return;

        slotWithItem.Item = null;
    }

    public void Remove(Slot slot, bool removeAnyway = false)
    {
        bool slotExists = _slots.Contains(slot);

        if (slotExists is false || (slot.Available || slot.Available is false && removeAnyway) is false) return;

        slot.Item = null;
    }

    public bool HasItem(Item item)
    {
        if (item is null) return false;
        return _slots.Exists(slot => slot.Item == item);
    }

    public void UpdateAvailable(int newAvailableCount)
    {
        if (newAvailableCount > _slots.Count) newAvailableCount = _slots.Count;

        for (int i = 0; i < _slots.Count; ++i)
            _slots[i].Available = i < newAvailableCount;
    }
}
