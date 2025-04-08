using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestKey : InteractableEntity, IPickable
{
	public PickableWeight weight { get; protected set; }

	private Collider groundCollider;
	private Rigidbody rb;


	[SerializeField] private KeyType keyType;

	private void Awake()
	{
		weight = PickableWeight.Light;

		rb = GetComponent<Rigidbody>();
		groundCollider = transform.Find("Collider").GetComponent<Collider>();
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

	public override bool CanInteractWith(IInteractable other)
	{
		if(other is Chest)
		{
			Chest chest = other as Chest;

			if(chest.isKeyRequired && chest.requiredKey == keyType)
			{
				return true;
			}
		}
		return false;
		//return other is Chest && ((Chest)other).requiredKey == keyType;
	}

	public override InteractionResult InteractWith(IInteractable other)
	{
		return base.InteractWith(other);
	}

	public KeyType GetKeyType()
	{
		return keyType;
	}
}
