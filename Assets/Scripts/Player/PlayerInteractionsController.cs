using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controla las interacciones del jugador con los elementos del entorno
/// </summary>
public class PlayerInteractionsController : MonoBehaviour, IInteractable
{
	[SerializeField] private List<IInteractable> interactablesInRange = new List<IInteractable>();

	private PlayerInputHandler inputHandler;

	[SerializeField] private Transform pickableElementSlot;

	private bool isPickingUpObject = false;
	private IPickable interactableInHand;

	private void Awake()
	{
		inputHandler = GetComponent<PlayerInputHandler>();
	}

	private void OnEnable()
	{
		if(inputHandler)
		{
			inputHandler.OnInteract += TryInteraction;
		}
	}

	private void OnDisable()
	{
		if (inputHandler)
		{
			inputHandler.OnInteract -= TryInteraction;
		}
	}

	private void TryInteraction()
	{
		Debug.Log("El jugador ha intentado realizar una interaccion");
		if(interactablesInRange.Count > 0)
		{
			Interact();
		}
		else
		{
			Debug.Log("No hay elementos con los que interactuar");
		}
	}


	public void Interact()
	{
		if(isPickingUpObject && interactableInHand != null)
		{
			interactableInHand.Drop();
			interactableInHand = null;
			isPickingUpObject = false;
		}
		else
		{
			IInteractable closestInteractable = interactablesInRange.OrderBy(interactable => Vector3.Distance(this.transform.position, ((MonoBehaviour)interactable).transform.position)).FirstOrDefault();

			if (closestInteractable != null)
			{
				if (closestInteractable as IPickable != null)
				{
					(closestInteractable as IPickable).PickUp(pickableElementSlot);
					isPickingUpObject = true;
					interactableInHand = (closestInteractable as IPickable);
				}
			}
		}
		
	}

	private void OnTriggerEnter(Collider other)
	{
		IInteractable interactable = other.GetComponent<IInteractable>();
		
		if(interactable != null && !interactablesInRange.Contains(interactable))
		{
			interactablesInRange.Add(interactable);
			Debug.Log($"{interactable} añadido a la lista de interactuables en rango");
		}
	}

	private void OnTriggerExit(Collider other)
	{
		IInteractable interactable = other.GetComponent<IInteractable>();

		if (interactable != null && interactablesInRange.Contains(interactable))
		{
			interactablesInRange.Remove(interactable);
			Debug.Log($"{interactable} eliminado de la lista de interactuables en rango");
		}
	}
}
