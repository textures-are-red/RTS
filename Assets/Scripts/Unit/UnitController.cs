using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitController : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private MouseMoveEnable _mouseMoveEnabler;
 
    [SerializeField] private byte _maxUnitsCount = 10;
    [SerializeField] private sbyte _startLevel = 1;
    [SerializeField] private Node _startNode;
    [SerializeField] private Color _startColor;

    [SerializeField] private List<Color> _colors;

    public IReadOnlyList<Unit> ExistsUnits => _existsUnits;

    private Unit _currentUnit;
    private List<Unit> _existsUnits = new();
    
    private KeyboardDigitGetter _keyboardDigitGetter;
    private InputAction _digitAction;

    private GamepadShoulderInput _gamepadShoulderInput;
    private InputAction _ShoulderAction;

    private void Awake()
    {
        InputSystem _input = InputSystemHolder.Instance;
        _digitAction = _input.Default.Digit;
        _ShoulderAction = _input.Default.UnitSwitch;

        _currentUnit = new Unit(_startNode, _startColor, _startLevel);
        _existsUnits.Add(_currentUnit);

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

        _currentUnit = _existsUnits[newIndex];
    }

    public void ChangeCurrentUnit(byte unitNumber)
    {
        if (unitNumber is 0) unitNumber = 10;
        if (_existsUnits.Count is 0 || unitNumber > _maxUnitsCount) return;

        if (unitNumber <= _existsUnits.Count)
            _currentUnit = _existsUnits[unitNumber - 1];
        else if (unitNumber > _existsUnits.Count)
            _currentUnit = _existsUnits[_existsUnits.Count - 1];
    }

    public void UpgradeCurrentUnit()
    {
        if (_currentUnit.IsTransiting) return;
        UpgradeUnit(_currentUnit, 1);
    }
    public void DowngradeCurrentUnit()
    {
        if (_currentUnit.IsTransiting) return;
        DowngradeUnit(_currentUnit, 1);
    }

    public void CreateUnitOnCurrentUnitNode()
    {
        if (_currentUnit?.CurrentNode is not null)
        {
            CreateUnit(_currentUnit.CurrentNode);
            _currentUnit = _existsUnits[_existsUnits.Count - 1];
        }
    }

    public void DisposeCurrentUnit()
    {
        if (_currentUnit is not null && _existsUnits.Count is not 0)
        {
            DisposeUnit(_currentUnit);
            _currentUnit = _existsUnits.Count is not 0 ? _existsUnits[_existsUnits.Count - 1] : null;
        }
    }

    public void UpgradeUnit(Unit unit, sbyte toUpgrade) => unit?.Upgrade(toUpgrade);
    public void DowngradeUnit(Unit unit, sbyte toDowngrade) => unit?.Downgrade(toDowngrade);

    public void CreateUnit(Node node, sbyte startLevel = Unit.MinLevel)
    {
        if (_colors.Count is 0)
        {
            Debug.LogError($"There are no Colors"); return;
        }
        
        var newUnit = new Unit(node, _colors[0], startLevel);
        _colors.RemoveAt(0);

        _existsUnits.Add(newUnit);

        print($"Unit Count: {_existsUnits.Count}");
    }

    public void DisposeUnit(Unit unit)
    {
        _colors.Add(unit.UnitColor);

        unit.Terminate();
        _existsUnits.Remove(unit);

        print($"Unit Count: {_existsUnits.Count}");
    }

    public void MoveCurrentUnit()
    {
        Node nodeToMove = _cameraController.NodeToCenter ?? _mouseMoveEnabler.ClickedNode;
        if (nodeToMove is not null) _currentUnit.StartMoveTo(nodeToMove);
    }

    private void OnDestroy()
    {
        _keyboardDigitGetter.InputProcessed -= ChangeCurrentUnit;
        _keyboardDigitGetter.Disable();

        _gamepadShoulderInput.InputProcessed -= ChangeCurrentUnitOnTheNearestUnit;
        _gamepadShoulderInput.Disable();
    }
}
