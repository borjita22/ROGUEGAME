using System.Collections;
using UnityEngine;

public class HealerSkill : BaseSkill
{
	private Coroutine activeCoroutine;
	private HealerSkillDefinition healerDefinition;

	private GameObject healPrefab;

	private float elapsedUseTime;
	public HealerSkill(SkillDefinition definition, PlayerController owner) : base(definition, owner)
	{
		// Verificar y castear a la definición específica
		if (definition is HealerSkillDefinition healerDef)
		{
			healerDefinition = healerDef;
		}
		else if (definition is StaticSkillDefinition staticDef && staticDef.staticBehaviour == StaticSkillBehaviour.Shield)
		{
			healerDefinition = ScriptableObject.CreateInstance<HealerSkillDefinition>();
			healerDefinition.skillName = definition.skillName;
			healerDefinition.cooldown = definition.cooldown;
		}

	}

	public override void Use(Vector3 direction)
	{
		if (!CanUse()) return;

		activeCoroutine = SkillCoroutineRunner.Instance.RunCoroutine(ActivateHealing());

	}

	//aqui se curaria al jugador dependiendo de su subtipo: salud o mana
	private IEnumerator ActivateHealing()
	{
		IsExecuting = true;
		elapsedUseTime = healerDefinition.healDuration;

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

		if (healPrefab == null)
		{
			healPrefab = GameObject.Instantiate(Definition.skillPrefab, owner.transform);
		}
		else
		{
			healPrefab.SetActive(true);
		}

		healPrefab.transform.position = owner.transform.position;
		healPrefab.transform.rotation = Quaternion.identity;
	}

	public override void Cancel()
	{
		if(IsExecuting && activeCoroutine != null)
		{
			SkillCoroutineRunner.Instance.StopCoroutine(activeCoroutine);

			if (healPrefab)
			{
				healPrefab.SetActive(false);
			}

			IsExecuting = false;
			activeCoroutine = null;
		}
	}
}
