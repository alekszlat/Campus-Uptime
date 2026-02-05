using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovingState : GroundedState
{
    TimerUtil movingStateTimer = new TimerUtil(0.1f, true);
    Vector2 moveDir;

    public override void Enter(Player stateController)
    {
        base.Enter(stateController);
        Debug.Log("MOVING state");
    }

    public override void Exit(Player stateController)
    {
         base.Exit(stateController);
       
    }

    public override void FixedUpdate(Player stateController)
    {
        base.FixedUpdate(stateController);
        stateController.SetInputMoveDir(stateController.GetInputMoveDir());
        stateController.Movement();

    }

    public void movementAnimation(Player stateController)
    {
        if (moveDir.x > 0.01f)
        {
            spriteRenderer.flipX = false;
            currentAnimation = stateController.horizontalMovement;
        }
        else if (moveDir.x < -0.01f)
        {
            spriteRenderer.flipX = true;
            currentAnimation = stateController.horizontalMovement;
        }
        else if (moveDir.y > 0.01f)
        {
            currentAnimation = stateController.UpMovement;
        }
        else if (moveDir.y < -0.01f)
        {
            currentAnimation = stateController.DownMovement;
        }



        //if the animation is diffrent from the last one we switch and save the new one as old animation
        if (currentAnimation != oldAnimation && currentAnimation != -1)
        {
            oldAnimation = currentAnimation;
            playerAnimator.CrossFade(currentAnimation, 0.2f);
        }
    }
   
    public override void Update(Player stateController)
    {
     
        base.Update(stateController);
        moveDir = stateController.GetInputMoveDir();//Direction the player is moving in

        //Switching player movement Animations

        SpriteRenderer spriteRenderer = stateController.GetComponent<SpriteRenderer>();
   
        //From Moving To idle
        bool hasStopped = moveDir == Vector2.zero;
        if (hasStopped && movingStateTimer.UpdateTimer(Time.deltaTime))
        {
            stateController.SwitchState(stateController.playerIdleState, stateController);
        }

        movementAnimation(stateController);



    }
}
