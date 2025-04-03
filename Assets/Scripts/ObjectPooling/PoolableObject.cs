using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    private ObjectPool originPool;

	private TemporalPooledElement temporalPooledObject;

	private void Awake()
	{
		temporalPooledObject = GetComponent<TemporalPooledElement>();
	}

	private void OnEnable()
	{
		//Cuando se activa el objeto, y si tiene un componente de elemento temporal, se devuelve a la pool pasado su tiempo de vida
		if(temporalPooledObject)
		{
			Invoke(nameof(ReturnToPool), temporalPooledObject.timeAlive);
		}
	}

	public void SetOriginPool(ObjectPool pool)
	{
		this.originPool = pool;
	}

	public void ReturnToPool()
	{
		if(originPool)
		{
			gameObject.SetActive(false);
			originPool.ReturnToPool(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}


}
