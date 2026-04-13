using System;
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

    public override Item CreateItemBasedOnThisTemplate()
    {
        return new Resource(_id, in _name, in _description, _icon, MaxStack, CurrentCount);
    }

    public bool TryStackTo(int countOfNewResurces)
    {
        throw new NotImplementedException();
    }

    public bool TryUnstackFrom(int countToUnstack)
    {
        throw new NotImplementedException();
    }
}
