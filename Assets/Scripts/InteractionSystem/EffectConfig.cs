using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectConfig
{
    [SerializeField] private ObjectPool fireEffectPool;
    [SerializeField] private ObjectPool iceEffectPool;
    [SerializeField] private ObjectPool poisonEffectPool;
    [SerializeField] private ObjectPool electricEffectPool;

    public ObjectPool GetEffectPool(EffectType effectType)
	{
        switch(effectType)
		{
            case EffectType.Fire: return fireEffectPool;
            case EffectType.Ice: return iceEffectPool;
            case EffectType.Poison: return poisonEffectPool;
            case EffectType.Electric: return electricEffectPool;
            default: return null;
        }
	}
}
