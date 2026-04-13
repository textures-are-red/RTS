using System.Collections.Generic;
using UnityEngine;

public class NodeAppearance : MonoBehaviour
{
    [SerializeField] private Color _multipleUnitsLocatedColor;

    private Material _material;
    private Color _defaultColor;
    private Lightener _lightener;

    public void UpdateColor(IReadOnlyList<Unit> locatedUnits)
    {
        Color newColor = locatedUnits.Count switch
        {
            > 1 => _multipleUnitsLocatedColor,
            1   => locatedUnits[0].UnitColor,
            _   => _defaultColor
        };

        _lightener.UpdateOriginalColor(newColor);
        _material.color = newColor;
    }

    public void ChangeColor(Color newColor) => _lightener.UpdateOriginalColor(newColor);

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
        _defaultColor = _material.color;
        _lightener = GetComponent<Lightener>();
    }
}
