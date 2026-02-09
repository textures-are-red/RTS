using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Node : MonoBehaviour, IInitializable, IClickable
{
    [SerializeField] private Edge _edgePrefab;
    [SerializeField] private float _edgeIndent = 0.75f;

    [SerializeField] private List<Node> _neighbors;

    [SerializeField] private UnityEvent<Transform> _onClick;

    public bool IsInitialized { get; private set; }

    public IReadOnlyList<Edge> Edges => _edges;
    public IReadOnlyList<Unit> LocatedUnits => _locatedUnits;

    private List<Edge> _edges = new();
    private List<Unit> _locatedUnits = new();

    private Material _material;
    private Color _defaultColor;
    private Lightener _lightener;

    public void Initialize()
    {
        if (IsInitialized) return;

        _material = GetComponent<Renderer>().material;
        _lightener = GetComponent<Lightener>();
        _lightener.Initialize();

        _defaultColor = _material.color;

        //Graph.AddNode(this);

        SpawnEdges();

        IsInitialized = true;
    }

    public void OnClick()
    {
        _onClick?.Invoke(transform);
    }

    public void LocateUnit(Unit unit)
    {
        _locatedUnits.Add(unit);
        _lightener.UpdateOriginalColor(unit.UnitColor);
        _material.color = unit.UnitColor;
    }

    public void UnlocateUnit(Unit unit)
    {
        _locatedUnits.Remove(unit);
        _lightener.UpdateOriginalColor(_defaultColor);
        _material.color = _defaultColor;
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
            newEdge.From = this;
            newEdge.To = neighbor;
            _edges.Add(newEdge);
            
            Vector3 spawnPoint = (transform.position + neighbor.transform.position) / 2f;
            //spawnPoint.y =
            newEdge.transform.position = spawnPoint;

            newEdge.transform.LookAt(neighbor.transform);

            newEdge.transform.position += newEdge.transform.right * _edgeIndent;

            newEdge.OnTransitionEnd += UnlocateUnit;
            newEdge.OnTransitionEnd += neighbor.LocateUnit;
            newEdge.OnTransitionEnd += unit => unit.EndMoveTo();
        }
    }
}
