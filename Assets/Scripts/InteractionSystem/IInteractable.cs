using UnityEngine;


/// <summary>
/// Interfaz con propiedades y funciones comunes a TODOS los elementos interactuables
/// </summary>
public interface IInteractable
{
    //Interaccion directa del jugador con el objeto
    InteractionResult Interact(PlayerInteractionsController controller = null); //Aqui quizas no haga falta pasarle el controlador de interaccion salvo que se pretenda hacer algo especifico con el

    //Determina si este objeto puede interactuar con otro objeto
    bool CanInteractWith(IInteractable other);

    //Ejecuta la interaccion entre este objeto y otro
    InteractionResult InteractWith(IInteractable other);

    //Determina si este objeto debe ser eliminado como consecuencia de una interacción
    bool IsConsumedOnInteraction { get; }

    //Este metodo sirve para que otros objetos interactuen con este
    void ReceiveInteraction(IInteractable from);

    
}

public enum InteractionResult
{
    None,
    ItemPickedUp,
    ItemConsumed,
    InteractionApplied,
    PushEnabled,
    PushDisabled
}
