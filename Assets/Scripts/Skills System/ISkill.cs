using UnityEngine;

public interface ISkill
{
    
    SkillDefinition Definition { get; }
    float RemainingCooldown { get; }

    bool IsExecuting { get; }

    bool IsOnCooldown { get; }
    bool CanUse();

    void Use(Vector3 direction);

    void UpdateCooldown(float deltaTime);

    void Cancel();
}
