using UnityEngine;

public class AttackLogic : MonoBehaviour
{
	//Esta clase recibe el evento de disparar por parte del inputHandler y ejecuta la accion de disparo

	//posteriormente, cuando tengamos implementado el sistema de armas, habra varios tipos de disparo (manual, semiautomatico y automatico)

	private PlayerInputHandler inputHandler;

	private ObjectPool bulletPool;

	[SerializeField] private Transform weaponMuzzle;

	private void Awake()
	{
		inputHandler = GetComponentInParent<PlayerInputHandler>();

		

		bulletPool = GameObject.Find("BulletPool").GetComponent<ObjectPool>();
	}

	private void OnEnable()
	{
		if (inputHandler)
		{
			inputHandler.OnAttack += ProcessAttack;
		}
	}


	private void OnDisable()
	{
		inputHandler.OnAttack -= ProcessAttack;
	}

	private void ProcessAttack()
	{
		Debug.Log("Player is shooting");
		//para empezar, hay que obtener un proyectil de la pool de proyectiles
		if(bulletPool)
		{
			GameObject bullet = bulletPool.GetObject();

			if(bullet)
			{
				bullet.transform.position = weaponMuzzle.position;

				Bullet bulletComponent = bullet.GetComponent<Bullet>();

				if(bulletComponent)
				{
					bulletComponent.Initialize(weaponMuzzle.forward);
				}
			}
		}
	}
}
