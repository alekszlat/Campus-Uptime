using UnityEngine;

public abstract class BaseState<T> : State<T>
where T : Character<T>
{
    protected Animator playerAnimator;
    protected SpriteRenderer spriteRenderer;

    protected int oldAnimation = -1;//used to check if an animation has already started
    protected int currentAnimation = -1;//used to check current an animation has already started
    public virtual void Enter(T stateController)
    {
        playerAnimator = stateController.GetAnimator();
        spriteRenderer = stateController.GetComponent<SpriteRenderer>();

        oldAnimation = -1;
        currentAnimation = -1;
    }
 

    public abstract void Exit(T stateController);
 
    public abstract void FixedUpdate(T stateController);
   
    public abstract void Update(T stateController);
   
    public virtual void transitionState(T stateController)
    {

    }
}
