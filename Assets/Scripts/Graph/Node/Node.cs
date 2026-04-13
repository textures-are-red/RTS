using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Node : MonoBehaviour
{
    private const sbyte _levelIndicatorsCount = 4;
    private const float _levelIndicatorsRotation = 90f;

    [SerializeField] private NodeLevel _nodeLevel;
    [SerializeField] private NodeInventory _nodeInventory;
    [SerializeField] private NodeConnections _nodeConnections;
    [SerializeField] private NodeAppearance _nodeAppearance;

    public NodeLevel NodeLevel => _nodeLevel;
    public NodeInventory NodeInventory => _nodeInventory;
    public NodeConnections NodeConnections => _nodeConnections;
    public NodeUnits NodeUnits => _nodeUnits;
    public NodeAppearance NodeAppearance => _nodeAppearance;

    private NodeUnits _nodeUnits;

    public void InitializeUnitsComponent()
    {
        _nodeUnits = new(_nodeAppearance);
        //Graph.AddNode(this);
    }

    public void InitializeConnections()
    {
        _nodeConnections.Initialize();
    }
}
