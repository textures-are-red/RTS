using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeTransitionsHandler : IDisposable
{
    public sbyte TargetLevel
    {
        get => _targetLevel;
        set
        {
            if (_targetLevel == value) return;
            _targetLevel = value;
            RecalculateTransitionTimes();
        }
    }
        
    private EdgeTransitionVisualData _visualData;
    private MonoBehaviour _owner;
    private Action<Unit> _onTransitionEndCallback;

    private List<KeyValuePair<Unit, Coroutine>> _unitTransitions = new();
    private List<KeyValuePair<Unit, float>> _transitionTimes = new();

    private sbyte _targetLevel;

    public EdgeTransitionsHandler(MonoBehaviour owner, Material material, Action<Unit> onTransitionEndCallback)
    {
        _owner = owner;
        _visualData = new(material);
        _onTransitionEndCallback = onTransitionEndCallback;
    }

    public bool IsUnitTransitioning(Unit unit) => _unitTransitions.Exists(kvp => kvp.Key == unit);

    public float UnitTransitionTime(Unit unit)
    {
        float newTime = Graph.Instance.DefaultTransitionTime + (((float)(_targetLevel - unit.Level)) * Graph.Instance.TransitionTimeLevelDifference);
        return Mathf.Max(newTime, Graph.Instance.MinTransitionTime);
    }

    public void StartUnitTransition(Unit unit)
    {
        if (_unitTransitions.Exists(kvp => kvp.Key == unit)) return;

        _transitionTimes.Add(new(unit, UnitTransitionTime(unit)));
        _unitTransitions.Add(new(unit, _owner.StartCoroutine(UnitTransition(unit))));

        _visualData.AddUnit(unit, unit.UnitColor, 0f);
    }

    public void StopUnitTransition(Unit unit)
    {
        StopUnitCoroutine(unit);
        ReleaseLogicData(unit);

        //UpdateBuffers();
        _visualData.RemoveUnit(unit);
    }

    public void ReleaseLogicData(Unit unit)
    {
        RemoveByKey(_transitionTimes, unit);
        RemoveByKey(_unitTransitions, unit);
    }

    private void StopUnitCoroutine(Unit unit)
    {
        int coroutineIndex = _unitTransitions.FindIndex(kvp => kvp.Key == unit);
        
        if (coroutineIndex >= 0)
        {
            var pair = _unitTransitions[coroutineIndex];
            _owner.StopCoroutine(pair.Value);
            _unitTransitions.RemoveAt(coroutineIndex);
        }
    }

    private void RemoveByKey<T>(List<KeyValuePair<Unit, T>> list, Unit unit)
    {
        int index = list.FindIndex(kvp => kvp.Key == unit);
        if (index >= 0) list.RemoveAt(index);
    }

    private IEnumerator UnitTransition(Unit unit)
    {
        float elapsed = 0f;

        while (true)
        {
            int index = _transitionTimes.FindIndex(kvp => kvp.Key == unit);

            if (index < 0)
            {
                Debug.LogError($"There is no time for unit actually"); break;
            }

            float transitionTime = _transitionTimes[index].Value;
            if (elapsed >= transitionTime || transitionTime is 0) break;

            _visualData.UpdateUnitThreshold(unit, elapsed / transitionTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        ReleaseLogicData(unit);
        _visualData.RemoveUnit(unit);

        unit.EndMoveTo();

        _onTransitionEndCallback?.Invoke(unit);
    }

    private void RecalculateTransitionTimes()
    {
        if (_transitionTimes.Count is 0 || _unitTransitions.Count is 0) return;

        for (int i = 0; i < _transitionTimes.Count; ++i)
        {
            var pair = _transitionTimes[i];

            float newTransitionTime = UnitTransitionTime(pair.Key);

            _transitionTimes[i] = new(pair.Key, newTransitionTime);
            Debug.Log($"{i}: {newTransitionTime}");
        }
    }

    public void Dispose()
    {
        _visualData.Dispose();
        _onTransitionEndCallback = null;
    }
}
