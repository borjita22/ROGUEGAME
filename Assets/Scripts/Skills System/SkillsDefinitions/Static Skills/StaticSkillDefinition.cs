using UnityEngine;

public abstract class StaticSkillDefinition : SkillDefinition
{
    public override SkillType SkillType => SkillType.Static;

    [Header("Static skill configuration")]
    public StaticSkillBehaviour staticBehaviour;

	public override ISkill CreateInstance(PlayerController owner)
	{
		switch (staticBehaviour)
		{
			case StaticSkillBehaviour.Dash:
				return new DashSkill(this, owner);
				
			case StaticSkillBehaviour.Shield:
				return new ShieldSkill(this, owner);

			case StaticSkillBehaviour.Heal:
				return new HealerSkill(this, owner);
		}

		return null; //temporal
	}
}
