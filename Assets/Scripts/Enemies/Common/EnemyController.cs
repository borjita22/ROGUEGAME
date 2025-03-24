using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controlador principal de enemigo
/// </summary>
public class EnemyController : MonoBehaviour, IEnemyBehavior
{
	[Header("Main components")]
	[SerializeField] private NavMeshAgent navMeshAgent;
	[SerializeField] private Animator animatorController;
	[SerializeField] private EnemyStateController stateController;

	//Estas propiedades de los enemigos se recibiran posteriormente de un SO que contendra los datos de los diferentes enemigos
	//Por ahora y con propositos de prueba, estas propiedades deberian servir
	[Header("Enemy properties")]
	[SerializeField] private float maxHealth;
	[SerializeField] private float currentHealth;
	[SerializeField] private float detectionRange;
	[SerializeField] private float attackRange;

	private Transform playerTransform;


	public NavMeshAgent NavMeshAgent => navMeshAgent;
	public Animator AnimatorController => animatorController;
	public Transform PlayerTransform => playerTransform;
	public float DetectionRange => detectionRange;
	public float AttackRange => attackRange;

	private void Awake()
	{
		currentHealth = maxHealth;
		playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
		stateController = GetComponent<EnemyStateController>();
		navMeshAgent = GetComponent<NavMeshAgent>();

		InitializeStates();
	}

	private void InitializeStates()
	{
		stateController.AddState(EnemyState.Patrol, new PatrolState(this));
		//stateController.AddState(EnemyState.Combat, new CombatState(this));
		//stateController.AddState(EnemyState.Attack, new AttackState(this));
		//stateController.AddState(EnemyState.Stunned, new StunnedState(this));
		//stateController.AddState(EnemyState.Retreat, new RetreatState(this));
		//stateController.AddState(EnemyState.Death, new DeathState(this));

		stateController.ChangeState(EnemyState.Patrol);
	}

	public bool isAlive => currentHealth > 0f;

	public void Die()
	{
		stateController.ChangeState(EnemyState.Death);
	}

	public void Initialize()
	{
		throw new System.NotImplementedException();
	}

	public void TakeDamage(float amount)
	{
		currentHealth -= amount;

		if(currentHealth <= 0f)
		{
			Die();
		}
		else
		{
			//aqui podriamos hacer que el enemigo cambiase a un potencial estado "Stunned"
			//stateController.ChangeState(EnemyState.Stunned);
		}
	}
}
