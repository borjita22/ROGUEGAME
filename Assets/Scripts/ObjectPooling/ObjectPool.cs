using UnityEngine;

//Esta es una clase base que representa una object pool, que sirve como almacen de objetos que ira necesitando el jugador durante la partida
public class ObjectPool : MonoBehaviour
{
    [Header("References")]

    [SerializeField] private string objectName;
    public string ObjectName => objectName;

    [SerializeField] protected GameObject pooledObject;
    [SerializeField] protected int numberOfPooledElements;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializePool();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Este metodo inicializa la pool creando los objetos y haciendo que sean hijos de esta pool de objetos
    private void InitializePool()
	{
        for(int i = 0; i < numberOfPooledElements; i++)
		{
            GameObject element = Instantiate(pooledObject, this.transform);
            element.SetActive(false);
		}
	}

    //Se encarga de devolver un elemento de la pool
    //Si hay al menos un elemento que no esta activo, lo devuelve
    //Si no lo hay, crea uno y lo devuelve igualmente
    public GameObject GetObject()
	{
        for(int i = 0; i< transform.childCount; i++)
		{
            GameObject pooledElement = transform.GetChild(i).gameObject;

            if(!pooledElement.activeInHierarchy)
			{
                AssignPoolToCreatedObject(pooledElement);

                pooledElement.SetActive(true);
                return pooledElement;
			}
		}

        GameObject newElement = Instantiate(pooledObject, this.transform);

        AssignPoolToCreatedObject(newElement);

        newElement.SetActive(true);
        return newElement;
	}

    public void ReturnToPool(GameObject element)
	{
        Transform parent = element.transform.parent;
        if(parent == null || parent != this.transform)
		{
            element.transform.SetParent(this.transform);
        }
        element.SetActive(false);
	}

    private void AssignPoolToCreatedObject(GameObject newObj)
	{
        PoolableObject poolable = newObj.GetComponent<PoolableObject>();
        if (poolable == null)
        {
            poolable = newObj.AddComponent<PoolableObject>();
        }

        // Asignar esta pool como origen
        poolable.SetOriginPool(this);
    }

}
