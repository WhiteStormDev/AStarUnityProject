using System;
using UnityEngine;

namespace Base
{
    public class DontDestroySingletonAttribute : Attribute
    {
    }

    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        public static bool WasDestroyed { get; private set; }
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        WasDestroyed = false;

                        var gameObject = new GameObject(typeof(T).Name);
                        _instance = gameObject.AddComponent<T>();

                        if (Attribute.GetCustomAttribute(typeof(T), typeof(DontDestroySingletonAttribute)) is DontDestroySingletonAttribute dontDestroyAttribute)
                        {
                            DontDestroyOnLoad(_instance);
                        }
                    }
                }

                return _instance;
            }
        }

        protected virtual void OnCreate()
        {
        }

        private void Awake()
        {
            OnCreate();
        }

        protected virtual void OnReleaseResource()
        {
        }

        private void OnDestroy()
        {
            WasDestroyed = true;
            _instance = null;
            OnReleaseResource();
        }
    }
}