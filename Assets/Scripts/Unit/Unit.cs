using UnityEngine;

public class Unit
{
    public const sbyte MaxLevel = 5;
    public const sbyte MinLevel = 1;

    public sbyte Level { get; private set; }
    public Color UnitColor { get; private set; }
    public Node CurrentNode { get; private set; }
    public Edge CurrentEdge { get; private set; }

    public bool IsTransiting => CurrentEdge is not null && CurrentNode is null;

    private Node _nodeToMove;

    public Unit(Node startNode, Color color, sbyte level)
    {
        Level = level;
        UnitColor = color;
        CurrentNode = startNode;
        CurrentNode.LocateUnit(this);
    }

    public void StartMoveTo(Node nodeToMove)
    {
        Edge _edgeToNode = CurrentNode?.EdgeLeadsToNode(nodeToMove);
        if (_edgeToNode is null) return;

        CurrentNode?.UnlocateUnit(this);

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
    }

    public void Upgrade(sbyte levelsToUpgrade = 1)
    {
        Level = (sbyte)((Level + levelsToUpgrade) > MaxLevel ? MaxLevel : Level + levelsToUpgrade);
        Debug.Log($"new unit lvl: {Level}");
    }

    public void Downgrade(sbyte levelsToDowngrade = 1)
    {
        Level = (sbyte)((Level - levelsToDowngrade) < MinLevel ? MinLevel : Level - levelsToDowngrade);
        Debug.Log($"new unit lvl: {Level}");
    }

    public void Terminate()
    {
        CurrentEdge?.StopUnitTransition(this);
        CurrentNode?.UnlocateUnit(this);
        CurrentNode = null;
        CurrentEdge = null;

        _nodeToMove = null;
    }
}
