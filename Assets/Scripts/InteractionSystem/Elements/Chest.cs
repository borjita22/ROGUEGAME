using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : InteractableEntity
{
	private Animator animatorController;

	[Header("Key interaction")]
	[SerializeField] public bool isKeyRequired;
	[SerializeField] public KeyType requiredKey;
	
	private void Awake()
	{
		animatorController = GetComponent<Animator>();
	}

	public override InteractionResult Interact(PlayerInteractionsController controller = null)
	{
		if (isKeyRequired && controller)
		{
			if(controller.interactableInHand == null || !(controller.interactableInHand is ChestKey))
			{
				return InteractionResult.None;
			}
		}


		if(animatorController)
		{
			animatorController.SetTrigger("Interact");
		}


		return base.Interact(controller);
	}

	public override void ReceiveInteraction(IInteractable from)
	{

		if(isKeyRequired)
		{
			Interact();

			//Se debe destruir la llave si el cofre interactua con ella
			GameObject key = ((MonoBehaviour)from).gameObject;

			if(key)
			{
				key.SetActive(false);
			}
		}
		
	}
}

public enum KeyType
{
	Silver,
	Gold
}




