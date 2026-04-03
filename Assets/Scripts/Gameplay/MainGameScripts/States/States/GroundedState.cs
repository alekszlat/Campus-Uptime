using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class GroundedState<T> : BaseState<T>
where T : Character<T>
{
    public override void Enter(T stateController)
    {
        base.Enter(stateController);
    }

    public override void Exit(T stateController)
    {

    }

    public override void FixedUpdate(T stateController)
    {

    }
    public override void Update(T stateController)
    {
       

    }

   
 
}
