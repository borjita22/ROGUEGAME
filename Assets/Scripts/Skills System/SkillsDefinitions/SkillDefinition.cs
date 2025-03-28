using UnityEngine;

public abstract class SkillDefinition : ScriptableObject
{
    [Header("Skill info")]
    public string skillName;
    public Sprite skillIcon;
    [TextArea(2, 5)]
    public string description;

    [Header("Gameplay settings")]
    public abstract SkillType SkillType { get; }
    public float cooldown;

    [Header("Effect settings")]
    public EffectType[] producedEffectTypes;

    [Range(0f, 1f)]
    public float effectIntensity;
    public float effectDuration;

    [Header("Prefab References")]
    public GameObject skillPrefab;

    [Header("Visual and audio")]
    public ParticleSystem activationEffect; //tal vez esta propiedad nos interese cambiarla si optamos por VFX en lugar de sistemas de particulas
    public AudioClip activationSound;


    //OJO: Evaluar viabilidad de esta funcion

    public abstract ISkill CreateInstance(PlayerController owner);


}
