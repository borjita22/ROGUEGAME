using System;
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

	[Header("Pickable slots")]
	[SerializeField] private Transform heavyPickableElementSlot;
	[SerializeField] private Transform lightPickableElementSlot;

	private bool isPickingUpObject = false;
	private IInteractable interactableInHand;

	public bool IsConsumedOnInteraction => false;

	public Action<bool> OnHeavyCarrying;
	public Action<bool> OnLightCarrying;

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
		// Si estamos sosteniendo un objeto
		if (isPickingUpObject && interactableInHand != null)
		{
			IInteractable closestInteractable = FindClosestInteractable();

			// Si hay un objeto cercano y puede interactuar con el objeto que sostenemos
			if (closestInteractable != null && interactableInHand.CanInteractWith(closestInteractable))
			{
				InteractWithHeldItem(closestInteractable);
			}
			// Si no hay objeto cercano o no puede interactuar, simplemente soltar el objeto
			else
			{
				DropHeldItem();
			}
		}
		// Si no sostenemos nada, intentar recoger o interactuar con algo cercano
		else if (interactablesInRange.Count > 0)
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
		IInteractable closestInteractable = FindClosestInteractable();

		if (closestInteractable != null && closestInteractable is IPickable pickable)
		{
			Transform elementSlot;
			if(pickable.weight == PickableWeight.Heavy)
			{
				elementSlot = heavyPickableElementSlot;
				OnHeavyCarrying(true);
			}
			else
			{
				elementSlot = lightPickableElementSlot;
				OnLightCarrying(true);
			}
			pickable.PickUp(elementSlot);
			isPickingUpObject = true;
			interactableInHand = (IInteractable)pickable;

			if(interactablesInRange.Contains(interactableInHand))
			{
				interactablesInRange.Remove(interactableInHand);
			}

			//habria que hacer que el arma se guardase y deshabilitar el control de activar / desactivar el arma
			if(inputHandler)
			{
				inputHandler.DisableWeapon();
			}
		}
		else if (closestInteractable != null)
		{
			// Interactuar con objetos no recogibles
			closestInteractable.Interact();
		}
	}

	private void InteractWithHeldItem(IInteractable target)
	{
		interactableInHand.InteractWith(target);

		if (interactableInHand.IsConsumedOnInteraction)
		{
			Destroy(((MonoBehaviour)interactableInHand).gameObject);
			interactableInHand = null;
			isPickingUpObject = false;
		}
	}

	private void DropHeldItem()
	{
		if (interactableInHand is IPickable pickable)
		{
			pickable.Drop();

			if(pickable.weight == PickableWeight.Heavy)
			{
				OnHeavyCarrying(false);
			}
			else
			{
				OnLightCarrying(false);
			}
			interactableInHand = null;
			isPickingUpObject = false;
		}

		if(inputHandler)
		{
			inputHandler.EnableWeapon();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		IInteractable interactable = other.GetComponent<IInteractable>();
		
		//Si existe el objeto, no esta en la lista y no lo tengo en la mano, se añade a la lista
		//de interactuables en rango de interaccion
		if(interactable != null && !interactablesInRange.Contains(interactable) && interactable != interactableInHand)
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

	private IInteractable FindClosestInteractable()
	{
		return interactablesInRange
			.OrderBy(interactable => Vector3.Distance(this.transform.position,
			((MonoBehaviour)interactable).transform.position)).FirstOrDefault();
	}


	//No se si el jugador va a tener que implementar necesariamente la interfaz IInteractable, ya que eso podria dar lugar a comportamientos imprevistos
	//y bucles de interaccion. Quizas solamente interesa que el jugador implemente la interfaz IEffectable, que sera la que se encargue de aplicar diferentes efectos de 
	//estado a los objetos
	public bool CanInteractWith(IInteractable other)
	{
		throw new System.NotImplementedException();
	}

	public void InteractWith(IInteractable other)
	{
		throw new System.NotImplementedException();
	}

	public void ReceiveInteraction(IInteractable from)
	{
		throw new System.NotImplementedException();
	}
}
