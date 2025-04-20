using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMediator : MonoBehaviour
{
    public static PoolMediator Instance { get; private set; }

    [SerializeField] private List<ObjectPool> gamePools = new List<ObjectPool>();


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public GameObject RequestPooledObject(string objectType)
    {
        try
        {
            ObjectPool requestedPool = FindPoolForObjectType(objectType);

            if(requestedPool)
            {
                return requestedPool.GetObject();
            }
        }
        catch(Exception)
        {
            Debug.LogError("No se ha encontrado una pool con ese identificador");
        }

        return null;
    }

    private ObjectPool FindPoolForObjectType(string type)
    {
        foreach(var pool in gamePools)
        {
            if(pool.ObjectName == type)
            {
                return pool;
            }
        }

        return null;
    }
}
