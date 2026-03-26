using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Items/Template Resource")]
public class TemplateResource : TemplateItem, IItemMovable, IStackable
{
    [SerializeField, Min(1)] private int _maxStack = 1;

    public int MaxStack
    {
        get => _maxStack;
        private set => _maxStack = value;
    }

    public int CurrentCount { get; private set; } = 1;

    public bool TryStackTo(int countOfNewResurces) //notimplementexception
    {
        int newValue = CurrentCount + countOfNewResurces;

        if (newValue > MaxStack) return false;

        CurrentCount = newValue;
        return true;
    }

    public bool TryUnstackFrom(int countToUnstack)
    {
        int newValue = CurrentCount - countToUnstack;

        if (newValue < 0 || newValue > MaxStack) return false;

        CurrentCount = newValue;
        return true;
    }

    /*public override object Clone()
    {
        return Instantiate(this);
    }*/
}
