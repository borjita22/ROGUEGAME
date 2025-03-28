using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Clase base para todas las habilidades, implementa la interfaz ISkill
/// </summary>
public abstract class BaseSkill : ISkill
{
	public SkillDefinition Definition { get; protected set; }

	public float RemainingCooldown { get; protected set; }

	public bool IsExecuting { get; protected set; }

	protected PlayerController owner;

	protected BaseSkill(SkillDefinition definition, PlayerController owner)
	{
		this.Definition = definition;
		this.owner = owner;
		this.RemainingCooldown = 0f;
		this.IsExecuting = false;
	}

	public bool CanUse()
	{
		return RemainingCooldown <= 0f;
	}

	//Este metodo solamente se va a ejecutar en el caso de que el cooldown sea mayor que 0, lo que implica de forma implicita que se ha utilizado la skill recientemente
	public void UpdateCooldown(float deltaTime)
	{
		if(RemainingCooldown > 0f)
		{
			Debug.Log("Updating skill cooldown");
			RemainingCooldown -= deltaTime;
		}
	}

	public abstract void Use(Vector3 direction);

	protected void StartCooldown()
	{
		Debug.Log("Starting skill cooldown");
		RemainingCooldown = Definition.cooldown;
	}

	//la implementacion de este metodo no nos va a interesar de momento, hasta que no se vea si se va añadir sonido / efecto visual al activar la habilidad
	protected virtual void PlayFeedback()
	{

	}

	public virtual void OnEquipSkill()
	{

	}

	public virtual void OnUnequipSkill()
	{

	}

	public abstract void Cancel();
}
