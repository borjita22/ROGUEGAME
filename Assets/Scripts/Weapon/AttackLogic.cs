using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class AttackLogic : MonoBehaviour
{
	//Esta clase recibe el evento de disparar por parte del inputHandler y ejecuta la accion de disparo

	//posteriormente, cuando tengamos implementado el sistema de armas, habra varios tipos de disparo (manual, semiautomatico y automatico)

	private PlayerInputHandler inputHandler;

	private ObjectPool bulletPool;
	private ObjectPool muzzleFlashPool;

	[SerializeField] private Transform weaponMuzzle;

	private CinemachineImpulseSource recoilSource;

	[Header("Weapon configuration")]
	[SerializeField] private WeaponType weaponType = WeaponType.Manual;
	[SerializeField] private float fireRate = 0.1f;
	[SerializeField] private int burstCount = 3;

	[Space]
	[Header("Weapon effect elements")]
	[SerializeField] private WeaponEffect currentWeaponEffect = WeaponEffect.Standard;
	[SerializeField] private GameObject fireElementEffect;
	[SerializeField] private GameObject iceElementEffect;
	[SerializeField] private GameObject electricElementEffect;


	private bool isFiring = false;
	private Coroutine autoFireCoroutine;
	private Coroutine burstFireCoroutine;
	private bool canFire = true;

	private bool isWeaponEffectActive;

	private void Awake()
	{
		inputHandler = GetComponentInParent<PlayerInputHandler>();

		recoilSource = GetComponent<CinemachineImpulseSource>();

		bulletPool = GameObject.Find("BulletPool").GetComponent<ObjectPool>();
		muzzleFlashPool = GameObject.Find("MuzzleFlashPool").GetComponent<ObjectPool>();

		fireElementEffect.SetActive(false);
		iceElementEffect.SetActive(false);
		electricElementEffect.SetActive(false);
	}

	private void OnEnable()
	{
		if (inputHandler)
		{
			inputHandler.OnAttack += ProcessAttack;
			inputHandler.OnAttackReleased += StopAttack;
		}
	}


	private void OnDisable()
	{
		if(inputHandler)
		{
			inputHandler.OnAttack -= ProcessAttack;
			inputHandler.OnAttackReleased -= StopAttack;
		}
		StopAllCoroutines();
		isFiring = false;
	}

	private void ProcessAttack()
	{
		if (!canFire) return;

		
		//Aqui hay que determinar el tipo del arma
		//Si es manual, simplemente disparamos una bala
		//Si es semiautomatica, hay que disparar una rafaga
		//Si es automatica, y mientras se mantenga pulsado, hay que continuar generando proyectiles
		if(currentWeaponEffect == WeaponEffect.Standard)
		{
			switch (weaponType)
			{
				case WeaponType.Manual:

					FireSingleBullet();
					StartCoroutine(FireRateCooldown());
					break;
				case WeaponType.Semiautomatic:
					if (burstFireCoroutine == null)
					{
						burstFireCoroutine = StartCoroutine(BurstFireCoroutine());
					}
					break;
				case WeaponType.Automatic:
					isFiring = true;
					if (autoFireCoroutine == null)
					{
						autoFireCoroutine = StartCoroutine(AutomaticFire());
					}
					break;
			}
		}
		else
		{
			ActivateElementalEffect();
		}
		
		//FireSingleBullet();

	}

	private void StopAttack()
	{
		isFiring = false;
		DeactivateElementalEffects();
	}



	private void FireSingleBullet()
	{
		if (bulletPool)
		{
			GameObject bullet = bulletPool.GetObject();

			if (bullet)
			{
				bullet.transform.position = weaponMuzzle.position;

				Bullet bulletComponent = bullet.GetComponent<Bullet>();

				if (bulletComponent)
				{
					bulletComponent.Initialize(weaponMuzzle.forward); //El timeAlive de las balas tambien va a estar determinado por el alcance del arma equipada

					if (recoilSource)
					{
						//Aqui hay elementos hardcodeados, cosa que luego habra que modificar dependiendo de la potencia del arma

						CameraEffects.Instance.RegisterExternalImpulse(recoilSource, weaponMuzzle.forward * 0.25f);
					}
				}
			}
		}
		DisplayMuzzleFlash();
	}

	private void DisplayMuzzleFlash()
	{
		if (muzzleFlashPool)
		{
			GameObject muzzleFlash = muzzleFlashPool.GetObject();

			if (muzzleFlash)
			{
				muzzleFlash.transform.position = weaponMuzzle.transform.position;
				//muzzleFlash.transform.forward = weaponMuzzle.forward; //Esto es chapucero hasta que consiga centrar bien el efecto del muzzle
				//muzzleFlash.transform.SetParent(weaponMuzzle);

				bool isFlipped = transform.lossyScale.x < 0;

				// Ajustar la rotación del muzzle flash según la orientación
				if (isFlipped)
				{
					// Si está volteado, invierte la dirección del forward
					muzzleFlash.transform.forward = -weaponMuzzle.forward;
				}
				else
				{
					// Dirección normal
					muzzleFlash.transform.forward = weaponMuzzle.forward;
				}
			}
		}
	}

	private IEnumerator FireRateCooldown()
	{
		canFire = false;
		yield return new WaitForSeconds(fireRate);
		canFire = true;
	}

	private IEnumerator BurstFireCoroutine()
	{
		for(int i=0; i < burstCount; i++)
		{
			FireSingleBullet();
			yield return new WaitForSeconds(fireRate);
		}
		burstFireCoroutine = null;
	}

	private IEnumerator AutomaticFire()
	{
		while(isFiring)
		{
			FireSingleBullet();
			yield return new WaitForSeconds(fireRate);
		}
		autoFireCoroutine = null;
	}

	//Esto se puede refactorizar haciendo una variable currentEffect, y haciendo que sea el gameObject del efecto el que settee esa variable para luego settear la posicion del gameObject
	private void ActivateElementalEffect()
	{
		DeactivateElementalEffects();

		switch(currentWeaponEffect)
		{
			case WeaponEffect.Fire:
				if(fireElementEffect)
				{
					currentWeaponEffect = WeaponEffect.Fire;
					fireElementEffect.SetActive(true);

					fireElementEffect.transform.position = weaponMuzzle.transform.position;
					fireElementEffect.transform.forward = weaponMuzzle.transform.forward;
				}
				break;

			case WeaponEffect.Ice:
				if(iceElementEffect)
				{
					currentWeaponEffect = WeaponEffect.Ice;
					iceElementEffect.SetActive(true);

					iceElementEffect.transform.position = weaponMuzzle.transform.position;
					iceElementEffect.transform.forward = weaponMuzzle.transform.forward;
				}
				break;

			case WeaponEffect.Electric:
				if(electricElementEffect)
				{
					currentWeaponEffect = WeaponEffect.Electric;
					electricElementEffect.SetActive(true);

					electricElementEffect.transform.position = weaponMuzzle.transform.position;
					electricElementEffect.transform.forward = weaponMuzzle.transform.forward;
				}
				break;
		}

		isWeaponEffectActive = true;
	}

	private void DeactivateElementalEffects()
	{
		if (fireElementEffect) fireElementEffect.SetActive(false);
		if (iceElementEffect) iceElementEffect.SetActive(false);
		if (electricElementEffect) electricElementEffect.SetActive(false);

		//currentWeaponEffect = WeaponEffect.Standard;
		isWeaponEffectActive = false;
	}

	public void CycleWeaponEffect()
	{
		currentWeaponEffect = (WeaponEffect)(((int)currentWeaponEffect + 1) % System.Enum.GetValues(typeof(WeaponEffect)).Length);
	}

	//Hay que hacer metodos para settear los datos del arma (Esto se hara una vez configure los diferentes scriptableObjects
}

public enum WeaponType
{
	Manual,
	Semiautomatic,
	Automatic
}

public enum WeaponEffect
{
	Standard,
	Fire,
	Ice,
	Electric
}
