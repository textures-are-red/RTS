using System;
using UnityEngine;

public abstract class Item
{
    public string Id
    {
        get => _id;
        protected set
        {
            _id = value;
            ItemChanged?.Invoke();
        }
    }

    public string Name
    {
        get => _name;
        protected set
        {
            _name = value;
            ItemChanged?.Invoke();
        }
    }
    public string Description
    {
        get => _description;
        protected set
        {
            _description = value;
            ItemChanged?.Invoke();
        }
    }
    public Sprite Icon
    {
        get => _icon;
        protected set
        {
            _icon = value;
            ItemChanged?.Invoke();
        }
    }

    public event Action ItemChanged;

    protected string _id;
    protected string _name;
    protected string _description;
    protected Sprite _icon;

    public Item(string id, in string name, in string description, Sprite icon)
    {
        _id = id; _name = name; _description = description; _icon = icon;
    }

    protected void ItemChangedInvoke() => ItemChanged?.Invoke();
}
