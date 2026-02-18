using Game.Core.EventSystem;
using Game.Core.GameSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Character<Player>, IEventHandler<OnFreezePlayerDuringDialogue>, IEventHandler<OnUnFreezePlayerOnDialogueEnd>
{
    //Monobehavior class that keeps track of the player input
    private PlayerInputService playerInputHandler;//we put the playerInputService in the inspector and access it from here
    
    //Diffrent player states
    public PlayerIdleState playerIdleState;
    public PlayerMovingState playerMovingState;
    public PlayerDialogueState playerDialogueState;
    public PlayerImmobileState playerImmobileState;

    //Animation
    public readonly int horizontalMovement = Animator.StringToHash("MoveHorizontal");
    public readonly int UpMovement = Animator.StringToHash("MoveUp");
    public readonly int DownMovement = Animator.StringToHash("MoveDown");
    public readonly int horizontalIdle = Animator.StringToHash("IdleHorizontal");
    public readonly int UpIdle = Animator.StringToHash("IdleUp");
    public readonly int DownIdle = Animator.StringToHash("IdleDown");
    //Parameters
    [SerializeField] private float playerSpeed = 6;

    Iinteractable interactObject=null;

    void OnEnable()
    {
        EventManager.Instance.Subscribe<OnFreezePlayerDuringDialogue, Player>(this);
        EventManager.Instance.Subscribe<OnUnFreezePlayerOnDialogueEnd, Player>(this);
    }

    void OnDisable()
    {
        EventManager.Instance.Unsubscribe<OnFreezePlayerDuringDialogue, Player>();
        EventManager.Instance.Unsubscribe<OnUnFreezePlayerOnDialogueEnd, Player>();
    }

    public override void Start()
    {
        base.Start();

        playerIdleState = new PlayerIdleState();
        playerMovingState = new PlayerMovingState();
        playerDialogueState = new PlayerDialogueState();
        playerImmobileState = new PlayerImmobileState();
        SetSpeed(playerSpeed);
        playerInputHandler = GetComponent<PlayerInputService>();
        SetInitialState(playerIdleState);
        EnterCurrentState(this);
    }
    private void FixedUpdate()
    {
      
        FixedUpdateCurrentState(this);
    }

    public override void Update()
    {
        base.Update();
        SetInputMoveDir(playerInputHandler.GetDirection());
        UpdateCurrentState(this);



        //INTERACT OBJ
        if (interactObject != null && playerInputHandler.GetIsInteracting())
        {
            interactObject.interact();
        }

    }

    //проверка дали сме в влезвли в обект с, който можем да интерактваме
    private void OnTriggerEnter2D(Collider2D collision)
    {
        testInteract interact = collision.GetComponentInParent<testInteract>();

        if (interact != null)
        {
            interactObject = interact;
        }
    }
  
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (interactObject != null)
        {
            interactObject = null;
        }
    }

    public void Handle(OnFreezePlayerDuringDialogue @event)
    {
        SwitchState(playerImmobileState,this);
    }

    public void Handle(OnUnFreezePlayerOnDialogueEnd @event)
    {
        SwitchState(playerIdleState, this);
    }
}
