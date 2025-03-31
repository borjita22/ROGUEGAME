using UnityEngine;


/// <summary>
/// Interfaz con propiedades y funciones comunes a TODOS los elementos interactuables
/// </summary>
public interface IInteractable
{
    //Interaccion directa del jugador con el objeto
    void Interact();

    //Determina si este objeto puede interactuar con otro objeto
    bool CanInteractWith(IInteractable other);

    //Ejecuta la interaccion entre este objeto y otro
    void InteractWith(IInteractable other);

    //Determina si este objeto debe ser eliminado como consecuencia de una interacción
    bool IsConsumedOnInteraction { get; }

    //Este metodo sirve para que otros objetos interactuen con este
    void ReceiveInteraction(IInteractable from);

    
}
