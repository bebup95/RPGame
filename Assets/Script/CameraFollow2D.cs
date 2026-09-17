using UnityEngine;

public sealed class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.2f;
    [SerializeField] private bool clampToBounds = true;
    [SerializeField] private Vector2 minimumPosition;
    [SerializeField] private Vector2 maximumPosition;

    private Vector3 followVelocity;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + (Vector3)offset;
        desiredPosition.z = transform.position.z;

        if (clampToBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minimumPosition.x, maximumPosition.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minimumPosition.y, maximumPosition.y);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            smoothTime);
    }
}
