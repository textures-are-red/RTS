using UnityEngine;

public class Resource : Item, IItemMovable, IStackable
{
    public int MaxStack { get; protected set; }

    public int CurrentCount { get; protected set; } = 1;

    public Resource(string id, in string name, in string description, Sprite icon, int maxStack, int currentCount)
        : base(id, in name, in description, icon)
    {
        MaxStack = maxStack;
        CurrentCount = currentCount;
    }

    public bool TryStackTo(int countOfNewResurces)
    {
        int newValue = CurrentCount + countOfNewResurces;

        if (newValue > MaxStack) return false;

        CurrentCount = newValue;
        ItemChangedInvoke();
        return true;
    }

    public bool TryUnstackFrom(int countToUnstack)
    {
        int newValue = CurrentCount - countToUnstack;

        if (newValue < 0 || newValue > MaxStack) return false;

        CurrentCount = newValue;
        ItemChangedInvoke();
        return true;
    }
}
