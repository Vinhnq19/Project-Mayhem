using UnityEngine;

namespace ProjectMayhem.Manager
{
    /// <summary>
    /// Generic Singleton base class for MonoBehaviour managers
    /// </summary>
    /// <typeparam name="T">The type of the singleton</typeparam>
    public class GenericSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[GenericSingleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError($"[GenericSingleton] Something went really wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = $"[Singleton] {typeof(T).ToString()}";

                            // DontDestroyOnLoad(singleton);

                            Debug.Log($"[GenericSingleton] An instance of {typeof(T)} is needed in the scene, so '{singleton}' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            Debug.Log($"[GenericSingleton] Using instance already created: {_instance.gameObject.name}");
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            _applicationIsQuitting = true;
        }
    }
}
