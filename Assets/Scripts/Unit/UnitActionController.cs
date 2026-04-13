using UnityEngine;
using System.Collections.ObjectModel;

public class UnitActionController : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private UnitManager _unitManager;

    private ReadOnlyObservableCollection<Unit> _existsUnits => _unitManager.ExistsUnits;

    private CurrentUnitSelector _selector;
    private byte _inventoryCellsCount;

    public void Initialize(CurrentUnitSelector selector, byte inventoryCellsCount)
    {
        _selector = selector;
        _inventoryCellsCount = inventoryCellsCount;
    }

    public void MoveCurrentUnit()
    {
        Node nodeToMove = _cameraController.NodeToCenter ?? _cameraController.ClickedNode;
        if (nodeToMove is not null) _selector.CurrentUnit.StartMoveTo(nodeToMove);
    }

    public void UpgradeCurrentUnit()
    {
        Unit currentUnit = _selector.CurrentUnit;
        if (currentUnit is null || currentUnit.IsTransiting) return;
        currentUnit.Upgrade();
    }
    public void DowngradeCurrentUnit()
    {
        Unit currentUnit = _selector.CurrentUnit;
        if (currentUnit is null || currentUnit.IsTransiting) return;
        currentUnit.Downgrade();
    }

    public void CreateUnitOnCurrentUnitNode()
    {
        Unit currentUnit = _selector.CurrentUnit;

        if (currentUnit?.CurrentNode is not null)
        {
            _unitManager.CreateUnit(currentUnit.CurrentNode, _inventoryCellsCount, Unit.MinLevel);
            _selector.SetCurrentUnit(_existsUnits[_existsUnits.Count - 1]);
        }
    }

    public void DisposeCurrentUnit()
    {
        Unit currentUnit = _selector.CurrentUnit;

        if (currentUnit is not null && _existsUnits.Count is not 0)
        {
            _unitManager.DisposeUnit(currentUnit);
            _selector.SetCurrentUnit(_existsUnits.Count is not 0 ? _existsUnits[_existsUnits.Count - 1] : null);
        }
    }
}
