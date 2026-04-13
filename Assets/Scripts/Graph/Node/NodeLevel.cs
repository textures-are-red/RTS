using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NodeLevel : MonoBehaviour
{
    private const sbyte _levelIndicatorsCount = 4;
    private const float _levelIndicatorsRotation = 90f;

    [SerializeField] private sbyte _maxLevel = 5;
    [SerializeField] private sbyte _minLevel = 0;
    [SerializeField] private sbyte _level = 1;

    [Space(15)]

    [SerializeField] private TextMeshPro _levelIndicatorPrefab;
    [SerializeField] private Vector3 _levelIndicatorMargin;

    public event Action LevelChanged;

    public sbyte Level => _level;

    private List<TextMeshPro> _levelIndicators = new();

    public void Upgrade(sbyte levelsToUpgrade = 1)
    {
        _level = (sbyte)((_level + levelsToUpgrade) > _maxLevel ? _maxLevel : _level + levelsToUpgrade);

        foreach (var indicator in _levelIndicators)
            indicator.text = _level.ToString();
        
        LevelChanged?.Invoke();
    }

    public void Downgrade(sbyte levelsToDowngrade = 1)
    {
        _level = (sbyte)((_level - levelsToDowngrade) < _minLevel ? _minLevel : _level - levelsToDowngrade);
        
        foreach (var indicator in _levelIndicators)
            indicator.text = _level.ToString();

        LevelChanged?.Invoke();
    }

    private void SpawnLevelIndicator()
    {
        Vector3? rotationPointer = null;

        for (sbyte i = 0; i < _levelIndicatorsCount; ++i)
        {
            var indicator = Instantiate(_levelIndicatorPrefab, transform);
            indicator.text = _level.ToString();
            _levelIndicators.Add(indicator);

            Transform indicatorTransform = indicator.transform;
            if (rotationPointer.HasValue is false) rotationPointer = indicatorTransform.eulerAngles;

            indicatorTransform.eulerAngles = rotationPointer.GetValueOrDefault();

            Vector3 scale = indicatorTransform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            indicatorTransform.localScale = scale;

            indicatorTransform.Translate(_levelIndicatorMargin, Space.Self);

            Vector3 rotationPointerValue = rotationPointer.GetValueOrDefault();
            rotationPointerValue.y += _levelIndicatorsRotation;
            rotationPointer = rotationPointerValue;
        }
    }

    private void DestroyLevelIndicators()
    {
        foreach (var indicator in _levelIndicators)
        {
            if (indicator is not null)
                Destroy(indicator.gameObject);
        }

        _levelIndicators.Clear();
    }

    private void OnValidate()
    {
        Mathf.Clamp(_level, _minLevel, _maxLevel);
    }

    private void Awake()
    {
        SpawnLevelIndicator();
    }

    private void OnDestroy()
    {
        DestroyLevelIndicators();
    }
}
