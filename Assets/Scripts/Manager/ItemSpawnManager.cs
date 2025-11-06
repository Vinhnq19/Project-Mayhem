using UnityEngine;
using System.Collections.Generic;
using ProjectMayhem.Items;
using ProjectMayhem.Utilities;

namespace ProjectMayhem.Manager
{
    public class ItemSpawnManager : GenericSingleton<ItemSpawnManager>
    {
        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 10f;
        [SerializeField] private int maxItemsInScene = 5;
        [SerializeField] private bool autoSpawn = true;

        [Header("Item Prefab")]
        [SerializeField] private BaseItem itemPrefab;  // Generic item prefab

        [Header("Loot Table")]
        [SerializeField] private List<ItemDropProfile> possibleDrops = new List<ItemDropProfile>();

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

        [Header("Visual Effects")]
        [SerializeField] private GameObject spawnEffectPrefab;
        [SerializeField] private AudioClip spawnSound;

        // Runtime state
        private float spawnTimer;
        private List<BaseItem> activeItems = new List<BaseItem>();
        private ObjectPooler objectPooler;

        public int ActiveItemCount => activeItems.Count;
        public bool CanSpawn => activeItems.Count < maxItemsInScene && spawnPoints.Count > 0;

        protected override void Awake()
        {
            base.Awake();
            objectPooler = ObjectPooler.Instance;
        }

        private void Start()
        {
            spawnTimer = spawnInterval;

            if (spawnPoints.Count == 0)
            {
                GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("ItemSpawnPoint");
                foreach (GameObject go in spawnPointObjects)
                {
                    spawnPoints.Add(go.transform);
                }

                Debug.Log($"[ItemSpawnManager] Auto-found {spawnPoints.Count} spawn points");
            }

            ValidateLootTable();
        }

        private void Update()
        {
            if (!autoSpawn) return;

            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f && CanSpawn)
            {
                SpawnRandomItem();
                spawnTimer = spawnInterval;
            }
        }

        private void ValidateLootTable()
        {
            possibleDrops.RemoveAll(drop => drop == null);

            if (possibleDrops.Count == 0)
            {
                Debug.LogWarning("[ItemSpawnManager] Loot table is empty! No items will spawn.");
            }
            else
            {
                Debug.Log($"[ItemSpawnManager] Loot table contains {possibleDrops.Count} drop profiles");
            }
        }

        public void SpawnRandomItem()
        {
            if (possibleDrops.Count == 0 || spawnPoints.Count == 0)
            {
                Debug.LogWarning("[ItemSpawnManager] Cannot spawn - no drops or spawn points!");
                return;
            }

            // Select random drop profile (weighted)
            ItemDropProfile selectedDrop = SelectWeightedDrop();

            // Select random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            // Spawn item
            SpawnItem(selectedDrop, spawnPoint.position);
        }

        private ItemDropProfile SelectWeightedDrop()
        {
            // Calculate total weight
            float totalWeight = 0f;
            foreach (ItemDropProfile drop in possibleDrops)
            {
                totalWeight += drop.spawnWeight;
            }

            // Random value
            float randomValue = Random.Range(0f, totalWeight);

            // Select based on weight
            float currentWeight = 0f;
            foreach (ItemDropProfile drop in possibleDrops)
            {
                currentWeight += drop.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return drop;
                }
            }

