using UnityEngine;

public static class ItemStacker
{
    public static bool TryMerge(Slot sourceSlot, Slot destinationSlot)
    {
        if (sourceSlot is null || destinationSlot is null || sourceSlot.Item is null || sourceSlot.Available is false || destinationSlot.Available is false)
            return false;
        
        if (destinationSlot.Item is null)
        {
            destinationSlot.Item = sourceSlot.Item;
            sourceSlot.Item = null;
            return true;
        }

        Item sourceItem = sourceSlot.Item;
        Item destItem = destinationSlot.Item;

        if (sourceItem.GetType() != destItem.GetType() || sourceItem.Id != destItem.Id || sourceItem is not IStackable sourceStackable ||
            destItem is not IStackable destStackable || sourceStackable.MaxStack != destStackable.MaxStack)
        {
            return false;
        }

        int totalCount = sourceStackable.CurrentCount + destStackable.CurrentCount;

        if (totalCount <= sourceStackable.MaxStack)
        {
            if (destStackable.TryStackTo(sourceStackable.CurrentCount))
            {
                sourceSlot.Item = null;
                return true;
            }
        }
        else
        {
            int neededToFill = sourceStackable.MaxStack - destStackable.CurrentCount;

            if (destStackable.TryStackTo(neededToFill) && sourceStackable.TryUnstackFrom(neededToFill))
            {
                return true;
            }
        }

        return false;
    }
}