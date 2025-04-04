using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectInstance
{
    public EffectType Type { get; private set; }
    public float RemainingDuration { get; private set; }
    public bool IsActive => RemainingDuration > 0 || RemainingDuration < 0;
    public GameObject VisualObject { get; set; }

    public EffectInstance(EffectType type, float duration)
    {
        Type = type;
        RemainingDuration = duration;
    }

    public void Update(float deltaTime)
    {
        if (RemainingDuration > 0)
        {
            RemainingDuration -= deltaTime;
        }
    }

    public void RefreshDuration(float newDuration)
    {
        if (newDuration > RemainingDuration)
        {
            RemainingDuration = newDuration;
        }
    }
}
