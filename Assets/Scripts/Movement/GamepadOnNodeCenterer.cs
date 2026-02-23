using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadOnNodeCenterer : MonoBehaviour
{
    private static readonly Vector3 _cameraCenter = new Vector3(0.5f, 0.5f, 0);

    [SerializeField] private LayerMask _fieldMask;
    [SerializeField] private LayerMask _nodeMask;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _castRange = 100f;

    [Space(15)]

    [SerializeField] private float _centerSpeed = 5f;

    private Camera _cameraMain;
    private Transform _cameraMainTransform;

    private void Awake()
    {
        _cameraMain = Camera.main;
        _cameraMainTransform = _cameraMain.transform;
    }

    public Vector3 CenterCameraOnNode(Node nodeToCenter)
    {
        if (Mathf.Approximately(_cameraMainTransform.forward.y, 0f)) return Vector3.zero;

        if (nodeToCenter is null) return Vector3.zero;

        Vector3 nodeToCenterPosition = nodeToCenter.transform.position;

        float t = (nodeToCenterPosition.y - _cameraMainTransform.position.y) / _cameraMainTransform.forward.y;
        Vector3 target = new Vector3(nodeToCenterPosition.x - t * _cameraMainTransform.forward.x, 0f, nodeToCenterPosition.z - t * _cameraMainTransform.forward.z);
        Vector3 flatPosition = new Vector3(_cameraMainTransform.transform.position.x, 0f, _cameraMainTransform.transform.position.z);

        Vector3 targetDelta = target - flatPosition;

        return Vector3.MoveTowards(Vector3.zero, targetDelta, Time.deltaTime * _centerSpeed);
    }

    public Node FindNearestNode(float radiusMultiplier = 1f)
    {
        Ray fromCameraCenter = _cameraMain.ViewportPointToRay(_cameraCenter);
        RaycastHit raycastHit;

        if (Physics.Raycast(fromCameraCenter, out raycastHit, _castRange, _fieldMask, QueryTriggerInteraction.Ignore) is false) return null;
        Vector3 hitPoint = raycastHit.point;

        var hits = Physics.SphereCastAll(fromCameraCenter, _radius * radiusMultiplier, _castRange, _nodeMask, QueryTriggerInteraction.Ignore);
        if (hits.Length is 0) return null;

        Vector3 rayOrigin = fromCameraCenter.origin;
        Vector3 rayDirection = Camera.main.transform.forward;

        GameObject nearestNodeGameObject = null;
        float nearestNodeDistanceSqr = float.PositiveInfinity;

        foreach(var hit in hits)
        {
            Vector3 toObject = hit.transform.position - rayOrigin;
            float currentDistance = Vector3.Cross(toObject, rayDirection).sqrMagnitude;

            if (nearestNodeGameObject is null || currentDistance < nearestNodeDistanceSqr)
            {
                nearestNodeGameObject = hit.collider.gameObject;
                nearestNodeDistanceSqr = currentDistance;
            }
        }

        return nearestNodeGameObject.GetComponent<Node>();
    }
}
