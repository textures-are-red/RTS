using System.Collections.Generic;
using UnityEngine;

public class NodeConnections : MonoBehaviour
{
    [SerializeField] private Edge _edgePrefab;
    [SerializeField] private Node _thisNode;

    [Space(15)]

    [SerializeField] private List<Node> _neighbors;

    public IReadOnlyList<Edge> Edges => _edges;
    public Node ThisNode => _thisNode;

    private List<Edge> _edges = new();
    private NodeUnits _nodeUnit;
    //private Node _thisNode;

    public void Initialize()
    {
        _nodeUnit = _thisNode.NodeUnits;
        SpawnEdges();
    }

    public bool HasEdgeTo(Node to)
    {
        foreach (var edge in _edges)
            if (edge.To == to) return true;

        return false;
    }

    public Edge EdgeLeadsToNode(Node to)
    {
        foreach (var edge in _edges)
            if (edge.To == to) return edge;

        return null;
    }

    private void SpawnEdges()
    {
        foreach (var neighbor in _neighbors)
        {
            if (HasEdgeTo(neighbor)) continue;

            var newEdge = Instantiate(_edgePrefab);
            _edges.Add(newEdge);

            newEdge.LocateBetweenNodes(_thisNode, neighbor);

            newEdge.OnTransitionEnd += _nodeUnit.UnlocateUnit;
            newEdge.OnTransitionEnd += neighbor.NodeUnits.LocateUnit;
        }
    }

    private void DestroyEdges()
    {
        foreach (var edge in _edges)
        {
            if (edge is not null)
            {
                edge.OnTransitionEnd -= _nodeUnit.UnlocateUnit;

                if (edge.To is not null)
                    edge.OnTransitionEnd -= edge.To.NodeUnits.LocateUnit;
            }
        }
    }

    private void OnDestroy()
    {
        DestroyEdges();
    }
}
