using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : EffectableObject, IPickable
{
	public PickableWeight weight { get; protected set; }


	//Esta antorcha debe de poder añadir, si se desea, una propiedad de efecto Fire desde el comienzo

	protected override void Awake()
	{
		base.Awake();
		weight = PickableWeight.Light;
	}

	public override InteractionResult Interact(PlayerInteractionsController controller)
	{
		// Si puede ser recogida, la recogemos
		PickUp(weight == PickableWeight.Heavy ?
			   controller.heavyPickableElementSlot :
			   controller.lightPickableElementSlot);

		return InteractionResult.ItemPickedUp;
	}

	public void Drop()
	{
		this.transform.SetParent(null);

		if (groundCollider)
		{
			groundCollider.enabled = true;
		}
		if (rb)
		{
			rb.isKinematic = false;
		}
	}

	public void PickUp(Transform parent)
	{
		this.transform.SetParent(parent);
		//this.transform.position = parent.position;

		this.transform.SetPositionAndRotation(parent.position, Quaternion.Euler(Vector3.zero));
		this.transform.localRotation = Quaternion.identity; //Aseguramos que no herede la rotacion del parent

		if (rb)
		{
			rb.isKinematic = true;
		}
		if (groundCollider)
		{
			groundCollider.enabled = false;
		}
	}


	protected override void OnEffectApplied(EffectType effectType)
	{
		base.OnEffectApplied(effectType);

		if(effectObject)
		{
			effectObject.transform.position = transform.Find("FirePoint").transform.position;
			//hay que hacerlo mas pequeño
			effectObject.transform.localScale *= 0.5f; //reducir la escala a la mitad para que se vea mejor el efecto
		}
	}
}
