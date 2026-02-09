using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    [SerializeField] private float _defaultTransitionTime = 1.5f;

    [HideInInspector] public Node From;
    [HideInInspector] public Node To;

    public event Action<Unit> OnTransitionEnd;
    public IReadOnlyList<KeyValuePair<Unit, Coroutine>> UnitTransitions => _unitTransitions; 

    private List<KeyValuePair<Unit, Coroutine>> _unitTransitions = new();

    private Color _defaultColor;
    private Material _material;

    private void OnEnable()
    {
        _material ??= GetComponent<Renderer>().material;
        if (_defaultColor == default(Color)) _defaultColor = _material.color;
    }

    public void StartUnitTransition(Unit unit)
    {
        if (_unitTransitions.Exists(kvp => kvp.Key == unit)) return;

        _unitTransitions.Add(new(unit, StartCoroutine(UnitTransition(unit))));

        _material.color = unit.UnitColor;
    }

    public void StopUnitTransition(Unit unit)
    {
        int index = _unitTransitions.FindIndex(kvp => kvp.Key == unit);
        
        if (index >= 0)
        {
            var pair = _unitTransitions[index];
            StopCoroutine(pair.Value);
            _unitTransitions.RemoveAt(index);
        }

        _material.color = _defaultColor;
    }

    private IEnumerator UnitTransition(Unit unit)
    {
        float elapsed = 0f;

        while (elapsed < _defaultTransitionTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        OnTransitionEnd?.Invoke(unit);

        int index = _unitTransitions.FindIndex(kvp => kvp.Key == unit);
        
        if (index >= 0)
            _unitTransitions.RemoveAt(index);
        
        _material.color = _defaultColor;
    }

    private void OnDestroy()
    {
        OnTransitionEnd = null;
    }
}
