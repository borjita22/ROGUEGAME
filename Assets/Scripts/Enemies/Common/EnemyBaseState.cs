using UnityEngine;

public abstract class EnemyBaseState
{
	protected EnemyController enemyController;

	public EnemyBaseState(EnemyController controller)
	{
		this.enemyController = controller;
	}

	//Metodos virtuales que van a ser redefinidos por los estados concretos
	public virtual void EnterState() { }

	public virtual void UpdateState() { }

	public virtual void FixedUpdateState() { }

	public virtual void ExitState() { }


}
