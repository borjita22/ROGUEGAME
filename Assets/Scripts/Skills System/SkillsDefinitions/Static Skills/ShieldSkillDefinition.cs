using UnityEngine;

[CreateAssetMenu(fileName = "NewDashSkill", menuName = "Game/Skills/Shield Skill")]
public class ShieldSkillDefinition : StaticSkillDefinition
{
    [Header("Shield Specific Settings")]

    [SerializeField] public float shieldDuration;

	private void OnEnable()
	{
		staticBehaviour = StaticSkillBehaviour.Shield;
	}

}
