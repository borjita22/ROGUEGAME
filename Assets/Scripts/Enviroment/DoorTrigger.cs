using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Interruptor para puertas

//Esta clase controla los interruptores de las puertas de los niveles
//Los interruptores pueden ser fijos (en cuanto lo activas te puedes salir) o no fijos (en cuanto sales el interruptor se desactiva, por
//lo que hay que mantener un elemento encima para que permanezca activado
//tiene que tener una referencia a la puerta que es capaz de abrir, y enviar un evento para abrirla o cerrarla
public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private bool isPermanentTrigger;
    [SerializeField] private Door attachedDoor;

    public delegate void OnTriggerStatus(bool status, Door attachedDoor);
    public static event OnTriggerStatus _OnTriggerStatus;

    private Renderer objectRenderer;

    [SerializeField] private Collider currentCollidedObject;
    // Start is called before the first frame update
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //CheckCurrentCollisions -> Cada cierto tiempo, detecta colisiones encima del gameObject. Si no detecta ninguna, cambia su status a false
        if(currentCollidedObject != null)
		{
            Invoke(nameof(CheckCollisions), 0.5f);
        }
    }

	private void OnCollisionEnter(Collision collision)
	{
        //un interruptor puede ser activado por el jugador o por un objeto interactuable
        if(collision.gameObject.GetComponent<InteractableEntity>() ||collision.gameObject.CompareTag("Player"))
		{
            _OnTriggerStatus?.Invoke(true, attachedDoor);
            ChangeTriggerVisualInfo(true);

            currentCollidedObject = collision.collider;
        }
        

	}

	private void OnCollisionExit(Collision collision)
	{
        if (collision.gameObject.GetComponent<InteractableEntity>() || collision.gameObject.CompareTag("Player"))
		{
            if (!isPermanentTrigger)
            {
                _OnTriggerStatus?.Invoke(false, attachedDoor);
                ChangeTriggerVisualInfo(false);
            }
        }
      
	}

    private void CheckCollisions()
	{
        bool hasCollidingObjects = false;

        Vector3 center = this.GetComponent<Collider>().bounds.center;
        Vector3 halfExtents = this.GetComponent<Collider>().bounds.extents;
        Quaternion orientation = transform.rotation;
        //Collider[] colliders = Physics.OverlapBox(transform.position);

        Collider[] colliders = Physics.OverlapBox(center, halfExtents, orientation);

        foreach (Collider col in colliders)
        {
            if (col.GetComponent<InteractableEntity>() || col.CompareTag("Player"))
            {
                hasCollidingObjects = true;
                break;
            }
        }

        if (!hasCollidingObjects)
        {
            _OnTriggerStatus?.Invoke(false, attachedDoor);
            ChangeTriggerVisualInfo(false);
            currentCollidedObject = null;
        }
    }

    private void ChangeTriggerVisualInfo(bool status)
	{
        MaterialPropertyBlock materialPB = new MaterialPropertyBlock();

        //objectRenderer = GetComponent<Renderer>();

        objectRenderer.GetPropertyBlock(materialPB);

        if(status == true)
		{
            materialPB.SetColor("_BaseColor", Color.blue);

        }
        else
		{
            materialPB.SetColor("_BaseColor", Color.red);
		}

        objectRenderer.SetPropertyBlock(materialPB);
        
	}
}
