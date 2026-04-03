using UnityEngine;


public class ImmobileState<T> : IdleState<T> where T : Character<T>
{
    //стейта е абсолютно същия като idleStatе с изключението за transition между movement и idleState
    public override void transitionState(T stateController)
    {
        
    }

}
