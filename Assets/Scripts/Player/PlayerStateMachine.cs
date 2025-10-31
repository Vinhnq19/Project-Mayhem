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
    }

    public class PlayerStateMachine : MonoBehaviour
    {
        private BasePlayer player;
        private BasePlayerState currentState;
        private BasePlayerState previousState;

        private IdleState idleState;
        private RunState runState;
        private JumpState jumpState;
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
        }
    }

    #endregion
}