            // Fallback
            return possibleDrops[0];
        }

        public void SpawnItem(ItemDropProfile dropProfile, Vector3 position)
        {
            if (dropProfile == null || itemPrefab == null) return;

            // Instantiate item
            BaseItem spawnedItem = Instantiate(itemPrefab, position, Quaternion.identity);

            // Configure item based on drop profile
            ConfigureItem(spawnedItem, dropProfile);

            // Track active item
            activeItems.Add(spawnedItem);

            // Spawn VFX
            PlaySpawnEffects(position);

            Debug.Log($"[ItemSpawnManager] Spawned {dropProfile.name} at {position}");
        }

        private void ConfigureItem(BaseItem item, ItemDropProfile dropProfile)
        {
            // Set the effect/weapon based on drop type
            if (dropProfile.dropType == DropType.Effect)
            {
                item.SetEffect(dropProfile.effectData);
            }
            else if (dropProfile.dropType == DropType.Weapon)
            {
                Debug.LogWarning("[ItemSpawnManager] Weapon drops not fully implemented yet!");
            }

            // Visual customization - Setup bubble (parent) và icon (child)
            SpriteRenderer bubbleRenderer = item.GetComponent<SpriteRenderer>();
            if (bubbleRenderer != null)
            {
                // Bubble giữ màu theo dropColor
                bubbleRenderer.color = dropProfile.dropColor;
            }
            Transform iconChild = item.transform.Find("iconChild");
            if (iconChild != null)
            {
            SpriteRenderer iconRenderer = iconChild.GetComponent<SpriteRenderer>();
                if (iconRenderer != null && dropProfile.dropIcon != null)
                {
                    iconRenderer.sprite = dropProfile.dropIcon;
                    iconRenderer.color = Color.white;
                }
                else if (iconRenderer == null)
                {
                    Debug.LogWarning("[ItemSpawnManager] iconChild exists but has no SpriteRenderer!");
                }
                else
                {
                    Debug.LogWarning($"[ItemSpawnManager] Item prefab missing 'iconChild' GameObject! Create a child named 'iconChild' with SpriteRenderer.");
                }
            }
            // Set respawn settings
            item.SetRespawnSettings(false, 0f);  // Items spawned by manager don't respawn themselves
        }

        private void PlaySpawnEffects(Vector3 position)
        {
            if (spawnEffectPrefab != null)
            {
                GameObject effect = Instantiate(spawnEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            if (spawnSound != null)
            {
                AudioSource.PlayClipAtPoint(spawnSound, position);
            }
        }

        public void RemoveItem(BaseItem item)
        {
            if (activeItems.Contains(item))
            {
                activeItems.Remove(item);
                Debug.Log($"[ItemSpawnManager] Removed item. Active items: {activeItems.Count}");
            }
        }

        public void ClearAllItems()
        {
            foreach (BaseItem item in activeItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            activeItems.Clear();
            Debug.Log("[ItemSpawnManager] Cleared all items");
        }

        public void SetSpawnInterval(float interval)
        {
            spawnInterval = Mathf.Max(1f, interval);
        }

        public void SetMaxItems(int max)
        {
            maxItemsInScene = Mathf.Max(1, max);
        }

        public void AddSpawnPoint(Transform point)
        {
            if (!spawnPoints.Contains(point))
            {
                spawnPoints.Add(point);
            }
        }

        public void RemoveSpawnPoint(Transform point)
        {
            spawnPoints.Remove(point);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (spawnPoints == null || spawnPoints.Count == 0) return;

            // Draw each spawn point with item previews
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                Transform point = spawnPoints[i];
                if (point == null) continue;

                // Main spawn point marker (màu xanh lá)
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(point.position, 0.5f);

                // Line pointing up
                Gizmos.DrawLine(point.position, point.position + Vector3.up * 1.5f);

                // Spawn area disc
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                UnityEditor.Handles.color = new Color(0, 1, 0, 0.3f);
                UnityEditor.Handles.DrawWireDisc(point.position, Vector3.forward, 1f);

            }

            // Draw active items in play mode
            if (Application.isPlaying && activeItems != null)
            {
                foreach (BaseItem item in activeItems)
                {
                    if (item == null) continue;

                    // Yellow marker for active items
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(item.transform.position, 0.4f);

                    Gizmos.color = new Color(1, 1, 0, 0.5f);
                    Gizmos.DrawSphere(item.transform.position, 0.3f);
                }
            }
        }
#endif
    }
}
