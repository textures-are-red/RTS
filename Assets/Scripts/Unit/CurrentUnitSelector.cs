using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CurrentUnitSelector : IDisposable
{
    public event Action<int, sbyte> LevelChanged;
    public event Action<Unit, Unit> CurrentUnitChanged;

    public Unit CurrentUnit { get; private set; }

    private UnitManager _unitManager;
    
    private KeyboardDigitGetter _keyboardDigitGetter;
    private InputAction _digitAction;

    private GamepadShoulderInput _gamepadShoulderInput;
    private InputAction _ShoulderAction;

    public CurrentUnitSelector(UnitManager unitManager, Unit unit)
    {
        _unitManager = unitManager;

        InputSystem _input = InputSystemHolder.Instance;
        _digitAction = _input.Default.Digit;
        _ShoulderAction = _input.Default.UnitSwitch;
        
        SetCurrentUnit(unit);

        _keyboardDigitGetter = new(_digitAction);
        _keyboardDigitGetter.Enable();
        _keyboardDigitGetter.InputProcessed += ChangeCurrentUnit;

        _gamepadShoulderInput = new(_ShoulderAction);
        _gamepadShoulderInput.Enable();
        _gamepadShoulderInput.InputProcessed += ChangeCurrentUnitOnTheNearestUnit;
    }

    public void ChangeCurrentUnitOnTheNearestUnit(sbyte moveStep)
    {
        var existsUnits = _unitManager.ExistsUnits;
        if (existsUnits.Count is 0) return;

        int newIndex = existsUnits.IndexOf(CurrentUnit) + moveStep;

        if (newIndex < 0) newIndex = existsUnits.Count - 1;
        else if (newIndex > existsUnits.Count - 1) newIndex = 0;

        SetCurrentUnit(existsUnits[newIndex]);
    }

    public void ChangeCurrentUnit(byte unitNumber)
    {
        var existsUnits = _unitManager.ExistsUnits;

        if (unitNumber is 0) unitNumber = 10;
        if (existsUnits.Count is 0 || unitNumber > _unitManager.MaxUnitsCount) return;

        SetCurrentUnit(unitNumber <= existsUnits.Count ? existsUnits[unitNumber - 1] : existsUnits[existsUnits.Count - 1]);
    }

    public void Dispose()
    {
        _keyboardDigitGetter.InputProcessed -= ChangeCurrentUnit;
        _keyboardDigitGetter.Disable();

        _gamepadShoulderInput.InputProcessed -= ChangeCurrentUnitOnTheNearestUnit;
        _gamepadShoulderInput.Disable();

        LevelChanged = null;
        CurrentUnitChanged = null;
    }

    public void SetCurrentUnit(Unit newUnit)
    {
        if (CurrentUnit is not null)
            CurrentUnit.LevelChanged -= OnCurrentUnitLevelChanged;
        
        Unit oldUnit = CurrentUnit;
        CurrentUnit = newUnit;

        if (CurrentUnit is not null)
            CurrentUnit.LevelChanged += OnCurrentUnitLevelChanged;
        
        CurrentUnitChanged?.Invoke(oldUnit, CurrentUnit);
    }

    private void OnCurrentUnitLevelChanged(Unit unit) => LevelChanged?.Invoke(_unitManager.ExistsUnits.IndexOf(unit), unit.Level);
}
