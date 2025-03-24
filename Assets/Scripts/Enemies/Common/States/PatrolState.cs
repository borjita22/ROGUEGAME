using System.Collections.Generic;
using UnityEngine;

public class PatrolState : EnemyBaseState
{
	private PatrolWaypointManager waypointManager;
	private List<Transform> currentWaypoints;

	[SerializeField] private int currentWaypointIndex;
	[SerializeField] private float waypointStopDuration = 2f;
	[SerializeField] private float waypointStopTimer = 0f;

	private bool isWaiting = false;


    public PatrolState(EnemyController controller) : base(controller) 
	{
		waypointManager = controller.GetComponent<PatrolWaypointManager>();

		if(waypointManager == null)
		{
			waypointManager = controller.gameObject.AddComponent<PatrolWaypointManager>();
		}
	}

	public override void EnterState()
	{
		if(currentWaypoints == null || currentWaypoints.Count == 0)
		{
			waypointManager.GenerateWaypoints(enemyController.transform);
			currentWaypoints = waypointManager.GetWaypoints();
		}

		enemyController.NavMeshAgent.isStopped = false;

		currentWaypointIndex = 0;
		SetNextWaypoint();
	}

	public override void UpdateState()
	{
		float distanceToPlayer = Vector3.Distance(enemyController.transform.position, enemyController.PlayerTransform.position);

		if(distanceToPlayer <= enemyController.DetectionRange)
		{
			//cambiamos el estado a combate
			return;
		}
		
		if(isWaiting)
		{
			waypointStopTimer += Time.deltaTime;

			if(waypointStopTimer >= waypointStopDuration)
			{
				isWaiting = false;
				SetNextWaypoint();
			}

			return;
		}

		if(!enemyController.NavMeshAgent.pathPending && enemyController.NavMeshAgent.remainingDistance <= enemyController.NavMeshAgent.stoppingDistance)
		{
			isWaiting = true;
			waypointStopTimer = 0f;

		}

	}

	public override void FixedUpdateState()
	{
		base.FixedUpdateState();
	}

	public override void ExitState()
	{
		enemyController.NavMeshAgent.isStopped = true;
	}

	private void SetNextWaypoint()
	{
		currentWaypointIndex = (currentWaypointIndex + 1) % currentWaypoints.Count;

		enemyController.NavMeshAgent.SetDestination(currentWaypoints[currentWaypointIndex].position);
		Debug.Log("Setting next waypoint to " + currentWaypoints[currentWaypointIndex]);
	}
}
