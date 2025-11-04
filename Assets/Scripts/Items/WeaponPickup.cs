using UnityEngine;
using ProjectMayhem.Player;
using ProjectMayhem.Manager;
using ProjectMayhem.Weapons;

namespace ProjectMayhem.Items
{
    /// <summary>
    /// Weapon pickup item (weapon crate/box) that gives player a new weapon
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WeaponPickup : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private BaseWeapon weaponPrefab;  // Optional: direct prefab reference

        [Header("Pickup Settings")]
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private float respawnTime = 15f;
        [SerializeField] private bool canRespawn = false;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem pickupEffect;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip pickupSound;

        [Header("Float Animation (Optional)")]
        [SerializeField] private bool enableFloating = true;
        [SerializeField] private float floatAmplitude = 0.3f;
        [SerializeField] private float floatSpeed = 2f;

        private bool isPickedUp = false;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float respawnTimer = 0f;
        private float floatTimer = 0f;

        private Collider2D itemCollider;

        public WeaponData WeaponData => weaponData;
        public bool IsPickedUp => isPickedUp;

        private void Awake()
        {
            itemCollider = GetComponent<Collider2D>();
            itemCollider.isTrigger = true;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null)
                animator = GetComponent<Animator>();

            originalPosition = transform.position;
            originalRotation = transform.rotation;
        }

        private void Start()
        {
            if (weaponData == null && weaponPrefab == null)
            {
                Debug.LogError($"[WeaponPickup] {gameObject.name} has no weapon assigned!");
                enabled = false;
                return;
            }

            // Set sprite from weapon data if available
            if (weaponData != null && weaponData.weaponIcon != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = weaponData.weaponIcon;
            }

            isPickedUp = false;
            respawnTimer = 0f;
            floatTimer = Random.Range(0f, Mathf.PI * 2f);  // Random start phase for variety
        }

        private void Update()
        {
            // Handle floating animation
            if (!isPickedUp && enableFloating)
            {
                floatTimer += Time.deltaTime * floatSpeed;
                float yOffset = Mathf.Sin(floatTimer) * floatAmplitude;
                transform.position = originalPosition + Vector3.up * yOffset;
            }

            // Handle respawn
            if (isPickedUp && canRespawn)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                {
                    RespawnItem();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPickedUp) return;

            BasePlayer player = other.GetComponent<BasePlayer>();
            if (player == null) return;

            GiveWeaponToPlayer(player);
        }

        private void GiveWeaponToPlayer(BasePlayer player)
        {
            if (player == null) return;

            PlayerCombat combat = player.Combat;
            if (combat == null)
            {
                Debug.LogWarning($"[WeaponPickup] Player {player.PlayerID} has no PlayerCombat component");
                return;
            }

            // Give weapon to player
            if (weaponData != null)
            {
                combat.EquipWeaponFromData(weaponData);
                Debug.Log($"[WeaponPickup] Player {player.PlayerID} picked up {weaponData.weaponName}");
            }
            else if (weaponPrefab != null)
            {
                combat.EquipWeapon(weaponPrefab, false);
                Debug.Log($"[WeaponPickup] Player {player.PlayerID} picked up {weaponPrefab.name}");
            }

            // Play effects
            PlayPickupEffects();

            // Emit event
            EventBus.Emit(GameEvent.ItemPickedUp, player, this);

            // Handle pickup
            HandleItemPickup();
        }

        private void PlayPickupEffects()
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            if (pickupEffect != null)
            {
                pickupEffect.transform.SetParent(null);  // Detach so it plays even after destroy
                pickupEffect.Play();
                Destroy(pickupEffect.gameObject, pickupEffect.main.duration + 1f);
            }

            if (animator != null)
            {
                animator.SetTrigger("Pickup");
            }
        }

        private void HandleItemPickup()
        {
            isPickedUp = true;

            if (destroyOnPickup)
            {
                DestroyItem();
            }
            else
            {
                HideItem();
            }

            if (canRespawn)
            {
                respawnTimer = respawnTime;
            }
        }

        private void HideItem()
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            itemCollider.enabled = false;
        }

        private void ShowItem()
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;

            itemCollider.enabled = true;
        }

        private void DestroyItem()
        {
            Destroy(gameObject);
        }

        private void RespawnItem()
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;

            ShowItem();

            isPickedUp = false;
            respawnTimer = 0f;
            floatTimer = Random.Range(0f, Mathf.PI * 2f);

            Debug.Log($"[WeaponPickup] {gameObject.name} respawned");
        }

        /// <summary>
        /// Set weapon data for this pickup
        /// </summary>
        public void SetWeaponData(WeaponData data)
        {
            weaponData = data;

            // Update sprite if possible
            if (data != null && data.weaponIcon != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = data.weaponIcon;
            }
        }

        /// <summary>
        /// Set weapon prefab for this pickup
        /// </summary>
        public void SetWeaponPrefab(BaseWeapon prefab)
        {
            weaponPrefab = prefab;
        }

        /// <summary>
        /// Set respawn settings
        /// </summary>
        public void SetRespawnSettings(bool canRespawn, float respawnTime)
        {
            this.canRespawn = canRespawn;
            this.respawnTime = respawnTime;
        }

        /// <summary>
        /// Force respawn this item
        /// </summary>
        public void ForceRespawn()
        {
            if (canRespawn)
            {
                RespawnItem();
            }
        }

        /// <summary>
        /// Get remaining respawn time
        /// </summary>
        public float GetRemainingRespawnTime()
        {
            return Mathf.Max(0f, respawnTimer);
        }

        /// <summary>
        /// Check if item is available for pickup
        /// </summary>
        public bool IsAvailableForPickup()
        {
            return !isPickedUp && (weaponData != null || weaponPrefab != null);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw pickup range
            Gizmos.color = isPickedUp ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
#endif
    }
}
