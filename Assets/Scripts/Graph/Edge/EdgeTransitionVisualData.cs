using System;
using System.Collections.Generic;
using UnityEngine;

public class EdgeTransitionVisualData : IDisposable
{
    private Material _material;

    private List<KeyValuePair<Unit, float>> _thresholds = new();
    private List<KeyValuePair<Unit, Color>> _unitColors = new();

    private ComputeBuffer _colorBuffer;
    private ComputeBuffer _thresholdsBuffer;

    private int[] _sortedIndices;
    private Color[] _sortedColors;
    private float[] _sortedThresholds;

    public EdgeTransitionVisualData(Material material)
    {
        _material = material;
        SetDummyBuffers();
    }

    public void AddUnit(Unit unit, Color color, float initialThreshold = 0f)
    {
        _unitColors.Add(new(unit, color));
        _thresholds.Add(new(unit, initialThreshold));
        UpdateBuffers();
    }

    public void UpdateUnitThreshold(Unit unit, float progress)
    {
        int index = _thresholds.FindIndex(kvp => kvp.Key == unit);
        if (index >= 0)
        {
            _thresholds[index] = new(unit, progress);
            UpdateBuffers();
        }
    }

    public void RemoveUnit(Unit unit)
    {
        RemoveByKey(_unitColors, unit);
        RemoveByKey(_thresholds, unit);
        UpdateBuffers();
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

    private void RemoveByKey<T>(List<KeyValuePair<Unit, T>> list, Unit unit)
    {
        int index = list.FindIndex(kvp => kvp.Key == unit);
        if (index >= 0) list.RemoveAt(index);
    }

    public void Dispose()
    {
        _colorBuffer?.Release();
        _thresholdsBuffer?.Release();
        _colorBuffer = null;
        _thresholdsBuffer = null;
    }
}
