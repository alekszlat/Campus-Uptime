/// Created ?
/// Updated: 07/02/2026
/// Added initialization check for edge cases bug when Start runs after Update.
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Character<T> : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 inputMoveDir;//direction currently moving towords,contrlled by input
    private Vector2 lookDir;//last direction we were moving in
    private float speed;
    private State<T> currentState;//needs a template for each class that extends character: state for Player,for passiveNPC,for agressiveNpc
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isInitialized = false;
  
    //is virtual so we can intialize rb
    public virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lookDir = Vector2.left;
        animator = GetComponent<Animator>();
    }
  
    public virtual void Update()
    {
        
    }

    //switches between states, it needs a new state,state needs its controller as a paramater so it can acess it, and we need the controller
    //when we switch states, we do the Exit function on the current one, switch to the new one and start the Enter function.
    
    public void SwitchState(State<T> state, T stateController)
    {
        currentState.Exit(stateController);
        currentState = state;
        currentState.Enter(stateController);
    }
    public void SetInitialState(State<T> state)
    {
        if (!isInitialized)
        {
            currentState = state;
            isInitialized = true;
        }
    }

    [Conditional("UNITY_EDITOR")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureStateInitialization()
    {
        if (!isInitialized) {
            UnityEngine.Debug.LogWarning($"State not set on {gameObject.name}, skipping Update()");
            return;
        }
    }
    //These areused to Switch the current state witouth breaking capsulation
    public void UpdateCurrentState(T stateController)
    {
        EnsureStateInitialization();
        currentState.Update(stateController); 
    }
    public void EnterCurrentState(T stateController)
    {
        EnsureStateInitialization();
        currentState.Enter(stateController);
    }
    public void FixedUpdateCurrentState(T stateController)
    {
        EnsureStateInitialization();
        currentState.FixedUpdate(stateController);
    }
    public void ExitCurrentState(T stateController)
    {
        EnsureStateInitialization();
        currentState.Exit(stateController);
    }

    //Helper functions
    public void Movement()
    {
        if (inputMoveDir != Vector2.zero)
        {
            lookDir = inputMoveDir;
        }
        rb.linearVelocity = inputMoveDir * speed;
    }

    //Geters and Setters
    public Animator GetAnimator()
    {
        return animator;
    }
    public State<T> getCurrentState()
    {
        return currentState;
    }
    public Vector2 GetInputMoveDir()
    {
        return inputMoveDir;
    }
    public void SetInputMoveDir(Vector2 moveDir)
    {
        this.inputMoveDir = moveDir;
    }
    public void SetSpeed(float speed)
    {
        this.speed=speed;
    }
    public void setSprite(Sprite currentSprite)
    {
        spriteRenderer.sprite = currentSprite;
    }
 
    public Vector2 GetLookDir()
    {
        return lookDir;
    }

}
