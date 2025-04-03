using UnityEngine;
using Unity.Cinemachine;

public class AttackLogic : MonoBehaviour
{
	//Esta clase recibe el evento de disparar por parte del inputHandler y ejecuta la accion de disparo

	//posteriormente, cuando tengamos implementado el sistema de armas, habra varios tipos de disparo (manual, semiautomatico y automatico)

	private PlayerInputHandler inputHandler;

	private ObjectPool bulletPool;
	private ObjectPool muzzleFlashPool;

	[SerializeField] private Transform weaponMuzzle;

	private CinemachineImpulseSource recoilSource;
	private void Awake()
	{
		inputHandler = GetComponentInParent<PlayerInputHandler>();

		recoilSource = GetComponent<CinemachineImpulseSource>();

		bulletPool = GameObject.Find("BulletPool").GetComponent<ObjectPool>();
		muzzleFlashPool = GameObject.Find("MuzzleFlashPool").GetComponent<ObjectPool>();
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
		//para empezar, hay que obtener un proyectil de la pool de proyectiles. Dependiendo del tipo de disparo, podria ser solamente uno o varios de ellos
		if(bulletPool)
		{
			GameObject bullet = bulletPool.GetObject();

			if(bullet)
			{
				bullet.transform.position = weaponMuzzle.position;

				Bullet bulletComponent = bullet.GetComponent<Bullet>();

				if(bulletComponent)
				{
					bulletComponent.Initialize(weaponMuzzle.forward); //El timeAlive de las balas tambien va a estar determinado por el alcance del arma equipada

					if(recoilSource)
					{
						//Aqui hay elementos hardcodeados, cosa que luego habra que modificar dependiendo de la potencia del arma
						
						CameraEffects.Instance.RegisterExternalImpulse(recoilSource, weaponMuzzle.forward * 0.25f);
					}
				}
			}
		}

		if(muzzleFlashPool)
		{
			GameObject muzzleFlash = muzzleFlashPool.GetObject();

			if(muzzleFlash)
			{
				muzzleFlash.transform.position = weaponMuzzle.transform.position;
				muzzleFlash.transform.forward = -weaponMuzzle.transform.forward; //Esto es chapucero hasta que consiga centrar bien el efecto del muzzle
			}
		}
	}
}
