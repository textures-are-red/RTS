using System;
using UnityEngine;

public abstract class TemplateItem : ScriptableObject
{
    [SerializeField] protected string _id;

    [Space(15)]

    [SerializeField] protected string _name;
    [SerializeField] protected string _description;
    [SerializeField] protected Sprite _icon;

    public string Id => _id;

    public string Name => _name;
    public string Description => _description;
    public Sprite Icon => _icon;

    public abstract Item CreateItemBasedOnThisTemplate();

    private void OnEnable()
    {
        ItemsFactory.Add(this);
    }
}
