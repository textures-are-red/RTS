using System.Collections.Generic;
using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    [SerializeField] private InventoryDisplay _unitInventoryDisplay;
    [SerializeField] private InventoryDisplay _nodeInventoryDisplay;

    [Space(15)]

    private List<LevelToCells> _avaliableCellsOptions;

    private CurrentUnitSelector _selector;
    private UnitManager _unitManager;

    public void Initialize(CurrentUnitSelector selector, List<LevelToCells> avaliableCellsOptions, List<TemplateItem> startItems)
    {
        _selector = selector;
        _avaliableCellsOptions = avaliableCellsOptions;
        _selector.CurrentUnitChanged += OnCurrentUnitChanged;

        Inventory inventory = _selector.CurrentUnit.Inventory;

        if (startItems?.Count is not 0)
            foreach(var item in startItems)
            {
                Item itemtoAdd = ItemsFactory.CreateItemByID(item.Id);

                if (inventory.TryAdd(itemtoAdd, addAnyway: true) is false)
                    Debug.LogError("cant add item to inventory");
            }

        if (_selector.CurrentUnit is not null)
            OnCurrentUnitChanged(null, _selector.CurrentUnit);
    }

    private void OnCurrentUnitChanged(Unit oldUnit, Unit newUnit)
    {
        if (oldUnit is not null)
        {
            oldUnit.ArrivedAtNode -= OnCurrentUnitArrivedAtNode;
            oldUnit.LeftNode -= OnCurrentUnitLeftNode;
            oldUnit.LevelChanged -= OnCurrentUnitLevelChanged;
        }
        
        if (newUnit is not null)
        {
            newUnit.ArrivedAtNode += OnCurrentUnitArrivedAtNode;
            newUnit.LeftNode += OnCurrentUnitLeftNode;
            newUnit.LevelChanged += OnCurrentUnitLevelChanged;

            _unitInventoryDisplay.Inventory = newUnit.Inventory;
            OnCurrentUnitArrivedAtNode(newUnit.CurrentNode);
        }
        else
        {
            _unitInventoryDisplay.Inventory = null;
            _nodeInventoryDisplay.Inventory = null;
        }
    }

    private void OnCurrentUnitArrivedAtNode(Node newNode)
    {
        _nodeInventoryDisplay.Inventory = newNode?.NodeInventory.Inventory;
    }

    private void OnCurrentUnitLeftNode()
    {
        _nodeInventoryDisplay.Inventory = null;
    }

    private void OnCurrentUnitLevelChanged(Unit unit)
    {
        unit.Inventory.UpdateAvailable(LevelToCells.CalculateInventoryAvaliableCells(unit.Level, _avaliableCellsOptions));
        
        _unitInventoryDisplay.UpdateSlotsInfo();
    }

    private void OnDestroy()
    {
        _selector.CurrentUnitChanged -= OnCurrentUnitChanged;
        _selector = null;
    }
}
