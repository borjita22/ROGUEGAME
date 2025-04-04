using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectable
{
    void ApplyEffect(EffectType effectType);

    bool HasEffect(EffectType effectType);

    void RemoveEffect(EffectType effectType);
}
