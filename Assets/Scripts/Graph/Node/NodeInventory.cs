using System.Collections.Generic;
using UnityEngine;

public class NodeInventory : MonoBehaviour, IInventoryHolder
{
    [SerializeField] private NodeLevel _nodeLevel;

    [Space(15)]

    [SerializeField] private byte _inventoryCellsCount = 3;
    [SerializeField] private List<TemplateItem> _startItems;
    [SerializeField] private List<LevelToCells> _avaliableCellsOptions;

    public Inventory Inventory { get; private set;}

    private void Awake()
    {
        Inventory = new(_inventoryCellsCount, LevelToCells.CalculateInventoryAvaliableCells(_nodeLevel.Level, _avaliableCellsOptions));
        _nodeLevel.LevelChanged += OnLevelChanged;

        if (_startItems?.Count is not 0)
            foreach(var item in _startItems)
            {
                Item itemtoAdd = ItemsFactory.CreateItemByID(item.Id);

                if (Inventory.TryAdd(itemtoAdd, addAnyway: true) is false)
                    Debug.LogError("cant add item to inventory");
            }
    }

    private void OnLevelChanged()
    {
        Inventory.UpdateAvailable(LevelToCells.CalculateInventoryAvaliableCells(_nodeLevel.Level, _avaliableCellsOptions));
    }

    private void OnDestroy()
    {
        _nodeLevel.LevelChanged -= OnLevelChanged;  
    }
}
