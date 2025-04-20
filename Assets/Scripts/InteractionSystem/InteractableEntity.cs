using UnityEngine;


//Ojo, posteriormente esta clase no va a implementar otra interfaz que no sea IInteractable, y ademas no va a definir aqui los metodos, sino que cada tipo de interactable redefinira el comportamiento de sus metodos asociados
public class InteractableEntity : MonoBehaviour, IInteractable
{
    [SerializeField] protected bool consumedOnInteraction = false;

    public bool IsConsumedOnInteraction => consumedOnInteraction;

    public virtual InteractionResult Interact(PlayerInteractionsController controller)
    {
        Debug.Log("Interacci?n b?sica con objeto interactuable");
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
        Debug.Log($"Recibiendo interacci?n de {from}");
    }

    protected virtual void OnDisable()
    {
        //Codigo a ejecutar cuando se desactiva un objeto interactuable
        if (PoolMediator.Instance)
        {
            GameObject disableCloud = PoolMediator.Instance.RequestPooledObject("DisappearCloud");
            if (disableCloud)
            {
                disableCloud.transform.position = this.transform.position;

            }
        }
    }
}
