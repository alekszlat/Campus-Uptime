using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class IdleState<T> : GroundedState<T>where T:Character<T>
{
    protected TimerUtil idleStateTimer = new TimerUtil(0.01f, true);//used to make a clean transition between states
    protected Rigidbody2D rb;
    protected Vector2 lookDir;
    public override void Enter(T stateController)
    {
        rb = stateController.GetComponent<Rigidbody2D>();
        base.Enter(stateController);
    }

    public override void Exit(T stateController)
    {
        base.Exit(stateController);
       
    }

    public override void FixedUpdate(T stateController)
    {
       base.FixedUpdate(stateController);
       rb.linearVelocity = Vector2.zero;
    }
    public void idleAnimationLogic(T stateController)
    {
        if (lookDir.x > 0.01f)
        {
            currentAnimation = stateController.GetHorizontalIdleAnim();
        }
        else if (lookDir.x < -0.01f)
        {
            currentAnimation = stateController.GetHorizontalIdleAnim();
        }
        else if (lookDir.y > 0.01f)
        {
            currentAnimation = stateController.GetUpIdleAnim();
        }
        else if (lookDir.y < -0.01f)
        {
            currentAnimation = stateController.GetDownIdleAnim();
        }
        else
        {
            currentAnimation = stateController.GetHorizontalIdleAnim();
        }

        //if the animation is diffrent from the last one we switch and save the new one as old animation
        if (currentAnimation != oldAnimation)
        {
            oldAnimation = currentAnimation;
            playerAnimator.CrossFade(currentAnimation, 0.1f);
        }
    }
    public override void Update(T stateController)
    {
        lookDir = stateController.GetLookDir();//Direction the player is looking at
     
 
        base.Update(stateController);
      
        //Функция ,която гледа кога можем да минем към друг стейт
        //за всеки клас Player NPC ще е различна
        transitionState(stateController);
        //Animation logic      
        idleAnimationLogic(stateController);


    }

    public override void transitionState(T stateController)
    {
        bool hasStartedMoving = stateController.GetInputMoveDir() != Vector2.zero;
        if (hasStartedMoving && idleStateTimer.UpdateTimer(Time.deltaTime))
        {
            stateController.SwitchState(stateController.GetMovingState(), stateController);
        }
    }
}
