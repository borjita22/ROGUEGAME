using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Esta clase se va a encargar de transmitir efectos de estados a objetos cercanos
/// No es interactuable y no puede ser afectada por otros efectos, sino que solamente los puede aplicar
/// </summary>
public class EffectTransmisor : MonoBehaviour
{
    [SerializeField] protected EffectType effectToApply; //Solamente van a poder aplicar un efecto

    //Necesito acceso al collider
    protected void OnTriggerEnter(Collider other)
    {
        //si other es affectable (tiene un componente del tipo effectableObject o implementa iEffectable
        if(other.TryGetComponent<IEffectable>(out var effectableElement))
        {
            effectableElement.ApplyEffect(effectToApply);
        }
    }
}
