// ============================================================
//  PlayerController.cs
//  Attach to: Max (Player GameObject)
//  Handles: Lane switching, jumping, sliding, collision response
// ============================================================

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ---- Lane Settings ----
    private int currentLane = 1;                           // 0 = Left, 1 = Centre, 2 = Right
    private float[] lanePositions = { -2f, 0f, 2f };  // World X positions of each lane (2m width per GDD)
    public float laneChangeSpeed = 10f;                    // How fast the player slides to new lane

    // ---- Jump Settings ----
    public float jumpForce = 7f;
    private bool isGrounded = true;

    // ---- Slide Settings ----
    private bool isSliding = false;
    private float normalColliderHeight = 1.8f;
    private float slideColliderHeight  = 0.6f;

    // ---- Components ----
    private Rigidbody rb;
    private CapsuleCollider capsuleCol;
    private float fixedZ;

    // ----------------------------------------------------------
    void Start()
    {
        rb         = GetComponent<Rigidbody>();
        capsuleCol = GetComponent<CapsuleCollider>();
        fixedZ     = transform.position.z;

        // Auto-configure Rigidbody constraints to lock Z position and all rotations
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionZ |
                             RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY |
                             RigidbodyConstraints.FreezeRotationZ;
        }

        // Create and apply a frictionless PhysicMaterial to prevent physics launches on moving platforms
        PhysicsMaterial frictionlessMat = new PhysicsMaterial("FrictionlessPlayer");
        frictionlessMat.dynamicFriction = 0f;
        frictionlessMat.staticFriction = 0f;
        frictionlessMat.frictionCombine = PhysicsMaterialCombine.Minimum;
        frictionlessMat.bounciness = 0f;
        frictionlessMat.bounceCombine = PhysicsMaterialCombine.Minimum;

        if (capsuleCol != null)
        {
            capsuleCol.sharedMaterial = frictionlessMat;
        }

        // Play gameplay background music
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGameplayMusic();
    }

    void Update()
    {
        // Only accept input while game is running
        if (GameManager.instance != null && !GameManager.instance.isPlaying) return;

        HandleKeyboardInput();
        SmoothMoveToLane();
    }

    void FixedUpdate()
    {
        // Apply extra gravity on descent to prevent floaty landings (2.5x total gravity per GDD)
        if (rb != null && !isGrounded && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * 1.5f * Time.fixedDeltaTime;
        }
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

    // Called by SwipeDetector for touch input, and by keyboard above
    public void ShiftLane(int direction)
    {
        currentLane = Mathf.Clamp(currentLane + direction, 0, 2);
    }

    // ----------------------------------------------------------
    //  MOVEMENT
    // ----------------------------------------------------------
    void SmoothMoveToLane()
    {
        float targetX    = lanePositions[currentLane];
        Vector3 targetPos = new Vector3(targetX, transform.position.y, fixedZ);
        Vector3 nextPos   = Vector3.Lerp(transform.position, targetPos,
                                         laneChangeSpeed * Time.deltaTime);
        
        if (rb != null)
        {
            rb.MovePosition(nextPos);
        }
        else
        {
            transform.position = nextPos;
        }
    }

    // ----------------------------------------------------------
    //  JUMP
    // ----------------------------------------------------------
    public void Jump()
    {
        if (isSliding)
        {
            StopSlide(); // Cancel slide early per GDD
        }

        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // ----------------------------------------------------------
    //  SLIDE
    // ----------------------------------------------------------
    public void Slide()
    {
        if (isGrounded)
        {
            isSliding          = true;
            capsuleCol.height  = slideColliderHeight;   // Shrink collider to duck under barriers
            CancelInvoke("StopSlide");
            Invoke("StopSlide", 0.8f);                  // Automatically stand back up after 0.8s per GDD
        }
    }

    void StopSlide()
    {
        isSliding         = false;
        capsuleCol.height = normalColliderHeight;
        CancelInvoke("StopSlide");
    }

    // ----------------------------------------------------------
    //  COLLISION
    // ----------------------------------------------------------
    void OnCollisionEnter(Collision collision)
    {
        // Landing on ground resets jump
        if (collision.gameObject.CompareTag("ground"))
            isGrounded = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Collecting a coin
        if (other.CompareTag("coin"))
        {
            if (ScoreManager.instance != null)
                ScoreManager.instance.AddCoin();
            if (AudioManager.instance != null)
                AudioManager.instance.PlayCoinSFX();
            Destroy(other.gameObject);
        }

        // Hitting an obstacle costs a life (shield absorbs first hit)
        if (other.CompareTag("obstacles"))
        {
            if (LivesManager.instance != null)
                LivesManager.instance.LoseLife();
            if (AudioManager.instance != null)
                AudioManager.instance.PlayObstacleSFX();
            Destroy(other.gameObject); // Destroy the obstacle so it doesn't trigger multiple hits!
        }

        // Picking up a power-up orb
        if (other.CompareTag("magnet")     ||
            other.CompareTag("shield")     ||
            other.CompareTag("speedboost") ||
            other.CompareTag("Multiplier"))
        {
            PowerUpManager.instance.ActivatePowerUp(other.gameObject);
            if (AudioManager.instance != null)
                AudioManager.instance.PlayPowerUpSFX();
        }
    }
}
