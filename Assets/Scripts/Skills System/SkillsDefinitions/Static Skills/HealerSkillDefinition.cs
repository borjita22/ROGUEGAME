using UnityEngine;

[CreateAssetMenu(fileName = "NewDashSkill", menuName = "Game/Skills/Healer Skill")]
public class HealerSkillDefinition : StaticSkillDefinition
{
    [Header("Shield Specific Settings")]
    [SerializeField] public float healDuration;
    [SerializeField] public float healAmount;
    [SerializeField] public HealerType healType;

	private void OnEnable()
	{
        staticBehaviour = StaticSkillBehaviour.Heal;
	}
}

public enum HealerType
{
    Health,
    Mana
}
