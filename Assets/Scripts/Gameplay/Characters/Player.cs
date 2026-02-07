using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Character<Player>
{
    //Monobehavior class that keeps track of the player input
    private PlayerInputService playerInputHandler;//we put the playerInputService in the inspector and access it from here
    
    //Diffrent player states
    public PlayerIdleState playerIdleState;
    public PlayerMovingState playerMovingState;
    public PlayerDialogueState playerDialogueState;
    public Vector2 rawInput;
    //Animation
    public readonly int horizontalMovement = Animator.StringToHash("MoveHorizontal");
    public readonly int UpMovement = Animator.StringToHash("MoveUp");
    public readonly int DownMovement = Animator.StringToHash("MoveDown");
    public readonly int horizontalIdle = Animator.StringToHash("IdleHorizontal");
    public readonly int UpIdle = Animator.StringToHash("IdleUp");
    public readonly int DownIdle = Animator.StringToHash("IdleDown");

    //Parameters
    private int playerSpeed = 6;
  
    public override void Start()
    {
        base.Start();

        playerIdleState = new PlayerIdleState();
        playerMovingState = new PlayerMovingState();
        playerDialogueState = new PlayerDialogueState();
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
  
    }
  
}
