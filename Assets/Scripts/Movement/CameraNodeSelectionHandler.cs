using UnityEngine;

public class CameraNodeSelectionHandler
{
    public Node ClickedNode { get; private set; }

    private CameraInputHandler _inputHandler;

    public CameraNodeSelectionHandler(CameraInputHandler inputHandler)
    {
        _inputHandler = inputHandler;
    }

    public void Enable()
    {
        NodeClickHandler.OnAnyNodeClicked += OnAnyNodeClicked;
        Field.OnBackgroundClicked += OnBackgroundClicked;
        _inputHandler.DefaultDeviceChanged += ClearClickedNode;
    }

    public void Disable()
    {
        NodeClickHandler.OnAnyNodeClicked -= OnAnyNodeClicked;
        Field.OnBackgroundClicked -= OnBackgroundClicked;
        _inputHandler.DefaultDeviceChanged -= ClearClickedNode;

        ClearClickedNode();
    }

    private void OnAnyNodeClicked(Node node)
    {
        ClickedNode = node;
    }

    private void OnBackgroundClicked()
    {
        ClickedNode = null;
    }

    private void ClearClickedNode()
    {
        ClickedNode = null;
    }
}
