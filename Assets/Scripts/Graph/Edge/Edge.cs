using System;
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

    private EdgeTransitionsHandler _transitionsHandler;

    private bool _nodeSubscribed;

    public void LocateBetweenNodes(Node from, Node to)
    {
        From = from;
        To = to;

        if (_nodeSubscribed is false && To is not null)
        {
            To.NodeLevel.LevelChanged += OnLevelChanged;
            _nodeSubscribed = true;
        }

        Transform toTransform = to.transform;
        Vector3 spawnPoint = (from.transform.position + toTransform.position) / 2f;
        //spawnPoint.y = currentField.transform.y + offset;
        transform.position = spawnPoint;

        transform.LookAt(toTransform);
        transform.position += transform.right * _edgeIndent;

        transform.localEulerAngles = new Vector3(90f, transform.localEulerAngles.y, transform.localEulerAngles.z - 90f);

        transform.localScale = new Vector3(Vector3.Distance(from.transform.position, toTransform.position) - _margin, transform.localScale.y, transform.localScale.z);

        EnsureTransitionHandler();
        if (To is not null) _transitionsHandler.TargetLevel = To.NodeLevel.Level;
    }

    public void StartUnitTransition(Unit unit) => _transitionsHandler.StartUnitTransition(unit);
    public void StopUnitTransition(Unit unit) => _transitionsHandler.StopUnitTransition(unit);

    private void EnsureTransitionHandler()
    {
        if (_transitionsHandler is not null) return;

        Material material = GetComponent<Renderer>().material;
        _transitionsHandler = new EdgeTransitionsHandler(this, material, unit => OnTransitionEnd?.Invoke(unit));
    }

    private void OnLevelChanged() => _transitionsHandler.TargetLevel = To.NodeLevel.Level;

    private void OnEnable()
    {
        if (To is not null && _nodeSubscribed is false)
        {
            To.NodeLevel.LevelChanged += OnLevelChanged;
            _nodeSubscribed = true;
        }

        EnsureTransitionHandler();
    }

    private void OnDisable()
    {
        if (To is not null && _nodeSubscribed)
        {
            To.NodeLevel.LevelChanged -= OnLevelChanged;
            _nodeSubscribed = false;
        }
    }

    private void OnDestroy()
    {
        _transitionsHandler?.Dispose();
        OnTransitionEnd = null;
    }
}
