using UnityEngine;

public class AttackLogic : MonoBehaviour
{
	//Esta clase recibe el evento de disparar por parte del inputHandler y ejecuta la accion de disparo

	//posteriormente, cuando tengamos implementado el sistema de armas, habra varios tipos de disparo (manual, semiautomatico y automatico)

	private PlayerInputHandler inputHandler;

	private void Awake()
	{
		inputHandler = GetComponentInParent<PlayerInputHandler>();

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
	}
}
