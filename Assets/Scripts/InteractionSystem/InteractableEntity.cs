using UnityEngine;

public class InteractableEntity : MonoBehaviour, IInteractable, IPickable
{

	public void Interact()
	{
		Debug.Log("Soy un objeto interactuable que esta cerca del jugador");
	}

	public void PickUp(Transform parent)
	{
		//Colocar este objeto como hijo del transform pasado como parametro
		this.transform.SetParent(parent);
		this.transform.position = parent.position;
	}


	public void Drop()
	{
		this.transform.SetParent(null);
	}


}
