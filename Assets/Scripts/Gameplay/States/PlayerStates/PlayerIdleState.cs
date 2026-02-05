using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class PlayerIdleState : GroundedState
{
    TimerUtil idleStateTimer = new TimerUtil(0.01f, true);//used to make a clean transition between states
    Rigidbody2D rb;
    Vector2 lookDir;
    public override void Enter(Player stateController)
    {
        rb = stateController.GetComponent<Rigidbody2D>();
        base.Enter(stateController);
        
        Debug.Log("idleState");

    }

    public override void Exit(Player stateController)
    {
        base.Exit(stateController);
       
    }

    public override void FixedUpdate(Player stateController)
    {
       base.FixedUpdate(stateController);
       rb.linearVelocity = Vector2.zero;
    }
    public void idleAnimationLogic(Player stateController)
    {
        if (lookDir.x > 0.01f)
        {
            currentAnimation = stateController.horizontalIdle;
        }
        else if (lookDir.x < -0.01f)
        {
            currentAnimation = stateController.horizontalIdle;
        }
        else if (lookDir.y > 0.01f)
        {
            currentAnimation = stateController.UpIdle;
        }
        else if (lookDir.y < -0.01f)
        {
            currentAnimation = stateController.DownIdle;
        }
        else
        {
            currentAnimation = stateController.horizontalIdle;
        }

        //if the animation is diffrent from the last one we switch and save the new one as old animation
        if (currentAnimation != oldAnimation)
        {
            oldAnimation = currentAnimation;
            playerAnimator.CrossFade(currentAnimation, 0.1f);
        }
    }
    public override void Update(Player stateController)
    {
        lookDir = stateController.GetLookDir();//Direction the player is looking at
     
 
        base.Update(stateController);

        //Transition to movmentState
        bool hasStartedMoving = stateController.GetInputMoveDir() != Vector2.zero;
        if (hasStartedMoving && idleStateTimer.UpdateTimer(Time.deltaTime))
        {
            stateController.SwitchState(stateController.playerMovingState, stateController);
        }

        //Animation logic      
        idleAnimationLogic(stateController);


    }

}
