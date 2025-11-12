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

    [SerializeField] private GameObject name;
    private Animator anim;
    private BasePlayer basePlayer;

    [Header("Component References")]
    [SerializeField] private SpriteRenderer visualSpriteRenderer;

    private Action<PlayerStateMachine> changeAnimationAction;

    void Start()
    {
        this.anim = this.GetComponent<Animator>();
        this.basePlayer = this.GetComponent<BasePlayer>();

        changeAnimationAction = (stateMachine) =>
        {
            if (stateMachine.Player.PlayerID == this.basePlayer.PlayerID)
            {
                this.ChangeAnimation(stateMachine);
            }
        };

        if (basePlayer.PlayerID == 1)
        {
            visualSpriteRenderer.color = GameData.Instance.player1Color;
        }
        else
        {
            visualSpriteRenderer.color = GameData.Instance.player2Color;
        }

        EventBus.On(GameEvent.PlayerChangeState, changeAnimationAction);
    }

    void Update()
    {
        Vector3 scale = name.transform.localScale;

        if (basePlayer.IsFacingRight)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x); 
        }

        name.transform.localScale = scale;
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
        if (changeAnimationAction != null)
        {
            EventBus.Off(GameEvent.PlayerChangeState, changeAnimationAction);
        }
    }


}