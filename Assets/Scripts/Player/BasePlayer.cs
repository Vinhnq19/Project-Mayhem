using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private LayerMask groundLayerMask = 1;

        [Header("Component References")]
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private PlayerEffectManager playerEffectManager;
        [SerializeField] private PlayerStateMachine playerStateMachine;

        // Components
        private Rigidbody2D rb;
        private CapsuleCollider2D capsuleCollider;
        private InputManager inputManager;

        // Movement state
        private Vector2 moveInput;
        private bool isGrounded;
        private bool jumpPressed;

        // Properties
        public int PlayerID => playerID;
        
        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public Rigidbody2D Rigidbody => rb;
        public CapsuleCollider2D Collider => capsuleCollider;
        public PlayerCombat Combat => playerCombat;
        public PlayerEffectManager EffectManager => playerEffectManager;
        public PlayerStateMachine StateMachine => playerStateMachine;
        public bool IsGrounded => isGrounded;
        public Vector2 MoveInput => moveInput;
        public bool JumpPressed => jumpPressed;

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
                }
                else if (playerID == 2)
                {
                    inputManager.OnPlayer2_Move += HandleMove;
                    inputManager.OnPlayer2_Jump += HandleJump;
                    inputManager.OnPlayer2_Shoot += HandleShoot;
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
                }
                else if (playerID == 2)
                {
                    inputManager.OnPlayer2_Move -= HandleMove;
                    inputManager.OnPlayer2_Jump -= HandleJump;
                    inputManager.OnPlayer2_Shoot -= HandleShoot;
                }
            }
        }

        private void Start()
        {
            InitializePlayer();
        }

        private void Update()
        {
            CheckGrounded();
        }

        private void FixedUpdate()
        {
            if (moveInput.magnitude > 0.1f)
            {
                Move(moveInput);
            }
        }

        private void InitializePlayer()
        {
            rb.freezeRotation = true;
            rb.gravityScale = 1f;

            Debug.Log($"[BasePlayer] Player {playerID} initialized");
        }

        public void HandleMove(Vector2 direction)
        {
            moveInput = direction;
        }

        public void HandleJump(bool isPressed)
        {
            jumpPressed = isPressed;
            
            if (isPressed && isGrounded)
            {
                Jump(jumpForce);
            }
        }

        public void HandleShoot()
        {
            // This will be handled by weapon system
            Debug.Log($"[BasePlayer] Player {playerID} shoot input received");
        }

        public virtual void Move(Vector2 velocity)
        {
            Vector2 targetVelocity = new Vector2(velocity.x * moveSpeed, rb.velocity.y);
            rb.velocity = targetVelocity;
        }

        public virtual void Jump(float force)
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, force);
                Debug.Log($"[BasePlayer] Player {playerID} jumped with force {force}");
            }
        }

        private void CheckGrounded()
        {
            Vector2 boxSize = new Vector2(capsuleCollider.size.x * 0.9f, 0.1f);
            Vector2 boxCenter = new Vector2(transform.position.x, transform.position.y - capsuleCollider.size.y / 2f);
            
            isGrounded = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayerMask);
        }

        public void SetPlayerID(int id)
        {
            playerID = id;
        }

        public Vector2 GetVelocity()
        {
            return rb.velocity;
        }

        public void SetVelocity(Vector2 velocity)
        {
            rb.velocity = velocity;
        }

        public void AddForce(Vector2 force)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        public void ResetToSpawn(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
            rb.velocity = Vector2.zero;
            Debug.Log($"[BasePlayer] Player {playerID} reset to spawn position");
        }
    }
}
