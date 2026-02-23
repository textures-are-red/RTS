using System;
//using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    [SerializeField] private float _edgeIndent = 0.5f;
    [SerializeField, Min(0f)] private float _margin = 3f;

    public Node From { get; private set; }
    public Node To { get; private set; }

    public event Action<Unit> OnTransitionEnd;
    public IReadOnlyList<KeyValuePair<Unit, Coroutine>> UnitTransitions => _unitTransitions; 

    private List<KeyValuePair<Unit, Coroutine>> _unitTransitions = new();
    private List<KeyValuePair<Unit, float>> _transitionTimes = new();
    private List<KeyValuePair<Unit, float>> _thresholds = new();
    private List<KeyValuePair<Unit, Color>> _unitColors = new();

    private ComputeBuffer _colorBuffer;
    private ComputeBuffer _thresholdsBuffer;

    private int[] _sortedIndices;
    private Color[] _sortedColors;
    private float[] _sortedThresholds;

    private Material _material;

    private bool _isInitialized;
    private bool _subscribed;

    public void LocateBetweenNodes(Node from, Node to)
    {
        From = from;
        To = to;

        if (_subscribed is false && To is not null)
        {
            To.LevelChanged += OnLevelChanged;
            _subscribed = true;
        }

        Transform toTransform = to.transform;
        Vector3 spawnPoint = (from.transform.position + toTransform.position) / 2f;
        //spawnPoint.y = currentField.transform.y + offset;
        transform.position = spawnPoint;

        transform.LookAt(toTransform);
        transform.position += transform.right * _edgeIndent;

        transform.localEulerAngles = new Vector3(90f, transform.localEulerAngles.y, transform.localEulerAngles.z - 90f);

        transform.localScale = new Vector3(Vector3.Distance(from.transform.position, toTransform.position) - _margin, transform.localScale.y, transform.localScale.z);
    }

    public void ReleaseUnitData(Unit unit)
    {
        int colorIndex = _unitColors.FindIndex(kvp => kvp.Key == unit);
        if (colorIndex >= 0) _unitColors.RemoveAt(colorIndex);

        int thresholdIndex = _thresholds.FindIndex(kvp => kvp.Key == unit);
        if (thresholdIndex >= 0) _thresholds.RemoveAt(thresholdIndex);

        int transitionTimeIndex = _transitionTimes.FindIndex(kvp => kvp.Key == unit);
        if (transitionTimeIndex >= 0) _transitionTimes.RemoveAt(transitionTimeIndex);

        int transitionIndex = _unitTransitions.FindIndex(kvp => kvp.Key == unit);
        if (transitionIndex >= 0) _unitTransitions.RemoveAt(transitionIndex);
    }

    public void StartUnitTransition(Unit unit)
    {
        if (_unitTransitions.Exists(kvp => kvp.Key == unit)) return;

        _transitionTimes.Add(new(unit, UnitTransitionTime(unit)));
        _unitTransitions.Add(new(unit, StartCoroutine(UnitTransition(unit))));

        _unitColors.Add(new KeyValuePair<Unit, Color>(unit, unit.UnitColor));
        _thresholds.Add(new KeyValuePair<Unit, float>(unit, 0f));

        UpdateBuffers();
    }

    public void StopUnitTransition(Unit unit)
    {
        StopUnitCoroutine(unit);
        ReleaseUnitData(unit);

        UpdateBuffers();
    }

    private void StopUnitCoroutine(Unit unit)
    {
        int coroutineIndex = _unitTransitions.FindIndex(kvp => kvp.Key == unit);
        
        if (coroutineIndex >= 0)
        {
            var pair = _unitTransitions[coroutineIndex];
            StopCoroutine(pair.Value);
            _unitTransitions.RemoveAt(coroutineIndex);
        }
    }

    private void UpdateBuffers()
    {
        int count = _thresholds.Count;

        if (count is 0)
        {
            SetDummyBuffers(); return;
        }

        if (_sortedIndices is null || _sortedIndices.Length != count)
        {
            _sortedIndices = new int[count];
            _sortedColors = new Color[count];
            _sortedThresholds = new float[count];
        }

        for (int i = 0; i < count; ++i)
            _sortedIndices[i] = i;

        Array.Sort(_sortedIndices, (a, b) => _thresholds[a].Value.CompareTo(_thresholds[b].Value));

        for (int i = 0; i < count; ++i)
        {
            int idx = _sortedIndices[i];
            _sortedColors[i] = _unitColors[idx].Value;
            _sortedThresholds[i] = _thresholds[idx].Value;
        }

        if (_colorBuffer is null || _colorBuffer.count != count)
        {
            _colorBuffer?.Release();
            _colorBuffer = new ComputeBuffer(count, sizeof(float) * 4);
        }

        _colorBuffer.SetData(_sortedColors);

        if (_thresholdsBuffer is null || _thresholdsBuffer.count != count)
        {
            _thresholdsBuffer?.Release();
            _thresholdsBuffer = new ComputeBuffer(count, sizeof(float));
        }

        _thresholdsBuffer.SetData(_sortedThresholds);

        _material.SetBuffer("_Colors", _colorBuffer);
        _material.SetBuffer("_Thresholds", _thresholdsBuffer);
        _material.SetInt("_ColorsCount", count);
    }

    private void SetDummyBuffers()
    {
        _colorBuffer?.Release();
        _colorBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        _colorBuffer.SetData(new Color[] { _material.GetColor("_BackgroundColor") });
            
        _thresholdsBuffer?.Release();
        _thresholdsBuffer = new ComputeBuffer(1, sizeof(float));
        _thresholdsBuffer.SetData(new float[] { 0f });
            
        _material.SetBuffer("_Colors", _colorBuffer);
            _material.SetBuffer("_Thresholds", _thresholdsBuffer);
        _material.SetInt("_ColorsCount", 0);
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

            int thresholdIndex = _thresholds.FindIndex(kvp => kvp.Key == unit);
            if (thresholdIndex >= 0) _thresholds[thresholdIndex] = new KeyValuePair<Unit, float>(unit, elapsed / transitionTime);
            UpdateBuffers();

            elapsed += Time.deltaTime;
            yield return null;
        }

        ReleaseUnitData(unit);
        UpdateBuffers();

        OnTransitionEnd?.Invoke(unit);
    }

    private void OnLevelChanged()
    {
        if (_transitionTimes.Count is 0 || _unitTransitions.Count is 0) return;

        for (int i = 0; i < _transitionTimes.Count; ++i)
        {
            var pair = _transitionTimes[i];

            float newTransitionTime = UnitTransitionTime(pair.Key);

            _transitionTimes[i] = new(pair.Key, newTransitionTime);
            print($"{i}: {newTransitionTime}");
        }
    }

    private float UnitTransitionTime(Unit unit)
    {
        float newTime = Graph.Instance.DefaultTransitionTime + (((float)(To.Level - unit.Level)) * Graph.Instance.TransitionTimeLevelDifference);
        return Mathf.Max(newTime, Graph.Instance.MinTransitionTime);
    }

    private void OnEnable()
    {
        if (To is not null && _subscribed is false)
        {
            To.LevelChanged += OnLevelChanged;
            _subscribed = true;
        }

        if (_isInitialized) return;

        _material ??= GetComponent<Renderer>().material;
        UpdateBuffers();

        _isInitialized = true;
    }

    private void OnDisable()
    {
        if (To is not null && _subscribed)
        {
            To.LevelChanged -= OnLevelChanged;
            _subscribed = false;
        }
    }

    private void OnDestroy()
    {
        _colorBuffer?.Release();
        _colorBuffer = null;
        _thresholdsBuffer?.Release();
        _thresholdsBuffer = null;

        OnTransitionEnd = null;
    }
}
