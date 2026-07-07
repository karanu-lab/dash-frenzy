// ============================================================
//  PlayerController.cs
//  Attach to: Max (Player Root GameObject)
//  Handles: Lane switching, jumping, sliding, collision response,
//           and driving the Animator state machine.
// ============================================================

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ---- Lane Settings ----
    private int currentLane = 1;                        // 0 = Left, 1 = Centre, 2 = Right
    private float[] lanePositions = { -2f, 0f, 2f };   // World X positions per GDD
    public float laneChangeSpeed = 10f;

    // ---- Jump Settings ----
    public float jumpForce = 7f;
    private bool isGrounded = true;

    // ---- Slide Settings ----
    private bool isSliding = false;
    private float normalColliderHeight = 1.8f;
    private float slideColliderHeight  = 0.6f;

    // ---- Hit State ----
    private bool isHit = false;                         // Locks input during fall-over animation
    private float hitLockDuration = 1.2f;               // How long input is blocked after a hit

    // ---- Components ----
    private Rigidbody rb;
    private CapsuleCollider capsuleCol;
    private Animator anim;                              // Reference to character Animator
    private float fixedZ;

    // ---- Animator Parameter Hashes (cached for performance) ----
    private static readonly int HashIsGrounded  = Animator.StringToHash("IsGrounded");
    private static readonly int HashIsSliding   = Animator.StringToHash("IsSliding");
    private static readonly int HashJump        = Animator.StringToHash("Jump");
    private static readonly int HashHit         = Animator.StringToHash("Hit");
    private static readonly int HashSpeed       = Animator.StringToHash("Speed");

    // ----------------------------------------------------------
    void Start()
    {
        rb         = GetComponent<Rigidbody>();
        capsuleCol = GetComponent<CapsuleCollider>();
        fixedZ     = transform.position.z;

        // Find Animator — may be on a child (the character mesh)
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogWarning("PlayerController: No Animator found. Animations will not play.");

        // Lock Z position and all rotation to prevent physics drift
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionZ |
                             RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY |
                             RigidbodyConstraints.FreezeRotationZ;
        }

        // Frictionless physics material — prevents moving tiles launching player
        PhysicsMaterial frictionlessMat = new PhysicsMaterial("FrictionlessPlayer");
        frictionlessMat.dynamicFriction = 0f;
        frictionlessMat.staticFriction  = 0f;
        frictionlessMat.frictionCombine = PhysicsMaterialCombine.Minimum;
        frictionlessMat.bounciness      = 0f;
        frictionlessMat.bounceCombine   = PhysicsMaterialCombine.Minimum;
        if (capsuleCol != null)
            capsuleCol.sharedMaterial = frictionlessMat;

        // Start gameplay music
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGameplayMusic();
    }

    // ----------------------------------------------------------
    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.isPlaying) return;
        if (isHit) return; // Block all input during fall-over animation

        HandleKeyboardInput();
        SmoothMoveToLane();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        // Extra gravity on descent — prevents floaty jumps (2.5x total gravity per GDD)
        if (rb != null && !isGrounded && rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * 1.5f * Time.fixedDeltaTime;
    }

    // ----------------------------------------------------------
    //  ANIMATOR SYNC
    // ----------------------------------------------------------
    void UpdateAnimator()
    {
        if (anim == null) return;

        // Drive the blend tree / state machine each frame
        anim.SetBool(HashIsGrounded, isGrounded);
        anim.SetBool(HashIsSliding,  isSliding);

        // Feed current game speed to allow run-cycle to speed up naturally
        float normalizedSpeed = SpeedController.currentSpeed / 10f; // 10 = base speed
        anim.SetFloat(HashSpeed, normalizedSpeed, 0.1f, Time.deltaTime);
    }

    // ----------------------------------------------------------
    //  INPUT
    // ----------------------------------------------------------
    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))  ShiftLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) ShiftLane(1);
        if (Input.GetKeyDown(KeyCode.Space))      Jump();
        if (Input.GetKeyDown(KeyCode.DownArrow))  Slide();
    }

    // Called by SwipeDetector for touch input
    public void ShiftLane(int direction)
    {
        currentLane = Mathf.Clamp(currentLane + direction, 0, 2);
    }

    // ----------------------------------------------------------
    //  MOVEMENT
    // ----------------------------------------------------------
    void SmoothMoveToLane()
    {
        float targetX     = lanePositions[currentLane];
        Vector3 targetPos = new Vector3(targetX, transform.position.y, fixedZ);
        Vector3 nextPos   = Vector3.Lerp(transform.position, targetPos, laneChangeSpeed * Time.deltaTime);

        if (rb != null)
            rb.MovePosition(nextPos);
        else
            transform.position = nextPos;
    }

    // ----------------------------------------------------------
    //  JUMP
    // ----------------------------------------------------------
    public void Jump()
    {
        if (isSliding) StopSlide(); // Cancel slide early per GDD

        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;

            if (anim != null) anim.SetTrigger(HashJump);
        }
    }

    // ----------------------------------------------------------
    //  SLIDE
    // ----------------------------------------------------------
    public void Slide()
    {
        if (isGrounded)
        {
            isSliding         = true;
            capsuleCol.height = slideColliderHeight;
            CancelInvoke(nameof(StopSlide));
            Invoke(nameof(StopSlide), 0.8f); // Stand back up after 0.8s per GDD
        }
    }

    void StopSlide()
    {
        isSliding         = false;
        capsuleCol.height = normalColliderHeight;
        CancelInvoke(nameof(StopSlide));
    }

    // ----------------------------------------------------------
    //  HIT REACTION (Fall Over)
    // ----------------------------------------------------------
    public void TriggerHitAnimation()
    {
        if (anim != null) anim.SetTrigger(HashHit);

        isHit = true;
        Invoke(nameof(ClearHitState), hitLockDuration);
    }

    void ClearHitState()
    {
        isHit = false;
    }

    // ----------------------------------------------------------
    //  COLLISION
    // ----------------------------------------------------------
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
            isGrounded = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Coin collection
        if (other.CompareTag("coin"))
        {
            if (ScoreManager.instance != null)  ScoreManager.instance.AddCoin();
            if (AudioManager.instance != null)  AudioManager.instance.PlayCoinSFX();
            Destroy(other.gameObject);
        }

        // Obstacle hit — triggers fall-over then costs a life
        if (other.CompareTag("obstacles"))
        {
            TriggerHitAnimation();

            if (LivesManager.instance != null)  LivesManager.instance.LoseLife();
            if (AudioManager.instance != null)  AudioManager.instance.PlayObstacleSFX();
            Destroy(other.gameObject);
        }

        // Power-up collection
        if (other.CompareTag("magnet")     ||
            other.CompareTag("shield")     ||
            other.CompareTag("speedboost") ||
            other.CompareTag("Multiplier"))
        {
            if (PowerUpManager.instance != null) PowerUpManager.instance.ActivatePowerUp(other.gameObject);
            if (AudioManager.instance != null)   AudioManager.instance.PlayPowerUpSFX();
        }
    }
}
