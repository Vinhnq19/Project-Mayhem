using System.Collections.Generic;
using UnityEngine;
using ProjectMayhem.Manager;
using ProjectMayhem.Projectiles;

namespace ProjectMayhem.Utilities
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public Transform parent;
    }

    public class ObjectPooler : GenericSingleton<ObjectPooler>
    {
        [Header("Pool Settings")]
        [SerializeField] private List<Pool> pools = new List<Pool>();

        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, Pool> poolDataDictionary;

        public Dictionary<string, Queue<GameObject>> PoolDictionary => poolDictionary;
        public int TotalPools => poolDictionary?.Count ?? 0;

        protected override void Awake()
        {
            base.Awake();
            InitializePools();
        }

        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            poolDataDictionary = new Dictionary<string, Pool>();

            foreach (Pool pool in pools)
            {
                CreatePool(pool);
            }

            Debug.Log($"[ObjectPooler] Initialized {pools.Count} object pools");
        }

        private void CreatePool(Pool pool)
        {
            if (pool.prefab == null)
            {
                Debug.LogError($"[ObjectPooler] Pool '{pool.tag}' has no prefab assigned!");
                return;
            }

            if (string.IsNullOrEmpty(pool.tag))
            {
                Debug.LogError("[ObjectPooler] Pool tag cannot be null or empty!");
                return;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();

            Transform poolParent = pool.parent;
            if (poolParent == null)
            {
                GameObject poolParentObj = new GameObject($"Pool_{pool.tag}");
                poolParentObj.transform.SetParent(transform);
                poolParent = poolParentObj.transform;
            }

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, poolParent);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
            poolDataDictionary.Add(pool.tag, pool);

            Debug.Log($"[ObjectPooler] Created pool '{pool.tag}' with {pool.size} objects");
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist!");
                return null;
            }

            GameObject objectToSpawn = poolDictionary[tag].Dequeue();

            if (objectToSpawn == null)
            {
                Pool poolData = poolDataDictionary[tag];
                objectToSpawn = Instantiate(poolData.prefab, poolData.parent);
            }

            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            objectToSpawn.SetActive(true);

            poolDictionary[tag].Enqueue(objectToSpawn);

            Debug.Log($"[ObjectPooler] Spawned object from pool '{tag}' at {position}");
            return objectToSpawn;
        }

        public GameObject SpawnFromPool(string tag)
        {
            return SpawnFromPool(tag, Vector3.zero, Quaternion.identity);
        }

        public void ReturnToPool(GameObject obj, string tag = null)
        {
            if (obj == null) return;

            if (string.IsNullOrEmpty(tag))
            {
                tag = FindPoolTagForObject(obj);
            }

            if (string.IsNullOrEmpty(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Could not find pool for object '{obj.name}'");
                return;
            }

            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist!");
                return;
            }

            obj.SetActive(false);

            ResetObjectState(obj);

            poolDictionary[tag].Enqueue(obj);

            Debug.Log($"[ObjectPooler] Returned object '{obj.name}' to pool '{tag}'");
        }

        private string FindPoolTagForObject(GameObject obj)
        {
            foreach (var kvp in poolDataDictionary)
            {
                if (obj.name.Contains(kvp.Value.prefab.name))
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        private void ResetObjectState(GameObject obj)
        {
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;

            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            BaseProjectile projectile = obj.GetComponent<BaseProjectile>();
            if (projectile != null)
            {
                projectile.ResetProjectile();
            }

            // Reset any other components that need resetting
            // Add more reset logic here as needed
        }

        
        public void AddPool(string tag, GameObject prefab, int size, Transform parent = null)
        {
            if (poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' already exists!");
                return;
            }

            Pool newPool = new Pool
            {
                tag = tag,
                prefab = prefab,
                size = size,
                parent = parent
            };

            CreatePool(newPool);
        }

        public void RemovePool(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist!");
                return;
            }

            Queue<GameObject> pool = poolDictionary[tag];
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            poolDictionary.Remove(tag);
            poolDataDictionary.Remove(tag);

            Debug.Log($"[ObjectPooler] Removed pool '{tag}'");
        }

        public int GetPoolSize(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
                return -1;

            return poolDictionary[tag].Count;
        }

        public bool PoolExists(string tag)
        {
            return poolDictionary.ContainsKey(tag);
        }

        public string[] GetAllPoolTags()
        {
            return new List<string>(poolDictionary.Keys).ToArray();
        }

        public void ClearAllPools()
        {
            foreach (var kvp in poolDictionary)
            {
                Queue<GameObject> pool = kvp.Value;
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }

            poolDictionary.Clear();
            poolDataDictionary.Clear();

            Debug.Log("[ObjectPooler] Cleared all pools");
        }
    }
}
