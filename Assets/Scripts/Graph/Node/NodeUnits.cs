using System.Collections.Generic;
using UnityEngine;

public class NodeUnits
{
    public bool HasUnits => _locatedUnits.Count is not 0;
    public IReadOnlyList<Unit> LocatedUnits => _locatedUnits;

    private List<Unit> _locatedUnits = new();
    private NodeAppearance _nodeAppearance;

    public NodeUnits(NodeAppearance nodeAppearance)
    {
        _nodeAppearance = nodeAppearance;
    }

    public void LocateUnit(Unit unit)
    {
        _locatedUnits.Add(unit);
        _nodeAppearance.UpdateColor(_locatedUnits);
    }

    public void UnlocateUnit(Unit unit)
    {
        _locatedUnits.Remove(unit);
        _nodeAppearance.UpdateColor(_locatedUnits);
    }
}
