using UnityEngine;

public static class ItemStacker
{
    public static bool TryMerge(Slot sourceSlot, Slot destinationSlot)
    {
        if (sourceSlot?.Item is null || destinationSlot?.Item is null)
            return false;

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