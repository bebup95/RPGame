using UnityEngine;

public sealed class ParallaxBackground2D : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField, Range(0f, 1f)] private float horizontalFactor = 0.15f;
    [SerializeField, Range(0f, 1f)] private float verticalFactor = 0.05f;

    private Vector3 startingPosition;
    private Vector3 cameraStartingPosition;

    private void Awake()
    {
        startingPosition = transform.position;

        if (cameraTransform != null)
            cameraStartingPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 cameraDelta = cameraTransform.position - cameraStartingPosition;
        transform.position = startingPosition + new Vector3(
            cameraDelta.x * horizontalFactor,
            cameraDelta.y * verticalFactor,
            0f);
    }
}
