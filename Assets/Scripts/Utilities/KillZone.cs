using UnityEngine;
using ProjectMayhem.Player;
using ProjectMayhem.Manager;

namespace ProjectMayhem.Utilities
{
    [RequireComponent(typeof(Collider2D))]
    public class KillZone : MonoBehaviour
    {
        [Header("Kill Zone Settings")]
        [SerializeField] private bool isActive = true;
        [SerializeField] private LayerMask playerLayerMask = 1;
        [SerializeField] private bool destroyPlayerOnKill = false;
        [SerializeField] private float killDelay = 0.1f;

        [Header("Visual Settings")]
        [SerializeField] private bool showDebugBounds = true;
        [SerializeField] private Color debugColor = Color.red;

        private Collider2D killZoneCollider;

        public bool IsActive => isActive;
        public Collider2D Collider => killZoneCollider;

        private void Awake()
        {
            killZoneCollider = GetComponent<Collider2D>();

            killZoneCollider.isTrigger = true;
        }

        private void Start()
        {
            if (killZoneCollider == null)
            {
                Debug.LogError("[KillZone] KillZone requires a Collider2D component!");
                enabled = false;
                return;
            }

            Debug.Log($"[KillZone] KillZone initialized at {transform.position}");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive) return;

            BasePlayer player = other.GetComponent<BasePlayer>();
            if (player == null) return;

            if (playerLayerMask != -1 && (playerLayerMask.value & (1 << other.gameObject.layer)) == 0)
                return;

            HandlePlayerKill(player);
        }

        private void HandlePlayerKill(BasePlayer player)
        {
            if (player == null) return;

            Debug.Log($"[KillZone] Player {player.PlayerID} entered kill zone at {transform.position}");

            // EventBus.Raise(new PlayerKilledEvent(player));

            if (destroyPlayerOnKill)
            {
                if (killDelay > 0f)
                {
                    Invoke(nameof(DestroyPlayer), killDelay);
                }
                else
                {
                    DestroyPlayer();
                }
            }
        }

        private void DestroyPlayer()
        {
            BasePlayer[] players = FindObjectsOfType<BasePlayer>();
            foreach (BasePlayer player in players)
            {
                if (killZoneCollider.bounds.Contains(player.transform.position))
                {
                    Debug.Log($"[KillZone] Destroying Player {player.PlayerID}");
                    Destroy(player.gameObject);
                    break;
                }
            }
        }

        public void SetActive(bool active)
        {
            isActive = active;
            Debug.Log($"[KillZone] Kill zone {(active ? "activated" : "deactivated")}");
        }

        public void SetPlayerLayerMask(LayerMask layerMask)
        {
            playerLayerMask = layerMask;
        }

        public void SetKillDelay(float delay)
        {
            killDelay = Mathf.Max(0f, delay);
        }

        public void SetDestroyPlayerOnKill(bool destroy)
        {
            destroyPlayerOnKill = destroy;
        }

        public Bounds GetKillZoneBounds()
        {
            return killZoneCollider.bounds;
        }

        public bool IsPositionInKillZone(Vector3 position)
        {
            return killZoneCollider.bounds.Contains(position);
        }

        public bool IsPlayerInKillZone(BasePlayer player)
        {
            if (player == null) return false;
            return IsPositionInKillZone(player.transform.position);
        }

        public BasePlayer[] GetPlayersInKillZone()
        {
            BasePlayer[] allPlayers = FindObjectsOfType<BasePlayer>();
            System.Collections.Generic.List<BasePlayer> playersInZone = new System.Collections.Generic.List<BasePlayer>();

            foreach (BasePlayer player in allPlayers)
            {
                if (IsPlayerInKillZone(player))
                {
                    playersInZone.Add(player);
                }
            }

            return playersInZone.ToArray();
        }

        public void ForceKillAllPlayersInZone()
        {
            BasePlayer[] playersInZone = GetPlayersInKillZone();
            
            foreach (BasePlayer player in playersInZone)
            {
                // EventBus.Raise(new PlayerKilledEvent(player));
                Debug.Log($"[KillZone] Force killed Player {player.PlayerID}");
            }
        }

        public void ResizeKillZone(Vector2 size)
        {
            if (killZoneCollider is BoxCollider2D boxCollider)
            {
                boxCollider.size = size;
            }
            else if (killZoneCollider is CircleCollider2D circleCollider)
            {
                circleCollider.radius = Mathf.Max(size.x, size.y) / 2f;
            }

            Debug.Log($"[KillZone] Resized kill zone to {size}");
        }

        public void MoveKillZone(Vector3 position)
        {
            transform.position = position;
            Debug.Log($"[KillZone] Moved kill zone to {position}");
        }

        private void OnDrawGizmos()
        {
            if (!showDebugBounds) return;

            Gizmos.color = debugColor;
            Gizmos.DrawWireCube(transform.position, killZoneCollider.bounds.size);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = debugColor;
            Gizmos.DrawCube(transform.position, killZoneCollider.bounds.size);
        }
    }
}
