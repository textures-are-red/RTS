using UnityEngine;

public class CurrentNodeController : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private UnitController _unitController;

    public void UpgradeCurrentNode()
    {
        Node nodeToUpgrade = _cameraController.NodeToCenter ?? _cameraController.ClickedNode;
        if (nodeToUpgrade is null) return;

        if (nodeToUpgrade.NodeUnits.HasUnits)
            nodeToUpgrade.NodeLevel.Upgrade();
    }

    public void DowngradeCurrentNode()
    {
        Node nodeToDowngrade = _cameraController.NodeToCenter ?? _cameraController.ClickedNode;
        if (nodeToDowngrade is null) return;

        if (nodeToDowngrade.NodeUnits.HasUnits)
            nodeToDowngrade.NodeLevel.Downgrade();
    }
}
