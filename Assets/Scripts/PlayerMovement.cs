using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum LaneStates { Left, Middle, Right }
public enum CharacterType { Humanoid, Capsule, Ball }

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 5.0f;
    [SerializeField] private float laneDistance = 2.5f;
    [SerializeField] private float horizontalSmoothSpeed = 15f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpHeight = 6.0f;
    [SerializeField] private float rollDuration = 1.0f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float rollHeight = 1.5f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float deathForce = 300f;
    [SerializeField]private Vector3 deathForceDirection = Vector3.up;
    [SerializeField] private Vector3 rollVisualScale = new Vector3(1f, 0.5f, 1f);

    [Header("Mobile Controls")]
    [SerializeField] private float swipeDeadZone = 100f;
    private Vector2 touchStartPos;
    private Dictionary<int, Vector2> swipeStartPositions = new Dictionary<int, Vector2>();
    private HashSet<int> swipeDetected = new HashSet<int>();


    private CharacterController characterController;
    private RagdollController ragdollController;
    private Vector3 velocity;
    private float originalHeight;
    private Vector3 originalCenter;
    private float rollTimer = 0f;
    private bool isRolling = false;
    private float targetX = 0f;
    public LaneStates currentLane = LaneStates.Middle;
    public CharacterType currentType = CharacterType.Capsule;
    private LaneStates previousLane = LaneStates.Middle;
    private bool StartGameInput = false;
    private bool isRunning = false;
    private bool isGrounded = false;
    private bool jumpLaunched = false;
    private bool isStumbling = false;
    private bool isDead = false;
    private float xVelocity = 0f;
    private bool isParkour = false;
    [HideInInspector]
    public bool IsParkour => isParkour;

    private float stumbleImmunityTimer = 0f;
    private const float StumbleImmunityDuration = 1.0f; // seconds, tweak as needed



    // Animation state names
    private Animator animationsManager;
    private readonly int Run = Animator.StringToHash("running");
    private readonly int Jump = Animator.StringToHash("jump");
    private readonly int Fall = Animator.StringToHash("falling");
    private readonly int Land = Animator.StringToHash("landing");
    private readonly int Roll = Animator.StringToHash("roll");
    private readonly int DodgeLeft = Animator.StringToHash("dodgeLeft");
    private readonly int DodgeRight = Animator.StringToHash("dodgeRight");
    private readonly int Stumble = Animator.StringToHash("stumble");
    private readonly int StumbleSideLeft = Animator.StringToHash("stumbleSideLeft");
    private readonly int StumbleSideRight = Animator.StringToHash("stumbleSideRight");
    private readonly int Death = Animator.StringToHash("death");

    [Space(10)]

    // Audio clips
    [Header("Audio Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private AudioClip dodgeSound;
    [SerializeField] private AudioClip stumbleSound;
    [SerializeField] private AudioClip deathSound;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animationsManager = GetComponent<Animator>();
        ragdollController = GetComponent<RagdollController>();
        originalHeight = characterController.height;
        originalCenter = characterController.center;
    }

    void Update()
    {
        isGrounded = characterController.isGrounded;

        if (isDead) return;
        if (!isRunning) return;

        // Prevent movement math when paused
        if (Time.deltaTime <= 0f) return;

        #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            if (Input.touchSupported && Application.isMobilePlatform)
                HandleMobileInput();
            else
                HandleKeyboardInput();
        #elif UNITY_IOS || UNITY_ANDROID
            HandleMobileInput();
        #endif
        HandleLane();
        HandleJump();
        HandleRoll();

        Vector3 move = new Vector3(GetHorizontalMove(), velocity.y, forwardSpeed);
        characterController.Move(move * Time.deltaTime);

        if (stumbleImmunityTimer > 0f)
            stumbleImmunityTimer -= Time.deltaTime;
    }

    #region Input Systems
      
    void HandleKeyboardInput()
    {
        if (isDead) return;


        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            MoveLane(-1);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            MoveLane(1);

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            TryJump();
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            TryRoll();
    }

    void HandleMobileInput()
    {
        if (isDead) return;
        float screenDPI = Screen.dpi > 0 ? Screen.dpi : 160f;
        float dynamicDeadZone = Mathf.Max(swipeDeadZone, screenDPI * 0.25f); // ~0.25 inch swipe

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            int fingerId = touch.fingerId;

            if (touch.phase == TouchPhase.Began)
            {
                swipeStartPositions[fingerId] = touch.position;
                swipeDetected.Remove(fingerId);
            }
            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended) && !swipeDetected.Contains(fingerId))
            {
                Vector2 swipeDelta = touch.position - swipeStartPositions[fingerId];

                // Use the axis with the largest movement
                if (swipeDelta.magnitude > dynamicDeadZone)
                {
                    if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                    {
                        MoveLane(swipeDelta.x > 0 ? 1 : -1);
                    }
                    else
                    {
                        if (swipeDelta.y > 0) TryJump();
                        else TryRoll();
                    }
                    swipeDetected.Add(fingerId);
                }
            }
            // Clean up on finger up
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                swipeStartPositions.Remove(fingerId);
                swipeDetected.Remove(fingerId);
            }
        }
        
    }
    public void MoveLane(int direction)
    {
        LaneStates oldLane = currentLane;

        if (direction > 0)
        {
            if (currentLane == LaneStates.Middle) currentLane = LaneStates.Right;
            else if (currentLane == LaneStates.Left) currentLane = LaneStates.Middle;
        }
        else
        {
            if (currentLane == LaneStates.Middle) currentLane = LaneStates.Left;
            else if (currentLane == LaneStates.Right) currentLane = LaneStates.Middle;
        }
        if (!isRolling && isGrounded && !isStumbling && !isDead)
        {
            if (oldLane != currentLane)
            {
                int dodgeAnim = direction > 0 ? DodgeRight : DodgeLeft;
                animationsManager.CrossFade(dodgeAnim, 0f);
            }
        }
        previousLane = oldLane; // for RevertToPreviousLane


    }

    private void RevertToPreviousLane()
    {
        if (currentLane != previousLane)
        {
            currentLane = previousLane;
            // Snap position instantly
            Vector3 pos = transform.position;
            if (currentLane == LaneStates.Left) pos.x = -laneDistance;
            else if (currentLane == LaneStates.Middle) pos.x = 0f;
            else if (currentLane == LaneStates.Right) pos.x = laneDistance;
            transform.position = pos;
            Debug.Log("↩️ Reverting to previous lane due to stumble or death!");
        }
    }

    private IEnumerator HandleStumble()
    {
        isStumbling = true;
        stumbleImmunityTimer = StumbleImmunityDuration; // Start immunity
        float originalSpeed = forwardSpeed;
        forwardSpeed *= 0.5f;

        RevertToPreviousLane();

      if (CinemachineShake.Instance != null)
          CinemachineShake.Instance.ShakeCamera(1f, 0.3f);
        #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
        #endif

        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(0.2f);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        yield return new WaitForSeconds(2.3f);

        forwardSpeed = originalSpeed;
        isStumbling = false;
    }

    public void TryRoll()
    {
        if (!isRolling)
        {
            if (!isGrounded)
            {
                StartCoroutine(PlayRollAfterJump());
            }
            else
            {
                StartRoll();
            }
        }
    }

    private IEnumerator PlayRollAfterJump()
    {
        yield return null;
        velocity.y = -30f;
        StartRoll();
    }

    float GetHorizontalMove()
    {
        float targetPos = targetX;
        float currentX = transform.position.x;
        float smoothedX = Mathf.SmoothDamp(currentX, targetPos, ref xVelocity, 0.05f);

        // Prevent division by zero (which causes NaN)
        if (Time.deltaTime <= 0f)
            return 0f;

        return (smoothedX - currentX) / Time.deltaTime;
    }
    public void StartRunning()
    {
        isRunning = true;
        if (animationsManager != null)
            animationsManager.CrossFade(Run, 0.15f);
    }
    void HandleLane()
    {
        if (currentLane == LaneStates.Left) targetX = -laneDistance;
        else if (currentLane == LaneStates.Middle) targetX = 0f;
        else if (currentLane == LaneStates.Right) targetX = laneDistance;
    }

    public void TryJump()
    {
        if (isGrounded)
        {
            if (isRolling)
            {
                EndRoll();
                StartCoroutine(PlayJumpAfterRoll());
            }
            if (isStumbling)
            {
                return;
                
            }
            else
            {
                PlayJump();
            }
        }
    }

    private IEnumerator PlayJumpAfterRoll()
    {
        yield return null;
        PlayJump();
    }

    private void PlayJump()
    {
        SoundFXManager.Instance.PlaySoundEffect(jumpSound, transform, 1f);
        isParkour = false; // Reset parkour state
        animationsManager.CrossFade(Jump, 0.15f);
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
        void HandleJump()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (isRolling)
        {
            velocity.y += gravity * Time.deltaTime;
            return;
        }
        else
        {
            if (!isGrounded)
            {
                jumpLaunched = true;

                if (velocity.y < 0)
                {
                    velocity.y += gravity * fallMultiplier * Time.deltaTime;
                    animationsManager.CrossFade(Fall, 0.12f);
                }
                else
                {
                    velocity.y += gravity * lowJumpMultiplier * Time.deltaTime;
                }
            }
            else
            {
                if (jumpLaunched)
                {
                    animationsManager.CrossFade(Land, 0.12f);
                    velocity.y += gravity * Time.deltaTime;
                    jumpLaunched = false;
                }
            }
        }
    }

    void HandleRoll()
    {
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f) EndRoll();
        }
    }
    #endregion

    #region Roll Mechanics
    void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;
        SoundFXManager.Instance.PlaySoundEffect(rollSound, transform, 1f);
        animationsManager.CrossFade(Roll, 0.15f);
        characterController.height = rollHeight;
        characterController.center = new Vector3(0, rollHeight / 2f, 0);
    }

    void EndRoll()
    {
        isRolling = false;
        animationsManager.CrossFade(Run, 0.15f);
        characterController.height = originalHeight;
        characterController.center = originalCenter;
    }
    #endregion

    #region HandleCollision

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 normal = hit.normal;
        float hitFront = Vector3.Dot(Vector3.forward, normal);
        float hitTop = Vector3.Dot(Vector3.up, normal);
        float hitRight = Vector3.Dot(Vector3.right, normal);

        if (hit.gameObject.tag == "Train")
        {
            if (hitFront < -0.7f)
            {
                Debug.Log("Hit front of train, player is dead!");
                HandleDeath();
                return;

            }
            if (hitRight > 0.7f || hitRight < -0.7f)
            {
                if (isStumbling && stumbleImmunityTimer <= 0f)
                {
                    Debug.Log("Hit side, player is dead!");
                    HandleDeath();
                    return;
                }
                else if (!isStumbling && stumbleImmunityTimer <= 0f)
                {
                    Debug.Log("Hit side, player is stumbling!");
                    animationsManager.CrossFade(Stumble, 0.15f);
                    StartCoroutine(HandleStumble());
                    return;
                }
                // If immunity is active, ignore further side hits
            }
            if (hitTop > 0.7f)
            {
                Debug.Log("Hit top of train, player is fine!");
            }
            else
                return;

        }
        if (hit.gameObject.tag == "Obstacle")
        {

            if (hitFront < -0.7f)
            {
                Debug.Log("Hit front of Obstacle, player is dead!");
                HandleDeath();
                return;

            } if (hitRight > 0.7f || hitRight < -0.7f)
            {
                if (isStumbling)
                {
                    Debug.Log("Hit side of Obstacle, player is dead!");
                    HandleDeath();
                    return;
                }
                else
                {
                    Debug.Log("Hit side of Obstacle, player is stumbling!");
                    animationsManager.CrossFade(Stumble, 0.15f);
                    StartCoroutine(HandleStumble());
                    return;
                }
                
            }
            else
            {
                Debug.Log("Hit somwhere but it's fine");
            }
        }



    }
    void HandleDeath()
    {
        if (isDead) return;

        isDead = true;
       velocity = Vector3.zero; // Stop all movement
        if (CinemachineShake.Instance != null)
          CinemachineShake.Instance.ShakeCamera(1f, 0.3f);
        #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
        #endif

        ragdollController.Die(deathForceDirection, deathForce); // Trigger ragdoll death
        SoundFXManager.Instance.PlaySoundEffect(deathSound, transform, 1f);
        characterController.enabled = false; // Disable character controller to prevent further movement
        StartCoroutine(GameManager.Instance.HandleDeathSequence());
    }
    #endregion
}
