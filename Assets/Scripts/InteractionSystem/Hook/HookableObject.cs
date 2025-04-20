using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookableObject : MonoBehaviour, IHookable
{
    public void OnHook()
    {
        Debug.Log("Object hooked");
    }
}
