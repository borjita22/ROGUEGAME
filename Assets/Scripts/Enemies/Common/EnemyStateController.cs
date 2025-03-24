using System.Collections.Generic;
using UnityEngine;


//Controlador de los estados de los enemigos, con funcionalidad para añadir estados
//cambiar entre estados y ejecutar los estados que toquen en cada momento
public class EnemyStateController : MonoBehaviour
{

    private Dictionary<EnemyState, EnemyBaseState> states = new Dictionary<EnemyState, EnemyBaseState>();
    private EnemyBaseState currentState;


    public void AddState(EnemyState stateType, EnemyBaseState state)
	{
        states[stateType] = state;
	}

    public void ChangeState(EnemyState newState)
	{
        if(currentState != null)
		{
			currentState.ExitState();
		}

		currentState = states[newState];
		currentState.EnterState();
	}

	private void Update()
	{
		currentState?.UpdateState();
		Debug.Log("Updating state " + currentState);
	}

	private void FixedUpdate()
	{
		currentState?.FixedUpdateState();
	}
}
