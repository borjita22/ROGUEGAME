using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Los objetos empujables van a requerir todos un nivel de fuerza para ser empujados
public class PushableObject : EffectableObject, IPushable
{
	[Header("Distance to detect player interaction")]
	[SerializeField] protected float interactionDistance;

	private Transform player;
	private bool interactionEnabled;

	[SerializeField] protected int requiredStrength;
	protected override void Awake()
	{
		base.Awake();
		rb.isKinematic = true;

		player = GameObject.Find("Player").transform;
	}

	protected override void Update()
	{
		if(player && Vector3.Distance(player.transform.position, this.transform.position) > interactionDistance && interactionEnabled == true)
		{
			DisablePush();
		}
	}
	public void DisablePush()
	{
		if(rb)
		{
			rb.isKinematic = true;
		}
		interactionEnabled = false;
	}

	public void EnablePush()
	{
		if(rb)
		{
			rb.isKinematic = false;
		}
		interactionEnabled = true;
	}

	public override InteractionResult Interact(PlayerInteractionsController controller)
	{
		if(controller.PlayerStats.Strength >= requiredStrength)
        {
			EnablePush();
			return InteractionResult.PushEnabled;
		}
		return InteractionResult.None;
	}


	private void OnTriggerExit(Collider other)
	{
		
	}
}
