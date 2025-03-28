using System.Collections;
using UnityEngine;

public class ShieldSkill : BaseSkill
{
	private Coroutine activeCoroutine;
	private ShieldSkillDefinition shieldDefinition;

	private GameObject shieldPrefab;

	private float elapsedUseTime;

	public ShieldSkill(SkillDefinition definition, PlayerController owner) : base(definition, owner)
    {
        
        // Verificar y castear a la definición específica
        if(definition is ShieldSkillDefinition shieldDef)
		{
			shieldDefinition = shieldDef;
		}
		else if(definition is StaticSkillDefinition staticDef && staticDef.staticBehaviour == StaticSkillBehaviour.Shield)
		{
			shieldDefinition = ScriptableObject.CreateInstance<ShieldSkillDefinition>();
			shieldDefinition.skillName = definition.skillName;
			shieldDefinition.cooldown = definition.cooldown;
		}

    }


	public override void Use(Vector3 direction)
	{
		if (!CanUse()) return;

		//en el caso del escudo, simplemente hay que generar el prefab del escudo en la posicion del jugador
		activeCoroutine = SkillCoroutineRunner.Instance.RunCoroutine(EnableShield());
	}


	private IEnumerator EnableShield()
	{
		IsExecuting = true;
		elapsedUseTime = shieldDefinition.shieldDuration;

		PlayFeedback();

		while (elapsedUseTime > 0f)
		{
			elapsedUseTime -= Time.deltaTime;
			yield return null;
		}

		Cancel();
		StartCooldown();

	}


	protected override void PlayFeedback()
	{
		if (Definition.skillPrefab == null) return;

		if (shieldPrefab == null)
		{
			shieldPrefab = GameObject.Instantiate(Definition.skillPrefab, owner.transform);
		}
		else
		{
			shieldPrefab.SetActive(true);
		}

		shieldPrefab.transform.position = owner.transform.position;
		shieldPrefab.transform.rotation = Quaternion.identity;
	}

	public override void Cancel()
	{
		if(IsExecuting && activeCoroutine != null)
		{
			SkillCoroutineRunner.Instance.StopCoroutine(activeCoroutine);

			if(shieldPrefab)
			{
				shieldPrefab.SetActive(false);
			}

			IsExecuting = false;
			activeCoroutine = null;
		}
	}
}
