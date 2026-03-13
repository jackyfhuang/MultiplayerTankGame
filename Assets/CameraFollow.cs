using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // The tank to follow (assign in Inspector)
    
    [Header("Follow Settings")]
    public float smoothSpeed = 0.125f; // How smoothly the camera follows (lower = smoother)
    public Vector3 offset = new Vector3(0, 0, -10); // Camera offset from target (Z should match camera's Z position)
    
    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: No target assigned!");
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }
}
