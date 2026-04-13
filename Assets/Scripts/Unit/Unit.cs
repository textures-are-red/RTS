using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : IInventoryHolder, IDisposable
{
    public const sbyte MaxLevel = 5;
    public const sbyte MinLevel = 1;

    public event Action<Node> ArrivedAtNode;
    public event Action LeftNode;

    public event Action<Unit> LevelChanged;

    public sbyte Level { get; private set; }
    public Color UnitColor { get; private set; }
    public Node CurrentNode { get; private set; }
    public Edge CurrentEdge { get; private set; }

    public Inventory Inventory { get; private set; }
    public bool IsTransiting => CurrentEdge is not null && CurrentNode is null;

    private List<LevelToCells> _avaliableCellsOptions;
    private Node _nodeToMove;

    public Unit(Node startNode, Color color, sbyte level, byte cellsCount, List<LevelToCells> avaliableCellsOptions)
    {
        Level = level;
        UnitColor = color;
        CurrentNode = startNode;
        CurrentNode.NodeUnits.LocateUnit(this);

        _avaliableCellsOptions = avaliableCellsOptions;
        Inventory = new(cellsCount, LevelToCells.CalculateInventoryAvaliableCells(Level, _avaliableCellsOptions));
    }

    public void StartMoveTo(Node nodeToMove)
    {
        Edge _edgeToNode = CurrentNode?.NodeConnections.EdgeLeadsToNode(nodeToMove);
        if (_edgeToNode is null) return;

        CurrentNode?.NodeUnits.UnlocateUnit(this);

        LeftNode?.Invoke();

        _edgeToNode.StartUnitTransition(this);
        CurrentNode = null;
        CurrentEdge = _edgeToNode;

        _nodeToMove = nodeToMove;
    }

    public void EndMoveTo()
    {
        CurrentEdge = null;
        CurrentNode = _nodeToMove;
        _nodeToMove = null;

        ArrivedAtNode?.Invoke(CurrentNode);
    }

    public void Upgrade(sbyte levelsToUpgrade = 1)
    {
        Level = (sbyte)((Level + levelsToUpgrade) > MaxLevel ? MaxLevel : Level + levelsToUpgrade);

        LevelChanged?.Invoke(this);
        
        Debug.Log($"new unit lvl: {Level}");
    }

    public void Downgrade(sbyte levelsToDowngrade = 1)
    {
        Level = (sbyte)((Level - levelsToDowngrade) < MinLevel ? MinLevel : Level - levelsToDowngrade);

        LevelChanged?.Invoke(this);

        Debug.Log($"new unit lvl: {Level}");
    }

    public void Dispose()
    {
        CurrentEdge?.StopUnitTransition(this);
        CurrentNode?.NodeUnits.UnlocateUnit(this);
        CurrentNode = null;
        CurrentEdge = null;

        _nodeToMove = null; 

        Inventory = null;
        _avaliableCellsOptions = null;
    }
}
