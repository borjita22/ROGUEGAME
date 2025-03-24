using UnityEngine;

public interface IEnemyBehavior
{
	void Initialize();
	void TakeDamage(float amount);

	void Die();

	bool isAlive { get; }
}
