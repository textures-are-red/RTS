using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Node : MonoBehaviour, IInitializable, IClickable, IInventoryHolder
{
    private const sbyte _levelIndicatorsCount = 4;
    private const float _levelIndicatorsRotation = 90f;

    [SerializeField] private sbyte _maxLevel = 5;
    [SerializeField] private sbyte _minLevel = 0;
    [SerializeField] private sbyte _level = 1;

    [Space(15)]

    [SerializeField] private byte _inventoryCellsCount = 3;
    [SerializeField] private List<Item> _startItems;
    [SerializeField] private List<LevelToCells> _avaliableCellsOptions;

    [Space(15)]

    [SerializeField] private Edge _edgePrefab;
    [SerializeField] private TextMeshPro _levelIndicatorPrefab;
    [SerializeField] private Vector3 _levelIndicatorMargin;

    [Space(15)]

    [SerializeField] private Color _multipleUnitsLocatedColor;
    [SerializeField] private List<Node> _neighbors;

    [SerializeField] private UnityEvent<Transform> _onClick;

    public bool IsInitialized { get; private set; }
    public bool HasUnits => _locatedUnits.Count is not 0;

    public Inventory Inventory { get; private set;}

    public event Action LevelChanged;
    public sbyte Level => _level;

    public IReadOnlyList<Edge> Edges => _edges;
    public IReadOnlyList<Unit> LocatedUnits => _locatedUnits;

    private List<TextMeshPro> _levelIndicators = new();
    private List<Edge> _edges = new();
    private List<Unit> _locatedUnits = new();

    private Material _material;
    private Color _defaultColor;
    private Lightener _lightener;

    private void OnValidate()
    {
        Mathf.Clamp(_level, _minLevel, _maxLevel);
    }

    public void Initialize()
    {
        if (IsInitialized) return;

        _material = GetComponent<Renderer>().material;
        _lightener = GetComponent<Lightener>();

        _defaultColor = _material.color;

        Inventory = new(_inventoryCellsCount, LevelToCells.CalculateInventoryAvaliableCells(_level, _avaliableCellsOptions));
        LevelChanged += OnLevelChanged;

        if (_startItems?.Count is not 0)
            foreach(var item in _startItems)
            {
                Item itemtoAdd = ItemsFactory.GetItemByID(item.Id) as Item;

                if (Inventory.TryAdd(itemtoAdd, addAnyway: true) is false)
                    Debug.LogError("cant add item to inventory");
            }

        //Graph.AddNode(this);

        SpawnEdges();
        SpawnLevelIndicator();

        IsInitialized = true;
    }

    public void OnClick()
    {
        _onClick?.Invoke(transform);
    }

    public void LocateUnit(Unit unit)
    {
        _locatedUnits.Add(unit);

        Color newColor = _locatedUnits.Count > 1 ? _multipleUnitsLocatedColor : unit.UnitColor;

        _lightener.UpdateOriginalColor(newColor);
        _material.color = newColor;
    }

    public void UnlocateUnit(Unit unit)
    {
        _locatedUnits.Remove(unit);

        Color newColor = _locatedUnits.Count switch
        {
            > 1 => _multipleUnitsLocatedColor,
            1   => _locatedUnits[0].UnitColor,
            _   => _defaultColor
        };

        _lightener.UpdateOriginalColor(newColor);
        _material.color = newColor;
    }
    
    public void Upgrade(sbyte levelsToUpgrade = 1)
    {
        _level = (sbyte)((_level + levelsToUpgrade) > _maxLevel ? _maxLevel : _level + levelsToUpgrade);

        foreach (var indicator in _levelIndicators)
            indicator.text = _level.ToString();
        
        LevelChanged?.Invoke();
    }

    public void Downgrade(sbyte levelsToDowngrade = 1)
    {
        _level = (sbyte)((_level - levelsToDowngrade) < _minLevel ? _minLevel : _level - levelsToDowngrade);
        
        foreach (var indicator in _levelIndicators)
            indicator.text = _level.ToString();

        LevelChanged?.Invoke();
    }

    public Edge EdgeLeadsToNode(Node to)
    {
        foreach (var edge in _edges)
            if (edge.To == to) return edge;

        return null;
    }

    public void ChangeColor(Color newColor) => _lightener.UpdateOriginalColor(newColor);

    public bool HasEdgeTo(Node to)
    {
        foreach (var edge in _edges)
            if (edge.To == to) return true;

        return false;
    }

    private void SpawnEdges()
    {
        foreach (var neighbor in _neighbors)
        {
            if (HasEdgeTo(neighbor)) continue;

            var newEdge = Instantiate(_edgePrefab);
            _edges.Add(newEdge);

            newEdge.LocateBetweenNodes(this, neighbor);

            newEdge.OnTransitionEnd += UnlocateUnit;
            newEdge.OnTransitionEnd += neighbor.LocateUnit;
        }
    }

    private void SpawnLevelIndicator()
    {
        Vector3? rotationPointer = null;

        for (sbyte i = 0; i < _levelIndicatorsCount; ++i)
        {
            var indicator = Instantiate(_levelIndicatorPrefab, transform);
            indicator.text = _level.ToString();
            _levelIndicators.Add(indicator);

            Transform indicatorTransform = indicator.transform;
            if (rotationPointer.HasValue is false) rotationPointer = indicatorTransform.eulerAngles;

            indicatorTransform.eulerAngles = rotationPointer.GetValueOrDefault();

            Vector3 scale = indicatorTransform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            indicatorTransform.localScale = scale;

            indicatorTransform.Translate(_levelIndicatorMargin, Space.Self);

            Vector3 rotationPointerValue = rotationPointer.GetValueOrDefault();
            rotationPointerValue.y += _levelIndicatorsRotation;
            rotationPointer = rotationPointerValue;
        }
    }

    private void OnLevelChanged()
    {
        Inventory.UpdateAvailable(LevelToCells.CalculateInventoryAvaliableCells(_level, _avaliableCellsOptions));
    }

    private void OnDestroy()
    {
        foreach (var indicator in _levelIndicators)
        {
            if (indicator is not null)
                Destroy(indicator.gameObject);
        }

        _levelIndicators.Clear();

        foreach (var edge in _edges)
        {
            if (edge is not null)
            {
                edge.OnTransitionEnd -= UnlocateUnit;

                if (edge.To is not null)
                    edge.OnTransitionEnd -= edge.To.LocateUnit;
            }
        }

        LevelChanged -= OnLevelChanged;

        Inventory = null;
    }
}
