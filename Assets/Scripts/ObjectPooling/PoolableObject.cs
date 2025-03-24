using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    private ObjectPool originPool;
   
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
