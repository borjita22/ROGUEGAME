using UnityEngine;

[CreateAssetMenu(fileName = "NewDashSkill", menuName = "Game/Skills/Dash Skill")]
public class DashSkillDefinition : StaticSkillDefinition
{
    [Header("Dash Specific Settings")]
    public float dashDistance = 5f;
    public float dashSpeed = 15f;
    public bool providesInvulnerability = true;

	private void OnEnable()
	{
		staticBehaviour = StaticSkillBehaviour.Dash;
	}
}
