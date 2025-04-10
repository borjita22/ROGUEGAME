using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controla las interacciones del jugador con los elementos del entorno
/// </summary>
public class PlayerInteractionsController : MonoBehaviour
{
	[SerializeField] private List<IInteractable> interactablesInRange = new List<IInteractable>();

	private PlayerInputHandler inputHandler;

	[Header("Pickable slots")]
	[SerializeField] public Transform heavyPickableElementSlot;
	[SerializeField] public Transform lightPickableElementSlot;

	private bool isPickingUpObject = false;
	public IInteractable interactableInHand;
	private IPushable currentPushable;

	//public bool IsConsumedOnInteraction => false;

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
			// Si no hay objeto cercano
			else if(closestInteractable == null)
			{
				DropHeldItem();
			}
		}
		// Si no sostenemos nada, intentar recoger o interactuar con algo cercano
		else if (interactablesInRange.Count > 0)
		{
			IInteractable closestInteractable = FindClosestInteractable();

			if (closestInteractable != null)
			{
				// Permitir que el objeto maneje su propia interacción
				InteractionResult result = closestInteractable.Interact(this);
				HandleInteractionResult(result, closestInteractable);
			}
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
			closestInteractable.Interact(this);
		}
	}

	//aqui tengo que comprobar que el objeto puede interactuar con el otro objeto
	//Ademas, el metodo canInteractWith debe redefinirse en las diferentes subclases
	private void InteractWithHeldItem(IInteractable target)
	{
		if(interactableInHand.CanInteractWith(target))
		{
			interactableInHand.InteractWith(target);
		}
		else
		{
			return;
		}
		
		//Si el objeto no puede interactuar esto no se debe de poder ejecutar, ya que estariamos eliminando el objeto
		if (interactableInHand.IsConsumedOnInteraction)
		{
			DropHeldItem();
			//Destroy(((MonoBehaviour)interactableInHand).gameObject);
			
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

	private void HandleInteractionResult(InteractionResult result, IInteractable interactable)
	{
		switch (result)
		{
			case InteractionResult.ItemPickedUp:
				isPickingUpObject = true;
				interactableInHand = interactable;

				// Determinar peso y notificar
				if (interactable is IPickable pickable)
				{
					if (pickable.weight == PickableWeight.Heavy)
						OnHeavyCarrying?.Invoke(true);
					else
						OnLightCarrying?.Invoke(true);
				}

				// Remover de la lista de interactuables en rango
				if (interactablesInRange.Contains(interactableInHand))
				{
					interactablesInRange.Remove(interactableInHand);
				}

				// Deshabilitar arma mientras cargamos objetos
				if (inputHandler)
				{
					inputHandler.DisableWeapon();
				}
				break;

			case InteractionResult.PushEnabled:
				// Almacenar referencia si es un objeto empujable
				currentPushable = interactable as IPushable;
				break;

			case InteractionResult.PushDisabled:
				// Limpiar referencia
				currentPushable = null;
				break;

			case InteractionResult.ItemConsumed:
				// El objeto ya se consumió/destruyó en su propia lógica
				// No necesitamos hacer nada aquí
				break;

			case InteractionResult.InteractionApplied:
				// Interacción genérica completada
				break;

			case InteractionResult.None:
				// No ocurrió nada especial
				break;
		}
	}
}
