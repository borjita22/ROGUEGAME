using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObject : EffectableObject, IPushable
{
	[Header("Distance to detect player interaction")]
	[SerializeField] protected float interactionDistance;

	private Transform player;
	private bool interactionEnabled;
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
		EnablePush();
		return InteractionResult.PushEnabled;
	}


	private void OnTriggerExit(Collider other)
	{
		
	}
}
