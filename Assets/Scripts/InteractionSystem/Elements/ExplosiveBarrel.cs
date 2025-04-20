using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : Box
{
    private ObjectPool explosionPool;

	protected override void Awake()
	{
		base.Awake();

		explosionPool = GameObject.Find("FireExplosionPool").GetComponent<ObjectPool>();

	}

	
	private new void OnDisable()
	{
		if(explosionPool)
		{
			GameObject explosion = explosionPool.GetObject();

			explosion.transform.position = this.transform.position;

			//Ademas de todo esto, hay que generar el collider de la explosion, que a su vez aplica un efecto de fuego a todos los objetos cercanos
			//Se va a generar el efecto de fuego desde este mismo script

			//ojo, esto se puede generar aqui, pero quizas interesa mas generarlo directamente a la altura del suelo, sin tener que calcular
			//dinamicamente la posicion en la que generarlo
		}
	}
}
