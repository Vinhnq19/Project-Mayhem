using UnityEngine;
using ProjectMayhem.Player;

namespace ProjectMayhem.Player
{

    public abstract class BasePlayerState
    {
        protected BasePlayer player;
        protected PlayerStateMachine stateMachine;

        public BasePlayerState(BasePlayer player, PlayerStateMachine stateMachine)
        {
            this.player = player;
            this.stateMachine = stateMachine;
        }

        public virtual void EnterState() { }

        public virtual void UpdateState() { }

        public virtual void FixedUpdateState() { }

        public virtual void ExitState() { }

        public virtual void HandleInput(PlayerInputData input) { }
    }

    public struct PlayerInputData
    {
        public Vector2 moveInput;
        public bool jumpPressed;
        public bool jumpHeld;
        public bool shootPressed;
        public bool specialPressed;
    }

    public class PlayerStateMachine : MonoBehaviour
    {
        [Header("State Settings")]
        [SerializeField] private float wallSlideSpeed = 2f;
        [SerializeField] private float wallJumpForce = 15f;
        [SerializeField] private LayerMask wallLayerMask = 1;

        private BasePlayer player;
        private BasePlayerState currentState;
        private BasePlayerState previousState;

        private IdleState idleState;
        private RunState runState;
        private JumpState jumpState;
        private WallSlideState wallSlideState;
        private FallState fallState;

        public BasePlayerState CurrentState => currentState;
        public BasePlayerState PreviousState => previousState;
        public BasePlayer Player => player;

        private void Awake()
        {
            player = GetComponent<BasePlayer>();
            InitializeStates();
        }

        private void Start()
        {
            ChangeState(idleState);
        }

        private void Update()
        {
            currentState?.UpdateState();
        }

        private void FixedUpdate()
        {
            currentState?.FixedUpdateState();
        }

        private void InitializeStates()
        {
            idleState = new IdleState(player, this);
            runState = new RunState(player, this);
            jumpState = new JumpState(player, this);
            wallSlideState = new WallSlideState(player, this);
            fallState = new FallState(player, this);
        }

        public void ChangeState(BasePlayerState newState)
        {
            if (newState == null) return;

            previousState = currentState;
            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState();

            Debug.Log($"[PlayerStateMachine] Changed from {previousState?.GetType().Name} to {currentState.GetType().Name}");
        }

        public void HandleInput(PlayerInputData input)
        {
            currentState?.HandleInput(input);
        }

        public bool IsOnWall()
        {
            Vector2 boxSize = new Vector2(0.1f, player.Collider.size.y * 0.9f);
            Vector2 boxCenter = new Vector2(transform.position.x + (player.MoveInput.x > 0 ? player.Collider.size.x / 2f : -player.Collider.size.x / 2f), transform.position.y);
            
            return Physics2D.OverlapBox(boxCenter, boxSize, 0f, wallLayerMask);
        }

        public float GetWallSlideSpeed()
        {
            return wallSlideSpeed;
        }

        public float GetWallJumpForce()
        {
            return wallJumpForce;
        }
    }

    #region Player States

    public class IdleState : BasePlayerState
    {
        public IdleState(BasePlayer player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void EnterState()
        {
            Debug.Log("[IdleState] Entered idle state");
        }

        public override void UpdateState()
        {
            if (!player.IsGrounded)
            {
                if (player.GetVelocity().y > 0)
                    stateMachine.ChangeState(new JumpState(player, stateMachine));
                else
                    stateMachine.ChangeState(new FallState(player, stateMachine));
            }
            else if (Mathf.Abs(player.MoveInput.x) > 0.1f)
            {
                stateMachine.ChangeState(new RunState(player, stateMachine));
            }
        }

        public override void HandleInput(PlayerInputData input)
        {
            if (input.jumpPressed && player.IsGrounded)
            {
                player.Jump(player.JumpForce);
                stateMachine.ChangeState(new JumpState(player, stateMachine));
            }
        }
    }

