using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PatrolWaypointManager : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [SerializeField] private float minDistanceBetweenWaypoints = 5f;
    [SerializeField] private float waypointsDetectionRadius = 20f;


    public void GenerateWaypoints(Transform startPoint)
	{
        waypoints.Clear();

        waypoints.Add(startPoint);

        for(int i = 0; i < 4; i++)
		{
            Vector3 randomDirection = Random.insideUnitSphere * waypointsDetectionRadius;

            NavMeshHit hit;

            if(NavMesh.SamplePosition(randomDirection, out hit, waypointsDetectionRadius, NavMesh.AllAreas))
			{
                if(!IsWaypointTooClose(hit.position))
				{
                    GameObject waypointObj = new GameObject($"Waypoint_{i}");
                    waypointObj.transform.position = hit.position;
                    waypointObj.transform.SetParent(null);

                    waypoints.Add(waypointObj.transform);
				}
			}
		}
	}

    private bool IsWaypointTooClose(Vector3 position)
	{
        return waypoints.Any(wp => Vector3.Distance(wp.position, position) < minDistanceBetweenWaypoints);
	}


    public List<Transform> GetWaypoints() => waypoints;
}
