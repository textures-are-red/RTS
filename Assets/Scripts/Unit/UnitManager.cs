using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private byte _maxUnitsCount = 10;

    [SerializeField] private List<LevelToCells> _avaliableCellsOptions;

    [Space(15)]

    [SerializeField] private List<Color> _colors;

    public event Action<Unit> UnitCreated;
    public event Action<Unit> UnitDisposed;

    public ReadOnlyObservableCollection<Unit> ExistsUnits { get; private set; }
    public byte MaxUnitsCount => _maxUnitsCount;

    public List<LevelToCells> AvaliableCellsOptions => _avaliableCellsOptions;

    private ObservableCollection<Unit> _existsUnits = new();
    
    public Unit CreateUnit(Node node, byte inventoryCellsCount, sbyte startLevel = Unit.MinLevel)
    {
        if (_colors.Count is 0)
        {
            Debug.LogError($"There are no Colors"); return null;
        }
        
        Unit newUnit = new Unit(node, _colors[0], startLevel, inventoryCellsCount, _avaliableCellsOptions);
        _colors.RemoveAt(0);

        _existsUnits.Add(newUnit);

        UnitCreated?.Invoke(newUnit);
        return newUnit;
    }

    public void DisposeUnit(Unit unit)
    {
        _colors.Add(unit.UnitColor);

        unit.Dispose();
        _existsUnits.Remove(unit);

        UnitDisposed?.Invoke(unit);
    }

    private void Awake()
    {
        ExistsUnits = new(_existsUnits);
    }
}