    public class RunState : BasePlayerState
    {
        public RunState(BasePlayer player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void EnterState()
        {
            Debug.Log("[RunState] Entered run state");
        }

        public override void UpdateState()
        {
            if (!player.IsGrounded)
            {
                if (player.GetVelocity().y > 0)
                    stateMachine.ChangeState(new JumpState(player, stateMachine));
                else
                    stateMachine.ChangeState(new FallState(player, stateMachine));
            }
            else if (Mathf.Abs(player.MoveInput.x) < 0.1f)
            {
                stateMachine.ChangeState(new IdleState(player, stateMachine));
            }
        }

        public override void HandleInput(PlayerInputData input)
        {
            if (input.jumpPressed && player.IsGrounded)
            {
                player.Jump(player.JumpForce);
                stateMachine.ChangeState(new JumpState(player, stateMachine));
            }
        }
    }

    public class JumpState : BasePlayerState
    {
        public JumpState(BasePlayer player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void EnterState()
        {
            Debug.Log("[JumpState] Entered jump state");
        }

        public override void UpdateState()
        {
            if (player.GetVelocity().y <= 0)
            {
                stateMachine.ChangeState(new FallState(player, stateMachine));
            }
            else if (stateMachine.IsOnWall() && Mathf.Abs(player.MoveInput.x) > 0.1f)
            {
                stateMachine.ChangeState(new WallSlideState(player, stateMachine));
            }
        }
    }

    public class FallState : BasePlayerState
    {
        public FallState(BasePlayer player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void EnterState()
        {
            Debug.Log("[FallState] Entered fall state");
        }

        public override void UpdateState()
        {
            if (player.IsGrounded)
            {
                if (Mathf.Abs(player.MoveInput.x) > 0.1f)
                    stateMachine.ChangeState(new RunState(player, stateMachine));
                else
                    stateMachine.ChangeState(new IdleState(player, stateMachine));
            }
            else if (stateMachine.IsOnWall() && Mathf.Abs(player.MoveInput.x) > 0.1f)
            {
                stateMachine.ChangeState(new WallSlideState(player, stateMachine));
            }
        }

        public override void HandleInput(PlayerInputData input)
        {
            if (input.jumpPressed && stateMachine.IsOnWall())
            {
                Vector2 wallJumpDirection = new Vector2(-player.MoveInput.x, 1f).normalized;
                player.SetVelocity(wallJumpDirection * stateMachine.GetWallJumpForce());
                stateMachine.ChangeState(new JumpState(player, stateMachine));
            }
        }
    }

    public class WallSlideState : BasePlayerState
    {
        public WallSlideState(BasePlayer player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void EnterState()
        {
            Debug.Log("[WallSlideState] Entered wall slide state");
        }

        public override void UpdateState()
        {
            Vector2 velocity = player.GetVelocity();
            velocity.y = Mathf.Max(velocity.y, -stateMachine.GetWallSlideSpeed());
            player.SetVelocity(velocity);

            if (!stateMachine.IsOnWall())
            {
                if (player.GetVelocity().y > 0)
                    stateMachine.ChangeState(new JumpState(player, stateMachine));
                else
                    stateMachine.ChangeState(new FallState(player, stateMachine));
            }
            else if (player.IsGrounded)
            {
                if (Mathf.Abs(player.MoveInput.x) > 0.1f)
                    stateMachine.ChangeState(new RunState(player, stateMachine));
                else
                    stateMachine.ChangeState(new IdleState(player, stateMachine));
            }
        }

        public override void HandleInput(PlayerInputData input)
        {
            if (input.jumpPressed)
            {
                Vector2 wallJumpDirection = new Vector2(-player.MoveInput.x, 1f).normalized;
                player.SetVelocity(wallJumpDirection * stateMachine.GetWallJumpForce());
                stateMachine.ChangeState(new JumpState(player, stateMachine));
            }
        }
    }

    #endregion
}
