using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionForwarder : MonoBehaviour
{
    public FlamethrowerController controller;

	private void Start()
	{
		controller = GetComponentInParent<FlamethrowerController>();
	}

	private void OnParticleCollision(GameObject other)
	{
		if(controller != null)
		{
			controller.OnParticleCollision(other);
		}
	}

	
}
