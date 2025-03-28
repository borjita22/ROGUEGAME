using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileSkill", menuName = "Game/Skills/Projectile Skill")]
public class ProjectileSkillDefinition : SkillDefinition
{
	public override SkillType SkillType => SkillType.Projectile;

	[Header("Projectile configuration")]
	public Vector3 spawnOffset = Vector3.forward;

	//tambien tiene una propiedad damage, que se le pasara al proyectil
	public float projectileDamage;

	public override ISkill CreateInstance(PlayerController owner)
	{
		return new ProjectileSkill(this, owner);
	}
}
