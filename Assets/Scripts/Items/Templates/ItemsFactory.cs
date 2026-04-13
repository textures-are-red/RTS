using System.Collections.Generic;
using UnityEngine;

public static class ItemsFactory
{
    private static Dictionary<string, TemplateItem> _itemsDictionary = new();

    public static void Add(TemplateItem item)
    {
        if (_itemsDictionary.TryAdd(item.Id, item))
            Debug.Log($"element added: id: {item.Id}");
    }

    public static Item CreateItemByID(string id)
    {
        return _itemsDictionary[id].CreateItemBasedOnThisTemplate();
    }
}
