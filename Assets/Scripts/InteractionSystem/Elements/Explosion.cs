using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Esta clase se vincula al objeto que representa una explosion
//Es un objecto Effectable, que puede aplicar efectos de estado a otros objetos
//Ademas, puede generar efectos derivados como fuego en el suelo
public class Explosion : MonoBehaviour
{
    //OJO, ESTO VA A SER PARA TESTEAR EL SISTEMA. POSTERIORMENTE SE HARAN SUBCLASES QUE IMPLEMENTARAN SU
    //PROPIA LOGICA

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            //Aplicamos efecto de fuego en el hit position
            Vector3 collisionPoint = other.ClosestPoint(transform.position);

            if(PoolMediator.Instance)
            {
                GameObject fireElement = PoolMediator.Instance.RequestPooledObject("GroundFire");

                if(fireElement)
                {
                    fireElement.transform.position = collisionPoint;
                }
            }
        }
    }
}
