using UnityEngine;
using ProjectMayhem.Items;
using ProjectMayhem.Weapons;
using System.Collections.Generic;

namespace ProjectMayhem.Manager
{
    /// <summary>
    /// Spawns weapon crates randomly on the map
    /// Quản lý spawn hòm súng random trên bản đồ
    /// </summary>
    public class WeaponCrateSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject weaponCratePrefab;  // Prefab của RandomWeaponPickup (đã setup sẵn weapon pool trong prefab)
        [SerializeField] private Transform[] spawnPoints;  // Các vị trí spawn
        [SerializeField] private int maxCratesOnMap = 3;  // Số lượng hòm tối đa trên map
        
        [Header("Spawn Timing")]
        [SerializeField] private float initialSpawnDelay = 5f;  // Delay trước lần spawn đầu
        [SerializeField] private float spawnInterval = 15f;  // Thời gian giữa các lần spawn
        [SerializeField] private bool spawnOnStart = true;

        [Header("Spawn Rules")]
        [SerializeField] private bool avoidOccupiedSpawns = true;  // Không spawn ở vị trí đã có hòm
        [SerializeField] private float minDistanceBetweenCrates = 3f;  // Khoảng cách tối thiểu giữa các hòm
        [SerializeField] private LayerMask obstacleCheckMask;  // Layer để check vật cản

        private List<GameObject> activeCrates = new List<GameObject>();
        private float nextSpawnTime;

        private void Start()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[WeaponCrateSpawner] No spawn points assigned!");
                enabled = false;
                return;
            }

            if (weaponCratePrefab == null)
            {
                Debug.LogError("[WeaponCrateSpawner] No weapon crate prefab assigned!");
                enabled = false;
                return;
            }

            nextSpawnTime = Time.time + initialSpawnDelay;

            if (spawnOnStart)
            {
                // Spawn initial crates
                int initialCount = Mathf.Min(maxCratesOnMap, spawnPoints.Length);
                for (int i = 0; i < initialCount; i++)
                {
                    SpawnCrate();
                }
            }
        }

        private void Update()
        {
            // Remove destroyed crates from list
            activeCrates.RemoveAll(crate => crate == null);

            // Spawn new crate if needed
            if (Time.time >= nextSpawnTime && activeCrates.Count < maxCratesOnMap)
            {
                SpawnCrate();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        /// <summary>
        /// Spawn a weapon crate at random available location
        /// </summary>
        public GameObject SpawnCrate()
        {
            Transform spawnPoint = GetAvailableSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("[WeaponCrateSpawner] No available spawn point!");
                return null;
            }

            // Instantiate crate (weapon pool đã được setup trong prefab)
            GameObject crate = Instantiate(weaponCratePrefab, spawnPoint.position, Quaternion.identity);
            crate.name = $"WeaponCrate_{activeCrates.Count}";

            Debug.Log($"[WeaponCrateSpawner] Spawned crate at {spawnPoint.position}");

            activeCrates.Add(crate);
            return crate;
        }

        /// <summary>
        /// Get a random available spawn point
        /// </summary>
        private Transform GetAvailableSpawnPoint()
        {
            // Get list of available spawn points
            List<Transform> availablePoints = new List<Transform>();

            foreach (Transform point in spawnPoints)
            {
                if (IsSpawnPointAvailable(point))
                {
                    availablePoints.Add(point);
                }
            }

            if (availablePoints.Count == 0)
            {
                // If no point available, use any point
                return spawnPoints[Random.Range(0, spawnPoints.Length)];
            }

            // Return random available point
            return availablePoints[Random.Range(0, availablePoints.Count)];
        }

        /// <summary>
        /// Check if spawn point is available
        /// </summary>
        private bool IsSpawnPointAvailable(Transform point)
        {
            if (point == null) return false;

            // Check if there's already a crate nearby
            if (avoidOccupiedSpawns)
            {
                foreach (GameObject crate in activeCrates)
                {
                    if (crate == null) continue;

                    float distance = Vector3.Distance(point.position, crate.transform.position);
                    if (distance < minDistanceBetweenCrates)
                    {
                        return false;  // Too close to existing crate
                    }
                }
            }

            // Check for obstacles
            Collider2D obstacle = Physics2D.OverlapCircle(point.position, 0.5f, obstacleCheckMask);
            if (obstacle != null)
            {
                return false;  // Blocked by obstacle
            }

            return true;
        }

        /// <summary>
        /// Force spawn a crate at specific position
        /// </summary>
        public GameObject SpawnCrateAt(Vector3 position)
        {
            GameObject crate = Instantiate(weaponCratePrefab, position, Quaternion.identity);
            activeCrates.Add(crate);
            return crate;
        }

        /// <summary>
        /// Spawn multiple crates
        /// </summary>
        public void SpawnMultipleCrates(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (activeCrates.Count >= maxCratesOnMap) break;
                SpawnCrate();
            }
        }

        /// <summary>
        /// Clear all active crates
        /// </summary>
        public void ClearAllCrates()
        {
            foreach (GameObject crate in activeCrates)
            {
                if (crate != null)
                {
                    Destroy(crate);
                }
            }
            activeCrates.Clear();
        }

        /// <summary>
        /// Set max crates on map
        /// </summary>
        public void SetMaxCrates(int max)
        {
            maxCratesOnMap = Mathf.Max(0, max);
        }

        /// <summary>
        /// Set spawn interval
        /// </summary>
        public void SetSpawnInterval(float interval)
        {
            spawnInterval = Mathf.Max(1f, interval);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (spawnPoints == null) return;

            // Draw spawn points
            foreach (Transform point in spawnPoints)
            {
                if (point == null) continue;

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(point.position, 0.5f);
                Gizmos.DrawLine(point.position, point.position + Vector3.up);

                // Draw min distance circle
                Gizmos.color = new Color(0, 1, 1, 0.2f);
                UnityEditor.Handles.DrawWireDisc(point.position, Vector3.forward, minDistanceBetweenCrates);
            }

            // Draw active crates
            if (Application.isPlaying && activeCrates != null)
            {
                foreach (GameObject crate in activeCrates)
                {
                    if (crate == null) continue;
                    
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(crate.transform.position, 0.3f);
                }
            }
        }
#endif
    }
}
