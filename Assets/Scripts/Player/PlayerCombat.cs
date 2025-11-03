using UnityEngine;
using ProjectMayhem.Manager;
using ProjectMayhem.Player;
using ProjectMayhem.Weapons;
using System;

namespace ProjectMayhem.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private int playerID = 1;
        [SerializeField] private float maxDamagePercent = 200f;
        // [SerializeField] private float damageDecayRate = 10f; // Damage decay per second
        [SerializeField] private float invulnerabilityDuration = 0.05f;

        [Header("Knockback Settings")]
        [SerializeField] private float baseKnockbackMultiplier = 1f;
        [SerializeField] private float knockbackResistance = 1f;

        [Header("Weapon System")]
        [SerializeField] private Transform weaponHolder;  // Vị trí gắn weapon (child của player)
        [SerializeField] private BaseWeapon startingWeapon;  // Weapon ban đầu (optional)

        private BaseWeapon currentWeapon;
        private bool canShoot = true;

        [Header("Bomb System (Special)")]
        [SerializeField] private BombWeapon bombWeapon;  // Bomb prefab cố định cho player
        [SerializeField] private int maxBombsPerLife = 3;  // Số bomb mỗi mạng
        [SerializeField] private Transform bombSpawnPoint;  // Vị trí spawn bomb (optional)

        private int currentBombCount;



        // Combat state
        private float currentDamagePercent = 0f;
        private bool isInvulnerable = false;
        private float invulnerabilityTimer = 0f;
        private BasePlayer basePlayer;
        private Rigidbody2D rb;

        //Shield

        private bool hasShield = false;

        public bool HasShield => hasShield;

        // Properties
        public int PlayerID => playerID;
        public float CurrentDamagePercent => currentDamagePercent;
        public bool IsInvulnerable => isInvulnerable;
        public float MaxDamagePercent => maxDamagePercent;
        public BaseWeapon CurrentWeapon => currentWeapon;
        public bool CanShoot => canShoot;
        public int CurrentBombCount => currentBombCount;
        public int MaxBombsPerLife => maxBombsPerLife;

        private void Awake()
        {
            basePlayer = GetComponent<BasePlayer>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            currentDamagePercent = 0f;
            isInvulnerable = false;

            // Initialize weapon holder to face right (default)
            if (weaponHolder != null)
            {
                weaponHolder.localScale = Vector3.one;
            }

            if (startingWeapon != null)
            {
                EquipWeapon(startingWeapon);
            }

            // Initialize bomb system
            InitializeBombSystem();
        }

        private void InitializeBombSystem()
        {
            currentBombCount = maxBombsPerLife;

            // Setup bomb weapon if provided
            if (bombWeapon != null)
            {
                bombWeapon.SetOwner(basePlayer);
                
                // Set spawn point for bomb if not specified
                if (bombSpawnPoint == null && weaponHolder != null)
                {
                    bombSpawnPoint = weaponHolder;
                }

                Debug.Log($"[PlayerCombat] Player {playerID} initialized with {currentBombCount} bombs");
            }
            else
            {
                Debug.LogWarning($"[PlayerCombat] Player {playerID} has no bomb weapon assigned!");
            }
        }

        private void Update()
        {
            if (isInvulnerable)
            {
                invulnerabilityTimer -= Time.deltaTime;
                if (invulnerabilityTimer <= 0f)
                {
                    isInvulnerable = false;
                }
            }

            // Flip weapon based on player facing direction
            UpdateWeaponFlip();

            // // Decay damage over time
            // if (currentDamagePercent > 0f)
            // {
            //     currentDamagePercent -= damageDecayRate * Time.deltaTime;
            //     currentDamagePercent = Mathf.Max(0f, currentDamagePercent);
            // }
        }

        private void UpdateWeaponFlip()
        {
            if (weaponHolder == null || basePlayer == null) return;

            // Get player's move input to determine facing direction (same as PlayerAnimation)
            float moveX = basePlayer.MoveInput.x;
            
            // Only update facing when player has input
            if (Mathf.Abs(moveX) > 0.1f)
            {
                // Flip weapon holder: scale.x = -1 when moving left (same logic as sprite.flipX)
                float scaleX = (moveX < 0) ? -1f : 1f;
                weaponHolder.localScale = new Vector3(scaleX, 1f, 1f);
            }
        }

        public void EquipWeapon(BaseWeapon weaponPrefab)
        {
            if (currentWeapon != null)
            {
                Destroy(currentWeapon.gameObject);
            }

            if (weaponPrefab != null && weaponHolder != null)
            {
                currentWeapon = Instantiate(weaponPrefab, weaponHolder);
                currentWeapon.transform.localPosition = Vector3.zero;
                currentWeapon.SetOwner(basePlayer);

                Debug.Log($"[PlayerCombat] Player {playerID} equipped {weaponPrefab.name}");

                EventBus.Emit(GameEvent.WeaponChanged, basePlayer, currentWeapon);
            }
        }

        public void EquipWeaponFromData(WeaponData weaponData)
        {
            if (weaponData == null) return;
            BaseWeapon weaponPrefab = null;

            switch (weaponData.weaponType)
            {
                case WeaponType.Gun:
                    // Tạo GunWeapon prefab hoặc sử dụng prefab template
                    weaponPrefab = Resources.Load<GunWeapon>("Weapons/GunWeaponTemplate");
                    break;

                case WeaponType.Bomb:
                    weaponPrefab = Resources.Load<BombWeapon>("Weapons/BombWeaponTemplate");
                    break;
            }

            if (weaponPrefab != null)
            {
                EquipWeapon(weaponPrefab);
                // Load data vào weapon
                currentWeapon.LoadFromWeaponData(weaponData);
            }
        }

        public void UseCurrentWeapon()
        {
            if (currentWeapon != null && canShoot)
            {
                currentWeapon.Use();
            }
            else if (!canShoot)
            {
                Debug.Log($"[PlayerCombat] Player {playerID} is silenced!");
            }
        }

        public void TakeDamage(float baseDamage, float baseKnockback, Vector2 knockbackDirection)
        {
            if (isInvulnerable || hasShield)
            {
                Debug.Log($"[PlayerCombat] Player {playerID} is invulnerable, damage ignored");
                return;
            }

            currentDamagePercent += baseDamage;
            currentDamagePercent = Mathf.Min(currentDamagePercent, maxDamagePercent);

            float knockbackMultiplier = 1f + (currentDamagePercent / 100f);
            float finalKnockbackForce = baseKnockback * knockbackMultiplier * baseKnockbackMultiplier;

            Vector2 knockbackForce = knockbackDirection.normalized * finalKnockbackForce;
            rb.AddForce(knockbackForce, ForceMode2D.Impulse);

            StartInvulnerability();

            // EventBus.Raise(new PlayerDamageUpdatedEvent(playerID, currentDamagePercent));

            Debug.Log($"[PlayerCombat] Player {playerID} took {baseDamage} damage. Damage%: {currentDamagePercent:F1}%, Knockback: {finalKnockbackForce:F1}");
        }

        private void StartInvulnerability()
        {
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
        }

        public void Heal(float healAmount)
        {
            currentDamagePercent -= healAmount;
            currentDamagePercent = Mathf.Max(0f, currentDamagePercent);

            // EventBus.Raise(new PlayerDamageUpdatedEvent(playerID, currentDamagePercent));

            Debug.Log($"[PlayerCombat] Player {playerID} healed {healAmount} damage. New Damage%: {currentDamagePercent:F1}%");
        }

        public void ResetDamage()
        {
            currentDamagePercent = 0f;

            // EventBus.Raise(new PlayerDamageUpdatedEvent(playerID, currentDamagePercent));

            Debug.Log($"[PlayerCombat] Player {playerID} damage reset to 0%");
        }

        public void SetDamagePercent(float damagePercent)
        {
            currentDamagePercent = Mathf.Clamp(damagePercent, 0f, maxDamagePercent);

            // EventBus.Raise(new PlayerDamageUpdatedEvent(playerID, currentDamagePercent));

            Debug.Log($"[PlayerCombat] Player {playerID} damage set to {currentDamagePercent:F1}%");
        }

        public void ApplyKnockback(Vector2 knockbackForce)
        {
            Vector2 adjustedForce = knockbackForce * knockbackResistance;
            rb.AddForce(adjustedForce, ForceMode2D.Impulse);

            Debug.Log($"[PlayerCombat] Player {playerID} received knockback force: {adjustedForce}");
        }

        public float GetKnockbackMultiplier()
        {
            return 1f + (currentDamagePercent / 100f);
        }

        public bool IsHighDamage()
        {
            return currentDamagePercent > 100f;
        }

        public bool IsCriticalDamage()
        {
            return currentDamagePercent > 150f;
        }

        public void SetPlayerID(int id)
        {
            playerID = id;
        }

        public float GetNormalizedDamagePercent()
        {
            return currentDamagePercent / maxDamagePercent;
        }

        public void RemoveInvulnerability()
        {
            isInvulnerable = false;
            invulnerabilityTimer = 0f;
        }

        public void ExtendInvulnerability(float additionalTime)
        {
            if (isInvulnerable)
            {
                invulnerabilityTimer += additionalTime;
            }
            else
            {
                StartInvulnerability();
                invulnerabilityTimer = additionalTime;
            }
        }
        public void SetHasShield(bool hasShield)
        {
            this.hasShield = hasShield;
        }
        public void SetCanShoot(bool value)
        {
            canShoot = value;
            Debug.Log($"[PlayerCombat] Player {playerID} canShoot = {value}");
        }

        // ==================== BOMB SYSTEM ====================

        /// <summary>
        /// Use special bomb (called from BasePlayer.HandleSpecial)
        /// </summary>
        public void UseSpecialBomb()
        {
            if (currentBombCount <= 0)
            {
                Debug.Log($"[PlayerCombat] Player {playerID} has no bombs left!");
                return;
            }

            if (bombWeapon == null)
            {
                Debug.LogWarning($"[PlayerCombat] Player {playerID} has no bomb weapon!");
                return;
            }

            // Throw bomb
            ThrowBomb();

            // Consume bomb count
            currentBombCount--;

            Debug.Log($"[PlayerCombat] Player {playerID} used bomb. Remaining: {currentBombCount}/{maxBombsPerLife}");

            // Emit event for UI update
            EventBus.Emit(GameEvent.BombUsed, basePlayer, currentBombCount);
        }

        private void ThrowBomb()
        {
            // Temporarily set bomb weapon position for throw
            Vector3 originalPosition = bombWeapon.transform.position;
            Quaternion originalRotation = bombWeapon.transform.rotation;

            if (bombSpawnPoint != null)
            {
                bombWeapon.transform.position = bombSpawnPoint.position;
                bombWeapon.transform.rotation = bombSpawnPoint.rotation;
            }

            // Use bomb weapon (will handle spawning, pooling, physics)
            bombWeapon.Use();

            // Restore position (if bomb weapon is not instantiated each time)
            bombWeapon.transform.position = originalPosition;
            bombWeapon.transform.rotation = originalRotation;
        }

        /// <summary>
        /// Reset bomb count (called when respawn)
        /// </summary>
        public void ResetBombCount()
        {
            currentBombCount = maxBombsPerLife;
            Debug.Log($"[PlayerCombat] Player {playerID} bomb count reset to {currentBombCount}");
        }

        /// <summary>
        /// Add bombs (for powerup items)
        /// </summary>
        public void AddBombs(int amount)
        {
            currentBombCount += amount;
            Debug.Log($"[PlayerCombat] Player {playerID} gained {amount} bombs. Total: {currentBombCount}");

            EventBus.Emit(GameEvent.BombUsed, basePlayer, currentBombCount);
        }

        /// <summary>
        /// Set max bombs per life (for game settings)
        /// </summary>
        public void SetMaxBombsPerLife(int max)
        {
            maxBombsPerLife = Mathf.Max(0, max);
            currentBombCount = Mathf.Min(currentBombCount, maxBombsPerLife);
            Debug.Log($"[PlayerCombat] Player {playerID} max bombs set to {maxBombsPerLife}");
        }
    }
}
