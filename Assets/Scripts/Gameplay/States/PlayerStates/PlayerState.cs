using UnityEngine;

public abstract class PlayerState : State<Player>
{
    protected Animator playerAnimator;
    protected SpriteRenderer spriteRenderer;

    protected int oldAnimation = -1;//used to check if an animation has already started
    protected int currentAnimation = -1;//used to check current an animation has already started
    public virtual void Enter(Player stateController)
    {
        playerAnimator = stateController.GetAnimator();
        spriteRenderer = stateController.GetComponent<SpriteRenderer>();

        oldAnimation = -1;
        currentAnimation = -1;
    }
 

    public abstract void Exit(Player stateController);
 
    public abstract void FixedUpdate(Player stateController);
   
    public abstract void Update(Player stateController);
   
    public virtual void transitionState(Player stateController)
    {

    }
}
