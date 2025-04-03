using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraEffects : MonoBehaviour
{
    private static CameraEffects _instance;

    public static CameraEffects Instance => _instance;

    private CinemachineCamera cinemachineCam; //esto quizas nos sirva mas adelante

	private void Awake()
	{
		if(_instance == null)
		{
			_instance = this;
		}
		else
		{
			Destroy(this.gameObject);
		}
	}

	//aqui vamos a poder modificar muchas mas cosas referentes al impulso que recibe la camara, entre ellas el tipo de impulso a recibir
	public void RegisterExternalImpulse(CinemachineImpulseSource source, Vector3 velocity)
	{
        if (source == null) return;

        source.GenerateImpulseWithVelocity(velocity);
	}
}
