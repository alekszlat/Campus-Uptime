using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovingState<T> : GroundedState<T>where T:Character<T>
{
   protected TimerUtil movingStateTimer = new TimerUtil(0.1f, true);
   protected Vector2 moveDir;

    public override void Enter(T stateController)
    {
        base.Enter(stateController);

    }

    public override void Exit(T stateController)
    {
         base.Exit(stateController);
       
    }

    public override void FixedUpdate(T stateController)
    {
        base.FixedUpdate(stateController);
        stateController.SetInputMoveDir(stateController.GetInputMoveDir());
        stateController.Movement();

    }

    public void movementAnimation(T stateController)
    {
        if (moveDir.x > 0.01f)
        {
            spriteRenderer.flipX = false;
            currentAnimation = stateController.GetHorizontalMoveAnim();
        }
        else if (moveDir.x < -0.01f)
        {
            spriteRenderer.flipX = true;
            currentAnimation = stateController.GetHorizontalMoveAnim();
        }
        else if (moveDir.y > 0.01f)
        {
            currentAnimation = stateController.GetUpMoveAnim();
        }
        else if (moveDir.y < -0.01f)
        {
            currentAnimation = stateController.GetDownMoveAnim();
        }



        //if the animation is diffrent from the last one we switch and save the new one as old animation
        if (currentAnimation != oldAnimation && currentAnimation != -1)
        {
            oldAnimation = currentAnimation;
            playerAnimator.CrossFade(currentAnimation, 0.2f);
        }
    }

  
  
    public override void Update(T stateController)
    {
     
        base.Update(stateController);
        moveDir = stateController.GetInputMoveDir();//Direction the player is moving in

        //Switching player movement Animations

        SpriteRenderer spriteRenderer = stateController.GetComponent<SpriteRenderer>();

        //From Moving To idle
        transitionState(stateController);

        movementAnimation(stateController);

    }
    public override void transitionState(T stateController)
    {
        bool hasStopped = moveDir == Vector2.zero;
        if (hasStopped && movingStateTimer.UpdateTimer(Time.deltaTime))
        {
            stateController.SwitchState(stateController.GetIdleState(), stateController);
        }
    }
}
