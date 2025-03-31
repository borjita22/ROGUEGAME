using UnityEngine;


//Ojo, posteriormente esta clase no va a implementar otra interfaz que no sea IInteractable, y ademas no va a definir aqui los metodos, sino que cada tipo de interactable redefinira el comportamiento de sus metodos asociados
public class InteractableEntity : MonoBehaviour, IInteractable, IPickable
{
	[SerializeField] private bool consumedOnInteraction = false;

	// Implementación de la propiedad de la interfaz
	public bool IsConsumedOnInteraction => consumedOnInteraction;

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

	public bool CanInteractWith(IInteractable other)
	{
		return other is IInteractable; //Esto sera asi por el momento, luego cada subclase implementara lo suyo propio
	}

	public void InteractWith(IInteractable other)
	{
		Debug.Log($"Interaccion recibida de {other}");

		//De momento llamamos al metodo receiveInteraction
		ReceiveInteraction(other);
	}

	public void ReceiveInteraction(IInteractable from)
	{

		//Metodo de prueba para ver si el objeto puede recibir el color del objeto que interactua con el
		GameObject otherObj = ((MonoBehaviour)from).gameObject;

		SpriteRenderer spRenderer = this.GetComponent<SpriteRenderer>();

		if(spRenderer)
		{
			otherObj.GetComponent<SpriteRenderer>().color = spRenderer.color;
		}
	}
}
