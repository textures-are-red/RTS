using UnityEngine;

public class Unit
{
    public Color UnitColor => _color;
    public Node CurrentNode { get; private set; }
    public Edge CurrentEdge { get; private set; }

    private Color _color;
    private Node _nodeToMove;

    public Unit(Node startNode, Color color)
    {
        _color = color;
        CurrentNode = startNode;
        CurrentNode.LocateUnit(this);
    }

    public void StartMoveTo(Node nodeToMove)
    {
        Edge _edgeToNode = CurrentNode.EdgeLeadsToNode(nodeToMove);
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
}
