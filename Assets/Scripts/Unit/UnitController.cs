using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitController : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;

    [Space(15)]

    [SerializeField] private UnitManager _unitManager;
    [SerializeField] private UnitActionController _unitActionController;
    [SerializeField] private InventoryUIController _inventoryUIController;

    [Space(15)]
 
    [SerializeField, Range(Unit.MinLevel, Unit.MaxLevel)] private sbyte _startLevel = 1;
    [SerializeField] private Node _startNode;

    [Space(15)]

    [SerializeField] private byte _inventoryCellsCount = 3;
    [SerializeField] private List<TemplateItem> _startItems;

    public CurrentUnitSelector Selector { get; private set; }
    public ReadOnlyObservableCollection<Unit> ExistsUnits => _unitManager.ExistsUnits;

    private void OnValidate()
    {
        if (_startItems?.Count > _inventoryCellsCount)
            _startItems.RemoveRange(3, _startItems.Count - 3);
    }

    private void Awake()
    {
        Unit firstUnit = _unitManager.CreateUnit(_startNode, _inventoryCellsCount, _startLevel);
        Selector = new(_unitManager, firstUnit);
        _unitActionController.Initialize(Selector, _inventoryCellsCount);
        _inventoryUIController.Initialize(Selector, _unitManager.AvaliableCellsOptions, _startItems);
    }

    //public void UpgradeUnit(Unit unit, sbyte toUpgrade) => unit?.Upgrade(toUpgrade);
    //public void DowngradeUnit(Unit unit, sbyte toDowngrade) => unit?.Downgrade(toDowngrade);

    private void OnDestroy()
    {
        Selector.Dispose();
    }
}
