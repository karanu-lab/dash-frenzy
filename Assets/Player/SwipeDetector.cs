// ============================================================
//  SwipeDetector.cs
//  Attach to: Player or a dedicated SwipeManager GameObject
//  Handles: Translates touch swipes into player actions (mobile)
// ============================================================

using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    [Header("Swipe Settings")]
    public float swipeThreshold = 50f;    // Minimum pixels to count as a swipe

    private Vector2 startTouch;
    private PlayerController player;

    // ----------------------------------------------------------
    void Start()
    {
        // FindAnyObjectByType is the Unity 6 equivalent of FindObjectOfType
        player = FindAnyObjectByType<PlayerController>();
    }

    void Update()
    {
        if (GameManager.instance == null || !GameManager.instance.isPlaying) return;
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        // Record where the touch started
        if (touch.phase == TouchPhase.Began)
            startTouch = touch.position;

        // When finger lifts, calculate swipe direction
        if (touch.phase == TouchPhase.Ended)
        {
            Vector2 swipe = touch.position - startTouch;

            // Determine if swipe is more horizontal or vertical
            if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            {
                // Horizontal swipe — change lane
                if (swipe.x >  swipeThreshold) player.ShiftLane(1);   // Right
                if (swipe.x < -swipeThreshold) player.ShiftLane(-1);  // Left
            }
            else
            {
                // Vertical swipe — jump or slide
                if (swipe.y >  swipeThreshold) player.Jump();   // Up
                if (swipe.y < -swipeThreshold) player.Slide();  // Down
            }
        }
    }
}
