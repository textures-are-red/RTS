using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    [SerializeField] private MouseMoveEnable _mouseMoveEnabler;
 
    [SerializeField] private Node _startNode;
    [SerializeField] private Color _startColor;

    [SerializeField] private List<Color> _colors;

    public IReadOnlyList<Unit> ExistsUnits => _existsUnits;

    private Unit _currentUnit;
    private List<Unit> _existsUnits = new();
    //private List<>
    //private List<>

    private void Awake()
    {
        _currentUnit = new Unit(_startNode, _startColor);
        _existsUnits.Add(_currentUnit);
    }

    public void MoveCurrentUnit()
    {
        if (_mouseMoveEnabler.ClickedNode is not null)
            _currentUnit.StartMoveTo(_mouseMoveEnabler.ClickedNode);
    }
}
