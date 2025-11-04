using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ProjectMayhem.Manager;

namespace ProjectMayhem.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class BasePlayer : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private int playerID = 1;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float speedMultiplier = 1.0f;
        [SerializeField] private LayerMask groundLayerMask = 1;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 10f;

        [Header("Double Jump Settings")]
        [SerializeField] private bool enableDoubleJump = true;
        [SerializeField] private float doubleJumpForce = 10f;

        [Header("Triple Jump Settings")]
        [SerializeField] private bool enableTripleJump = false;
        [SerializeField] private float tripleJumpForce = 10f;

        [Header("Platform Drop Settings")]
        [SerializeField] private float dropDownTime = 0.5f;

        [Header("Movement Smoothing")]
        [SerializeField] private float movementSmoothingDuration = 0.2f;
        [SerializeField] private Ease movementEase = Ease.OutQuad;

        [Header("Component References")]
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private PlayerEffectManager playerEffectManager;
        [SerializeField] private PlayerStateMachine playerStateMachine;

        // Components
        private Rigidbody2D rb;
        private CapsuleCollider2D capsuleCollider;
        private InputManager inputManager;

        private ContactFilter2D platformFilter;
        private List<Collider2D> overlappingPlatformsList = new List<Collider2D>();

        // Movement state
        private Vector2 moveInput;
        private bool isGrounded;
        private bool wasGrounded;

        private bool jumpInputPressed = false;
        private bool dropDownInputPressed = false;
        private bool isShootButtonHeld = false;  // Track if shoot button is being held

        // Jump state tracking
        private int jumpCount = 0;
        private bool canTripleJump = false;

        // Platform drop state
        private bool isDroppingDown = false;
        private float dropDownTimer = 0f;
        private Collider2D currentPlatformCollider = null;

        // Horizontal movement smoothing
        private float targetVelocityX = 0f;
        private float velocityXSmoothing = 0f;

        //Movement reversed
        private bool isMovementReversed = false;

        // Properties
        public int PlayerID => playerID;

        public float MoveSpeed => moveSpeed;
        public float SpeedMultiplier => speedMultiplier;
        public float JumpForce => jumpForce;
        public Rigidbody2D Rigidbody => rb;
        public CapsuleCollider2D Collider => capsuleCollider;
        public PlayerCombat Combat => playerCombat;
        public PlayerEffectManager EffectManager => playerEffectManager;
        public PlayerStateMachine StateMachine => playerStateMachine;
        public bool IsGrounded => isGrounded;
        public Vector2 MoveInput => moveInput;
        public bool CanDoubleJump => enableDoubleJump && (jumpCount == 1);
        public bool CanTripleJump => enableTripleJump && (jumpCount == 2);
        public int JumpCount => jumpCount;
        public bool IsDroppingDown => isDroppingDown;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();

            if (playerCombat == null)
                playerCombat = GetComponent<PlayerCombat>();
            if (playerEffectManager == null)
                playerEffectManager = GetComponent<PlayerEffectManager>();
            if (playerStateMachine == null)
                playerStateMachine = GetComponent<PlayerStateMachine>();

            inputManager = InputManager.Instance;
        }

        private void OnEnable()
        {
            if (inputManager != null)
            {
                if (playerID == 1)
                {
                    inputManager.OnPlayer1_Move += HandleMove;
                    inputManager.OnPlayer1_Jump += HandleJump;
                    inputManager.OnPlayer1_Shoot += HandleShoot;
                    inputManager.OnPlayer1_Special += HandleSpecial;
                }
                else if (playerID == 2)
                {
                    inputManager.OnPlayer2_Move += HandleMove;
                    inputManager.OnPlayer2_Jump += HandleJump;
                    inputManager.OnPlayer2_Shoot += HandleShoot;
                    inputManager.OnPlayer2_Special += HandleSpecial;
                }
            }
        }

        private void OnDisable()
        {
            if (inputManager != null)
            {
                if (playerID == 1)
                {
                    inputManager.OnPlayer1_Move -= HandleMove;
                    inputManager.OnPlayer1_Jump -= HandleJump;
                    inputManager.OnPlayer1_Shoot -= HandleShoot;
                    inputManager.OnPlayer1_Special -= HandleSpecial;
                }
                else if (playerID == 2)
                {
                    inputManager.OnPlayer2_Move -= HandleMove;
                    inputManager.OnPlayer2_Jump -= HandleJump;
                    inputManager.OnPlayer2_Shoot -= HandleShoot;
                    inputManager.OnPlayer2_Special -= HandleSpecial;
                }
            }
        }

        private void Start()
        {
            InitializePlayer();

            platformFilter = new ContactFilter2D();
            platformFilter.SetLayerMask(groundLayerMask);
            platformFilter.useTriggers = false;
        }

        private void Update()
        {
            HandlePlatformCollision();

            bool wasGroundedPrevious = wasGrounded;
            CheckGrounded();
            wasGrounded = isGrounded;

            if (!wasGroundedPrevious && isGrounded)
            {
                ResetJumpCount();
            }

            HandleDropDownTimer();
            HandleActionInput();
            HandleContinuousShooting();  // Handle continuous shooting when button is held
        }

        private void LateUpdate()
        {
            jumpInputPressed = false;
            dropDownInputPressed = false;
        }

        private void FixedUpdate()
        {
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                Move(new Vector2(moveInput.x, 0f));
            }
            else
            {
                StopMovement();
            }

            float newVelX = Mathf.SmoothDamp(rb.velocity.x, targetVelocityX, ref velocityXSmoothing, movementSmoothingDuration);
            rb.velocity = new Vector2(newVelX, rb.velocity.y);
        }

        private void InitializePlayer()
        {
            rb.freezeRotation = true;
            rb.gravityScale = 3f;

            jumpCount = 0;
            canTripleJump = enableTripleJump;
            wasGrounded = false;
            isDroppingDown = false;
            dropDownTimer = 0f;

            Debug.Log($"[BasePlayer] Player {playerID} initialized - Double Jump: {enableDoubleJump}, Triple Jump: {enableTripleJump}");
        }

        public void HandleMove(Vector2 direction)
        {
            bool jumpPressed = direction.y > 0.1f;
            bool wasJumpPressed = moveInput.y > 0.1f;
            if (jumpPressed && !wasJumpPressed)
            {
                jumpInputPressed = true;
            }

            bool downPressed = direction.y < -0.1f && isGrounded;
            bool wasDownPressed = moveInput.y < -0.1f;
            if (downPressed && !wasDownPressed)
            {
                dropDownInputPressed = true;
            }

            moveInput = direction;
        }

        public void HandleJump(bool isPressed)
        {
            if (isPressed)
            {
                jumpInputPressed = true;
            }
        }

        private void HandleActionInput()
        {
            if (jumpInputPressed)
            {
                if (isGrounded && !isDroppingDown)
                {
                    Jump(jumpForce);
                    jumpCount = 1;
                    Debug.Log($"[BasePlayer] Player {playerID} normal jump executed");
                }
                else if (!isGrounded)
                {
                    if (enableDoubleJump && jumpCount == 1)
                    {
                        Jump(doubleJumpForce);
                        jumpCount = 2;
                        Debug.Log($"[BasePlayer] Player {playerID} double jump executed");
                    }
                    else if (enableTripleJump && jumpCount == 2)
                    {
                        Jump(tripleJumpForce);
                        jumpCount = 3;
                        Debug.Log($"[BasePlayer] Player {playerID} triple jump executed");
                    }
                    else
                    {
                        Debug.Log($"[BasePlayer] Player {playerID} no more jumps available - JumpCount: {jumpCount}, DoubleJump: {enableDoubleJump}, TripleJump: {enableTripleJump}");
                    }
                }
                else
                {
                    Debug.Log($"[BasePlayer] Player {playerID} jump blocked - Grounded: {isGrounded}, DroppingDown: {isDroppingDown}");
                }
            }

            if (dropDownInputPressed)
            {
                if (isGrounded)
                {
                    StartDropDown();
                    Debug.Log($"[BasePlayer] Player {playerID} drop down executed");
                }
                else
                {
                    Debug.Log($"[BasePlayer] Player {playerID} drop down blocked - not grounded");
                }
            }
        }

        private void HandleDropDownTimer()
        {
            if (isDroppingDown)
            {
                dropDownTimer -= Time.deltaTime;

                if (dropDownTimer <= 0f)
                {
                    EndDropDown();
                }
            }
        }

        private void StartDropDown()
        {
            Vector2 boxSize = new Vector2(capsuleCollider.size.x * 0.9f, 0.2f);
            Vector2 boxCenter = new Vector2(transform.position.x, transform.position.y - capsuleCollider.size.y / 2f);

            RaycastHit2D boxHit = Physics2D.BoxCast(
                boxCenter,
                boxSize,
                0f,
                Vector2.down,
                0.2f,
                groundLayerMask
            );

            if (boxHit.collider != null)
            {
                currentPlatformCollider = boxHit.collider;
                Physics2D.IgnoreCollision(capsuleCollider, currentPlatformCollider, true);
                isDroppingDown = true;
                dropDownTimer = dropDownTime;

                Debug.Log($"[BasePlayer] Player {playerID} dropping down through platform: {boxHit.collider.gameObject.name}");
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    Vector2.down,
                    capsuleCollider.size.y / 2f + 0.1f,
                    groundLayerMask
                );

                if (hit.collider != null)
                {
                    currentPlatformCollider = hit.collider;
                    Physics2D.IgnoreCollision(capsuleCollider, currentPlatformCollider, true);
                    isDroppingDown = true;
                    dropDownTimer = dropDownTime;

                    Debug.Log($"[BasePlayer] Player {playerID} dropping down through platform (raycast): {hit.collider.gameObject.name}");
                }
                else
                {
                    Debug.Log($"[BasePlayer] Player {playerID} no platform found for drop down");
                }
            }
        }

        private void EndDropDown()
        {
            if (currentPlatformCollider != null)
            {
                Physics2D.IgnoreCollision(capsuleCollider, currentPlatformCollider, false);
                currentPlatformCollider = null;
            }
            isDroppingDown = false;
            dropDownTimer = 0f;
        }

        private void HandlePlatformCollision()
        {
            if (isDroppingDown)
            {
                return;
            }

            int count = capsuleCollider.OverlapCollider(platformFilter, overlappingPlatformsList);

            for (int i = 0; i < count; i++)
            {
                Collider2D platform = overlappingPlatformsList[i];

                if (rb.velocity.y > 0.01f)
                {
                    Physics2D.IgnoreCollision(capsuleCollider, platform, true);
                }
                else
                {
                    Physics2D.IgnoreCollision(capsuleCollider, platform, false);
                }
            }
        }
        public void HandleShoot(bool isPressed)
        {
            isShootButtonHeld = isPressed;
        }

        private void HandleContinuousShooting()
        {
            if (isShootButtonHeld && playerCombat != null)
            {
                playerCombat.UseCurrentWeapon();
            }
        }

        public void HandleSpecial()
        {
            if (playerCombat != null)
            {
                playerCombat.UseSpecialBomb();
            }
            else
            {
                Debug.LogWarning($"[BasePlayer] Player {playerID} has no PlayerCombat component!");
            }
        }

        public virtual void Move(Vector2 direction)
        {
            if (isMovementReversed) direction.x = -direction.x;
            float targetX = direction.x * moveSpeed * speedMultiplier;
            targetVelocityX = targetX;
        }

        private void StopMovement()
        {
            targetVelocityX = 0f;
        }

        public virtual void Jump(float force)
        {
            rb.velocity = new Vector2(rb.velocity.x, force);
        }


        private void ResetJumpCount()
        {
            jumpCount = 0;
            EndDropDown();
        }

        public void EnableTripleJump()
        {
            enableTripleJump = true;
            Debug.Log($"[BasePlayer] Player {playerID} triple jump enabled");
        }

        public void DisableTripleJump()
        {
            enableTripleJump = false;
            if (jumpCount >= 3)
                jumpCount = 2;
            Debug.Log($"[BasePlayer] Player {playerID} triple jump disabled");
        }

        private void CheckGrounded()
        {
            if (isDroppingDown)
            {
                isGrounded = false;
                return;
            }

            Vector2 rayStart = new Vector2(transform.position.x, transform.position.y - capsuleCollider.size.y / 2f);
            float rayDistance = 0.3f;

            bool foundGround = false;
            for (int i = -1; i <= 1; i++)
            {
                Vector2 multiRayStart = new Vector2(transform.position.x + i * capsuleCollider.size.x * 0.3f, transform.position.y - capsuleCollider.size.y / 2f);
                RaycastHit2D hit = Physics2D.Raycast(multiRayStart, Vector2.down, rayDistance, groundLayerMask);

                if (hit.collider != null)
                {
                    foundGround = true;
                    break;
                }
            }

            if (!foundGround)
            {
                Vector2 boxSize = new Vector2(capsuleCollider.size.x * 0.9f, 0.1f);
                Vector2 boxCenter = new Vector2(transform.position.x, transform.position.y - capsuleCollider.size.y / 2f);
                foundGround = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayerMask);
            }

            bool wasGrounded = isGrounded;
            isGrounded = foundGround;
        }

        public void SetPlayerID(int id)
        {
            playerID = id;
        }
        public Vector2 GetMoveInput()
        {
            return moveInput;
        }

        public Vector2 GetVelocity()
        {
            return rb.velocity;
        }

        public void SetVelocity(Vector2 velocity)
        {
            rb.velocity = velocity;
        }

        public void SetMoveSpeed(float newSpeed)
        {
            moveSpeed = newSpeed;
        }

        public void SetSpeedMultiplier(float newMultiplier)
        {
            speedMultiplier = newMultiplier;
        }

        public void SetMovementReversed(bool isReversed)
        {
            isMovementReversed = isReversed;
        }

        public void AddForce(Vector2 force)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        public void ResetToSpawn(Vector3 spawnPosition)
        {
            transform.DOKill();
            transform.position = spawnPosition;
            rb.velocity = Vector2.zero;

            ResetJumpCount();
            jumpCount = 0;
            canTripleJump = enableTripleJump;
            EndDropDown();

            Debug.Log($"[BasePlayer] Player {playerID} reset to spawn position");
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}