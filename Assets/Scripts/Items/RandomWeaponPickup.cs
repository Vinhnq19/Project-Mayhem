using UnityEngine;
using ProjectMayhem.Player;
using ProjectMayhem.Manager;
using ProjectMayhem.Weapons;

namespace ProjectMayhem.Items
{
    /// <summary>
    /// Mystery weapon crate that gives a random weapon when picked up
    /// Hòm súng random - cho weapon ngẫu nhiên khi nhặt
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RandomWeaponPickup : MonoBehaviour
    {
        [Header("Random Weapon Pool")]
        [SerializeField] private BaseWeapon[] possibleWeapons;  // List các weapon prefab có thể random
        [SerializeField] private float[] spawnWeights;  // Tỉ lệ spawn (optional)

        [Header("Pickup Settings")]
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private float respawnTime = 20f;
        [SerializeField] private bool canRespawn = false;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite mysteryBoxSprite;  // Sprite hòm bí ẩn (dấu ?)
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem pickupEffect;
        [SerializeField] private ParticleSystem idleEffect;  // Effect lúc đứng yên (sparkles)

        [Header("Audio Settings")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip revealSound;  // Âm thanh khi reveal weapon

        [Header("Float Animation")]
        [SerializeField] private bool enableFloating = true;
        [SerializeField] private float floatAmplitude = 0.3f;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private bool enableRotation = true;
        [SerializeField] private float rotationSpeed = 50f;

        [Header("Visual Feedback")]
        [SerializeField] private bool showWeaponPreview = true;  // Hiện weapon icon khi random
        [SerializeField] private float previewDuration = 1f;  // Thời gian hiện preview

        private bool isPickedUp = false;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float respawnTimer = 0f;
        private float floatTimer = 0f;

        private Collider2D itemCollider;
        private BaseWeapon selectedWeapon;  // Weapon prefab đã được random

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
            if (possibleWeapons == null || possibleWeapons.Length == 0)
            {
                Debug.LogError($"[RandomWeaponPickup] {gameObject.name} has no weapons in pool!");
                enabled = false;
                return;
            }

            // Set mystery box sprite (không fallback về weapon sprite)
            if (mysteryBoxSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = mysteryBoxSprite;
            }
            else if (spriteRenderer != null)
            {
                // Nếu không có mystery sprite thì warning
                Debug.LogWarning($"[RandomWeaponPickup] {gameObject.name} has no mystery box sprite assigned!");
            }

            // Start idle effect
            if (idleEffect != null)
            {
                idleEffect.Play();
            }

            isPickedUp = false;
            respawnTimer = 0f;
            floatTimer = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            // Float animation
            if (!isPickedUp && enableFloating)
            {
                floatTimer += Time.deltaTime * floatSpeed;
                float yOffset = Mathf.Sin(floatTimer) * floatAmplitude;
                transform.position = originalPosition + Vector3.up * yOffset;
            }

            // Rotation animation
            if (!isPickedUp && enableRotation)
            {
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }

            // Respawn timer
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

            GiveRandomWeaponToPlayer(player);
        }

        private void GiveRandomWeaponToPlayer(BasePlayer player)
        {
            if (player == null) return;

            PlayerCombat combat = player.Combat;
            if (combat == null)
            {
                Debug.LogWarning($"[RandomWeaponPickup] Player {player.PlayerID} has no PlayerCombat component");
                return;
            }

            // Random weapon from pool
            selectedWeapon = SelectRandomWeapon();
            
            if (selectedWeapon == null)
            {
                Debug.LogError("[RandomWeaponPickup] Failed to select random weapon!");
                return;
            }

            Debug.Log($"[RandomWeaponPickup] Player {player.PlayerID} got random weapon: {selectedWeapon.name}");

            // Show preview if enabled
            if (showWeaponPreview)
            {
                StartCoroutine(ShowWeaponPreviewCoroutine(player, combat));
            }
            else
            {
                // Give weapon immediately
                combat.EquipWeapon(selectedWeapon, false);  // false = pickup weapon (not starting)
                PlayPickupEffects();
                HandleItemPickup();
            }

            // Emit event
            EventBus.Emit(GameEvent.ItemPickedUp, player, this);
        }

        private BaseWeapon SelectRandomWeapon()
        {
            if (possibleWeapons.Length == 0) return null;

            // Filter out null weapons
            BaseWeapon[] validWeapons = System.Array.FindAll(possibleWeapons, w => w != null);
            if (validWeapons.Length == 0) return null;

            // If no weights specified, use equal probability
            if (spawnWeights == null || spawnWeights.Length != possibleWeapons.Length)
            {
                int randomIndex = Random.Range(0, validWeapons.Length);
                return validWeapons[randomIndex];
            }

            // Use weighted random selection
            float totalWeight = 0f;
            for (int i = 0; i < possibleWeapons.Length; i++)
            {
                if (possibleWeapons[i] != null)
                {
                    totalWeight += spawnWeights[i];
                }
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            for (int i = 0; i < possibleWeapons.Length; i++)
            {
                if (possibleWeapons[i] == null) continue;
                
                currentWeight += spawnWeights[i];
                if (randomValue <= currentWeight)
                {
                    return possibleWeapons[i];
                }
            }

            // Fallback
            return validWeapons[0];
        }

        private System.Collections.IEnumerator ShowWeaponPreviewCoroutine(BasePlayer player, PlayerCombat combat)
        {
            // Play reveal sound
            if (revealSound != null)
            {
                AudioSource.PlayClipAtPoint(revealSound, transform.position);
            }

            // Change sprite to weapon sprite
            if (selectedWeapon != null && spriteRenderer != null)
            {
                SpriteRenderer weaponSprite = selectedWeapon.GetComponent<SpriteRenderer>();
                if (weaponSprite != null && weaponSprite.sprite != null)
                {
                    spriteRenderer.sprite = weaponSprite.sprite;
                }
            }

            // Play reveal animation
            if (animator != null)
            {
                animator.SetTrigger("Reveal");
            }

            // Stop idle effect, play pickup effect
            if (idleEffect != null)
            {
                idleEffect.Stop();
            }

            if (pickupEffect != null)
            {
                pickupEffect.Play();
            }

            // Wait for preview duration
            yield return new WaitForSeconds(previewDuration);

            // Give weapon to player
            combat.EquipWeapon(selectedWeapon, false);  // false = pickup weapon

            // Play pickup sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Handle pickup
            HandleItemPickup();
        }

        private void PlayPickupEffects()
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            if (revealSound != null)
            {
                AudioSource.PlayClipAtPoint(revealSound, transform.position);
            }

            if (pickupEffect != null)
            {
                pickupEffect.transform.SetParent(null);
                pickupEffect.Play();
                Destroy(pickupEffect.gameObject, pickupEffect.main.duration + 1f);
            }

            if (idleEffect != null)
            {
                idleEffect.Stop();
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

            if (idleEffect != null)
                idleEffect.Stop();

            itemCollider.enabled = false;
        }

        private void ShowItem()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                // Reset to mystery box sprite
                if (mysteryBoxSprite != null)
                {
                    spriteRenderer.sprite = mysteryBoxSprite;
                }
            }

            if (idleEffect != null)
                idleEffect.Play();

            itemCollider.enabled = true;
        }

