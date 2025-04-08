using UnityEngine;


//Ojo, posteriormente esta clase no va a implementar otra interfaz que no sea IInteractable, y ademas no va a definir aqui los metodos, sino que cada tipo de interactable redefinira el comportamiento de sus metodos asociados
public class InteractableEntity : MonoBehaviour, IInteractable
{
    [SerializeField] protected bool consumedOnInteraction = false;

    public bool IsConsumedOnInteraction => consumedOnInteraction;

    public virtual InteractionResult Interact(PlayerInteractionsController controller)
    {
        Debug.Log("Interacción básica con objeto interactuable");
        return InteractionResult.InteractionApplied;
    }

    public virtual bool CanInteractWith(IInteractable other)
    {
        return other is IInteractable && other != this;
    }

    public virtual InteractionResult InteractWith(IInteractable other)
    {
        Debug.Log($"Interactuando con {other}");
        other.ReceiveInteraction(this);
        return InteractionResult.InteractionApplied;
    }

    public virtual void ReceiveInteraction(IInteractable from)
    {
        Debug.Log($"Recibiendo interacción de {from}");
    }
}
