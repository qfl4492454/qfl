using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QTool.Resource;
using System;

namespace QTool
{
  
    public abstract class InstanceBehaviour<T,ResourceLabel> : InstanceBehaviour<T> where ResourceLabel : PrefabResourceList<ResourceLabel> where T : InstanceBehaviour<T,ResourceLabel>
    {
        public new static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        PrefabResourceList<ResourceLabel>.LoadOverRun(() =>
                        {
                            if (_instance == null)
                            {
                                _instance = GetNewInstance();
                            }
                        });
                    }
                }
                return _instance;
            }
        }
       
        public static T GetNewInstance()
        {
           return  PrefabResourceList<ResourceLabel>.GetInstance(typeof(T).Name).GetComponent<T>();
        }
    }
    public abstract class InstanceBehaviour<T> : MonoBehaviour where T : InstanceBehaviour<T>
    {
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                }
                return _instance;
            }
        }
        protected static T _instance;
        protected virtual void Awake()
        {
            _instance = this as T;
        }
    }
    public abstract class InstanceObject<T> where T : InstanceObject<T>
    {
        public static readonly T Instance = Activator.CreateInstance<T>();
        protected InstanceObject()
        {

        }
    }
}