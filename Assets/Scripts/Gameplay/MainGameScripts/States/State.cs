using UnityEngine;

//The template tells witch Character the state has access to.
public interface State<T>
{
    void Enter(T stateController);
    void Update(T stateController);
    void FixedUpdate(T stateController);
    void Exit(T stateController);
   
}
