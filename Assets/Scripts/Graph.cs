using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-99)]
public class Graph : MonoBehaviour
{
    public static Graph Instance { get; private set; }
        
    private List<Node> _nodes = new();

    private void Awake()
    {
        if (Instance is not null && Instance != this)
        {
            Destroy(this); return;
        }

        Instance = this;
        
        var nodesInScene = FindObjectsByType<Node>(FindObjectsSortMode.None); //temporary
        foreach (var node in nodesInScene)
        {
            if (_nodes.Contains(node) is false)
                _nodes.Add(node);
            
            node.Initialize();
        }
    }

    public void AddNode(Node node) => _nodes.Add(node);

    public bool HasEdge(Node from, Node to)
    {
        bool foundedFrom = false, foundedTo = false;

        foreach (var node in _nodes)
        {
            if (foundedFrom || node != from) continue;
            else foundedFrom = true;

            if (foundedTo || node != to) continue;
            else foundedTo = true;

            if (foundedFrom && foundedTo) return true;
        }

        return foundedFrom && foundedTo;
    }
}
