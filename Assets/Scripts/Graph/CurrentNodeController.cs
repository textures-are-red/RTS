using UnityEngine;

public class CurrentNodeController : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private UnitController _unitController;
    [SerializeField] private MouseMoveEnable _mouseMoveEnabler;

    public void UpgradeCurrentNode()
    {
        Node nodeToUpgrade = _cameraController.NodeToCenter ?? _mouseMoveEnabler.ClickedNode;
        if (nodeToUpgrade is null) return;

        if (nodeToUpgrade.HasUnits)
            nodeToUpgrade.Upgrade();
    }

    public void DowngradeCurrentNode()
    {
        Node nodeToDowngrade = _cameraController.NodeToCenter ?? _mouseMoveEnabler.ClickedNode;
        if (nodeToDowngrade is null) return;

        if (nodeToDowngrade.HasUnits)
            nodeToDowngrade.Downgrade();
    }
}
