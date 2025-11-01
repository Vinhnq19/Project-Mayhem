using System;
using System.Collections;
using System.Collections.Generic;
using ProjectMayhem.Player;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAnimation : MonoBehaviour
{

    private readonly string IS_RUNNING = "IsRunning";
    private readonly string IS_JUMPING = "IsJumping";

    private readonly string IS_FALLING = "IsFalling";
    private readonly string IS_LANDING = "IsLanding";

    private Animator anim;

    public SpriteRenderer Visual { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        this.anim = this.GetComponent<Animator>();
        this.Visual = this.GetComponent<SpriteRenderer>();

        EventBus.On(GameEvent.PlayerChangeState, (Action<PlayerStateMachine>)((stateMachine) =>
        {
            this.ChangeAnimation(stateMachine);
        }));
    }

    private void ChangeAnimation(PlayerStateMachine stateMachine)
    {
        var current = stateMachine.CurrentState;
        var prev = stateMachine.PreviousState;

        anim.SetBool(IS_RUNNING, current is RunState);
        anim.SetBool(IS_JUMPING, current is JumpState);
        anim.SetBool(IS_FALLING, current is FallState);
        anim.SetBool(IS_LANDING, prev is FallState && current is not JumpState);
    }

    void OnDestroy()
    {
        
    }
}
