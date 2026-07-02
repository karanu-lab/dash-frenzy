// ============================================================
//  CameraFollow.cs
//  Attach to: Main Camera
//  Handles: Smooth third-person follow from behind the player
// ============================================================

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;                                // Drag Max's Transform here in Inspector

    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0, 3.5f, -6f);    // Behind and above the player
    public float smoothSpeed = 10f;                        // Higher = snappier follow

    // ----------------------------------------------------------
    void LateUpdate()
    {
        // LateUpdate runs after all Update() calls — prevents camera jitter
        if (player == null) return;

        Vector3 targetPosition = player.position + offset;

        // Smoothly move camera toward the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition,
                                          smoothSpeed * Time.deltaTime);

        // Always look at the player
        transform.LookAt(player);
    }
}
