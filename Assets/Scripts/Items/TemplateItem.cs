using System;
using UnityEngine;

public abstract class TemplateItem : ScriptableObject//, ICloneable
{
    [SerializeField] private string _id;

    [Space(15)]

    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;

    public string Id => _id;

    public string Name => _name;
    public string Description => _description;
    public Sprite Icon => _icon;

    //public abstract object Clone();

    private void OnEnable()
    {
        ItemsFactory.Add(this);
    }
}
