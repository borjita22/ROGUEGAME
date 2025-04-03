using System.Collections;
using UnityEngine;

//Clase que representa una bala del jugador
public class Bullet : MonoBehaviour
{
    [Header("Bullet configuration")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float timeAlive;

    private Rigidbody rb;

    private Coroutine timeAliveCoroutine;

    private PoolableObject poolableComponent;

    private ObjectPool bulletImpactEffectPool;

	private void Awake()
	{
        rb = GetComponent<Rigidbody>();
        bulletImpactEffectPool = GameObject.Find("BulletImpactEffectPool").GetComponent<ObjectPool>();
	}


	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	protected void OnEnable()
	{
        //nada mas se active la bala hay que ejecutar una corutina que espera el tiempo de vida
        //de la bala para devolverla a la pool de objetos
        //Esto tengo que hacerlo aqui, o bien añadir el componente por inspector
        poolableComponent = GetComponent<PoolableObject>();

        if (timeAliveCoroutine != null)
        {
            StopCoroutine(timeAliveCoroutine);
            timeAliveCoroutine = null;
        }

        timeAliveCoroutine = StartCoroutine(DestroyBulletByTime());
	}

    public void Initialize(Vector3 direction)
	{
        Vector3 normalizedDirection = direction.normalized;

        if(rb != null)
		{
            rb.velocity = normalizedDirection * bulletSpeed;
		}

        transform.forward = normalizedDirection;
	}

    //Esta funcionalidad podria ir en un script independiente que pueda ajustar el tiempo de vida del objeto
    private IEnumerator DestroyBulletByTime()
	{
        yield return new WaitForSeconds(timeAlive);

        //PoolableObject poolableComponent = this.GetComponent<PoolableObject>();

        if(poolableComponent)
		{
            poolableComponent.ReturnToPool();
		}
        else
		{
            Destroy(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.TryGetComponent<EnemyController>(out var enemy))
		{
            enemy.TakeDamage(10f);
		}

        if(bulletImpactEffectPool)
		{
            Vector3 collisionPoint = other.ClosestPoint(this.transform.position);

            GameObject impactEffect = bulletImpactEffectPool.GetObject();

            if(impactEffect)
			{
                impactEffect.transform.position = collisionPoint;

                Vector3 impactDirection = collisionPoint - transform.position;

                impactEffect.transform.rotation = Quaternion.LookRotation(impactDirection);
            }
		}

        if (poolableComponent)
        {
            poolableComponent.ReturnToPool();
        }

    }
}
