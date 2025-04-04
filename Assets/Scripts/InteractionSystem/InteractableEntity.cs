using UnityEngine;


//Ojo, posteriormente esta clase no va a implementar otra interfaz que no sea IInteractable, y ademas no va a definir aqui los metodos, sino que cada tipo de interactable redefinira el comportamiento de sus metodos asociados
public class InteractableEntity : MonoBehaviour, IInteractable
{
	[SerializeField] private bool consumedOnInteraction = false;

	// Implementación de la propiedad de la interfaz
	public bool IsConsumedOnInteraction => consumedOnInteraction;

	public void Interact()
	{
		Debug.Log("Soy un objeto interactuable que esta cerca del jugador");
	}

	//Este metodo sera refactorizado eventualmente en otras clases hijas
	public bool CanInteractWith(IInteractable other)
	{
		return other is IInteractable;
	}

	public void InteractWith(IInteractable other)
	{
		Debug.Log($"Interaccion recibida de {other}");

		//De momento llamamos al metodo receiveInteraction
		other.ReceiveInteraction(this);
	}


	public virtual void ReceiveInteraction(IInteractable from)
	{
		//AQUI HAY QUE RECIBIR LA INTERACCION DE OTRO OBJETO. ESTO PUEDE VARIAR MUCHO DEPENDIENDO DEL TIPO DE OBJETO, POR LO QUE SEGURAMENTE SE REDEFINIRA EN LAS CLASES
		//HIJAS DE ESTE OBJETO
		
		
	}
}
