using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitController : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private MouseMoveEnable _mouseMoveEnabler;

    [Space(15)]
 
    [SerializeField] private byte _maxUnitsCount = 10;
    [SerializeField, Range(Unit.MinLevel, Unit.MaxLevel)] private sbyte _startLevel = 1;
    [SerializeField] private Node _startNode;

    [Space(15)]

    [SerializeField] private InventoryDisplay _unitInventoryDisplay; //вынести в отдельный класс
    [SerializeField] private InventoryDisplay _nodeInventoryDisplay;
    [SerializeField] private byte _inventoryCellsCount = 3;
    [SerializeField] private List<LevelToCells> _avaliableCellsOptions;
    [SerializeField] private List<Item> _startItems;

    [Space(15)]

    [SerializeField] private Color _startColor;
    [SerializeField] private List<Color> _colors;

    public event Action<int, sbyte> LevelChanged;

    public ReadOnlyObservableCollection<Unit> ExistsUnits { get; private set; }

    private Unit _currentUnit;
    private ObservableCollection<Unit> _existsUnits = new();
    
    private KeyboardDigitGetter _keyboardDigitGetter;
    private InputAction _digitAction;

    private GamepadShoulderInput _gamepadShoulderInput;
    private InputAction _ShoulderAction;

    private void OnValidate()
    {
        if (_startItems?.Count > _inventoryCellsCount)
            _startItems.RemoveRange(3, _startItems.Count - 3);
    }

    private void Awake()
    {
        InputSystem _input = InputSystemHolder.Instance;
        _digitAction = _input.Default.Digit;
        _ShoulderAction = _input.Default.UnitSwitch;

        _currentUnit = new Unit(_startNode, _startColor, _startLevel, _inventoryCellsCount, _avaliableCellsOptions);
        ExistsUnits = new(_existsUnits);
        _existsUnits.Add(_currentUnit);
        SetCurrentUnit(_currentUnit);

        Inventory inventory = _currentUnit.Inventory;

        foreach (var item in _startItems)
            inventory.TryAdd(item);

        _unitInventoryDisplay.Inventory = _currentUnit.Inventory;

        _keyboardDigitGetter = new(_digitAction);
        _keyboardDigitGetter.Enable();
        _keyboardDigitGetter.InputProcessed += ChangeCurrentUnit;

        _gamepadShoulderInput = new(_ShoulderAction);
        _gamepadShoulderInput.Enable();
        _gamepadShoulderInput.InputProcessed += ChangeCurrentUnitOnTheNearestUnit;
    }

    public void ChangeCurrentUnitOnTheNearestUnit(sbyte moveStep)
    {
        if (_existsUnits.Count is 0) return;

        int newIndex = _existsUnits.IndexOf(_currentUnit) + moveStep;

        if (newIndex < 0) newIndex = _existsUnits.Count - 1;
        else if (newIndex > _existsUnits.Count - 1) newIndex = 0;

        SetCurrentUnit(_existsUnits[newIndex]);
    }

    public void ChangeCurrentUnit(byte unitNumber)
    {
        if (unitNumber is 0) unitNumber = 10;
        if (_existsUnits.Count is 0 || unitNumber > _maxUnitsCount) return;

        SetCurrentUnit(unitNumber <= _existsUnits.Count ? _existsUnits[unitNumber - 1] : _existsUnits[_existsUnits.Count - 1]);
    }

    public void UpgradeCurrentUnit()
    {
        if (_currentUnit is null || _currentUnit.IsTransiting) return;
        UpgradeUnit(_currentUnit, 1);
    }
    public void DowngradeCurrentUnit()
    {
        if (_currentUnit is null || _currentUnit.IsTransiting) return;
        DowngradeUnit(_currentUnit, 1);
    }

    public void CreateUnitOnCurrentUnitNode()
    {
        if (_currentUnit?.CurrentNode is not null)
        {
            CreateUnit(_currentUnit.CurrentNode);
            SetCurrentUnit(_existsUnits[_existsUnits.Count - 1]);
        }
    }

    public void DisposeCurrentUnit()
    {
        if (_currentUnit is not null && _existsUnits.Count is not 0)
        {
            DisposeUnit(_currentUnit);
            SetCurrentUnit(_existsUnits.Count is not 0 ? _existsUnits[_existsUnits.Count - 1] : null);
        }
    }

    public void UpgradeUnit(Unit unit, sbyte toUpgrade)
    {
        unit?.Upgrade(toUpgrade);
        LevelChanged?.Invoke(_existsUnits.IndexOf(unit), unit.Level);
    }
    public void DowngradeUnit(Unit unit, sbyte toDowngrade)
    {
        unit?.Downgrade(toDowngrade);
        LevelChanged?.Invoke(_existsUnits.IndexOf(unit), unit.Level);
    }

    public void CreateUnit(Node node, sbyte startLevel = Unit.MinLevel)
    {
        if (_colors.Count is 0)
        {
            Debug.LogError($"There are no Colors"); return;
        }
        
        var newUnit = new Unit(node, _colors[0], startLevel, _inventoryCellsCount, _avaliableCellsOptions);
        _colors.RemoveAt(0);

        _existsUnits.Add(newUnit);
    }

    public void DisposeUnit(Unit unit)
    {
        _colors.Add(unit.UnitColor);

        unit.Dispose();
        _existsUnits.Remove(unit);

        print($"Unit Count: {_existsUnits.Count}");
    }

    public void MoveCurrentUnit()
    {
        Node nodeToMove = _cameraController.NodeToCenter ?? _mouseMoveEnabler.ClickedNode;
        if (nodeToMove is not null) _currentUnit.StartMoveTo(nodeToMove);
    }

    private void SetCurrentUnit(Unit newUnit)
    {
        if (_currentUnit is not null)
        {
            _currentUnit.ArrivedAtNode -= OnCurrentUnitArrivedAtNode;
            _currentUnit.LeftNode -= OnCurrentUnitLeftNode;
            _currentUnit.LevelChanged -= OnCurrentUnitLevelChanged;
        }
        
        _currentUnit = newUnit;

        if (_currentUnit is not null)
        {
            _currentUnit.ArrivedAtNode += OnCurrentUnitArrivedAtNode;
            _currentUnit.LeftNode += OnCurrentUnitLeftNode;
            _currentUnit.LevelChanged += OnCurrentUnitLevelChanged;

            _unitInventoryDisplay.Inventory = _currentUnit.Inventory;
            OnCurrentUnitArrivedAtNode(_currentUnit.CurrentNode);
        }
        else
        {
            _unitInventoryDisplay.Inventory = null;
            _nodeInventoryDisplay.Inventory = null;
        }
    }

    private void OnCurrentUnitLeftNode()
    {
        _nodeInventoryDisplay.Inventory = null;
    }

    private void OnCurrentUnitArrivedAtNode(Node newNode)
    {
        _nodeInventoryDisplay.Inventory = newNode?.Inventory;
    }

    private void OnCurrentUnitLevelChanged(Unit unit)
    {
        unit.Inventory.UpdateAvailable(LevelToCells.CalculateInventoryAvaliableCells(unit.Level, _avaliableCellsOptions));
        
        _unitInventoryDisplay.UpdateSlotsInfo();
    }

    private void OnDestroy()
    {
        _keyboardDigitGetter.InputProcessed -= ChangeCurrentUnit;
        _keyboardDigitGetter.Disable();

        _gamepadShoulderInput.InputProcessed -= ChangeCurrentUnitOnTheNearestUnit;
        _gamepadShoulderInput.Disable();
    }
}