        private void DestroyItem()
        {
            Destroy(gameObject, 0.5f);  // Delay để effect kịp play
        }

        private void RespawnItem()
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;

            ShowItem();

            isPickedUp = false;
            respawnTimer = 0f;
            floatTimer = Random.Range(0f, Mathf.PI * 2f);
            selectedWeapon = null;

            Debug.Log($"[RandomWeaponPickup] {gameObject.name} respawned");
        }

        /// <summary>
        /// Add weapon prefab to possible pool
        /// </summary>
        public void AddWeaponToPool(BaseWeapon weapon, float weight = 1f)
        {
            if (weapon == null) return;

            System.Array.Resize(ref possibleWeapons, possibleWeapons.Length + 1);
            possibleWeapons[possibleWeapons.Length - 1] = weapon;

            if (spawnWeights != null && spawnWeights.Length > 0)
            {
                System.Array.Resize(ref spawnWeights, spawnWeights.Length + 1);
                spawnWeights[spawnWeights.Length - 1] = weight;
            }
        }

        /// <summary>
        /// Set weapon pool with equal weights
        /// </summary>
        public void SetWeaponPool(BaseWeapon[] weapons)
        {
            possibleWeapons = weapons;
            spawnWeights = null;  // Equal probability
        }

        /// <summary>
        /// Set weapon pool with custom weights
        /// </summary>
        public void SetWeaponPoolWithWeights(BaseWeapon[] weapons, float[] weights)
        {
            possibleWeapons = weapons;
            spawnWeights = weights;
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
        /// Force respawn
        /// </summary>
        public void ForceRespawn()
        {
            if (canRespawn)
            {
                RespawnItem();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw pickup range
            Gizmos.color = isPickedUp ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // Draw weapon pool info
            if (possibleWeapons != null && possibleWeapons.Length > 0)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, 
                    $"Random Pool: {possibleWeapons.Length} weapons");
            }
        }

        private void OnValidate()
        {
            // Validate weights array
            if (spawnWeights != null && spawnWeights.Length != possibleWeapons.Length)
            {
                Debug.LogWarning($"[RandomWeaponPickup] Spawn weights count ({spawnWeights.Length}) doesn't match weapons count ({possibleWeapons.Length}). Weights will be ignored.");
            }
        }
#endif
    }
}
