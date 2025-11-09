using UnityEngine;
using ProjectMayhem.Manager;
using ProjectMayhem.Player;
using ProjectMayhem.Weapons;

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
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private BaseWeapon startingWeapon;

        private BaseWeapon currentWeapon;
        private BaseWeapon startingWeaponInstance;
        private bool canShoot = true;

        [Header("Bomb System (Special)")]
        [SerializeField] private BombWeapon bombWeaponPrefab;
        [SerializeField] private int maxBombsPerLife = 3;

        private BombWeapon bombWeaponInstance;
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

            if (startingWeapon != null)
            {
                EquipStartingWeapon();
            }

            // Initialize bomb system
            InitializeBombSystem();
        }

        private void InitializeBombSystem()
        {
            currentBombCount = maxBombsPerLife;

            if (bombWeaponPrefab != null && weaponHolder != null)
            {
                bombWeaponInstance = Instantiate(bombWeaponPrefab, weaponHolder);
                bombWeaponInstance.SetOwner(basePlayer);
                bombWeaponInstance.gameObject.SetActive(true);  // Luôn active
                
                Debug.Log($"[PlayerCombat] Player {playerID} initialized bomb weapon with {currentBombCount} bombs");
            }
            else
            {
                Debug.LogWarning($"[PlayerCombat] Player {playerID} has no bomb weapon prefab assigned!");
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
            // // Decay damage over time
            // if (currentDamagePercent > 0f)
            // {
            //     currentDamagePercent -= damageDecayRate * Time.deltaTime;
            //     currentDamagePercent = Mathf.Max(0f, currentDamagePercent);
            // }
        }

        private void EquipStartingWeapon()
        {
            if (startingWeapon == null || weaponHolder == null) return;

            // If we already have starting weapon instance, just switch to it
            if (startingWeaponInstance != null)
            {
                if (currentWeapon != null && currentWeapon != startingWeaponInstance)
                {
                    currentWeapon.gameObject.SetActive(false);
                }
                
                startingWeaponInstance.gameObject.SetActive(true);
                currentWeapon = startingWeaponInstance;
                
                Debug.Log($"[PlayerCombat] Player {playerID} switched back to starting weapon");
            }
            else
            {
                // First time, create the starting weapon instance
                startingWeaponInstance = Instantiate(startingWeapon, weaponHolder);
                startingWeaponInstance.SetOwner(basePlayer);
                startingWeaponInstance.SetAsStartingWeapon(true);
                currentWeapon = startingWeaponInstance;
                
                Debug.Log($"[PlayerCombat] Player {playerID} equipped starting weapon: {startingWeapon.name}");
            }

            EventBus.Emit(GameEvent.WeaponChanged, basePlayer, currentWeapon);
        }

        public void EquipWeapon(BaseWeapon weaponPrefab, bool isStartingWeapon = false)
        {
            if (weaponPrefab == null || weaponHolder == null) return;

            // If equipping starting weapon, use the dedicated method
            if (isStartingWeapon)
            {
                EquipStartingWeapon();
                return;
            }

            // Hide starting weapon but don't destroy it
            if (startingWeaponInstance != null)
            {
                startingWeaponInstance.gameObject.SetActive(false);
            }

            // Destroy previous pickup weapon (not starting weapon)
            if (currentWeapon != null && currentWeapon != startingWeaponInstance)
            {
                Destroy(currentWeapon.gameObject);
            }

            // Create new pickup weapon
            currentWeapon = Instantiate(weaponPrefab, weaponHolder);
            currentWeapon.SetOwner(basePlayer);
            currentWeapon.SetAsStartingWeapon(false);  // Pickup weapons are not starting weapons

            Debug.Log($"[PlayerCombat] Player {playerID} equipped pickup weapon: {weaponPrefab.name}");

            EventBus.Emit(GameEvent.WeaponChanged, basePlayer, currentWeapon);
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

                // Check if pickup weapon is out of ammo (not starting weapon)
                if (currentWeapon != startingWeaponInstance && 
                    currentWeapon.CurrentAmmo <= 0 && 
                    !currentWeapon.IsReloading)
                {
                    Debug.Log($"[PlayerCombat] Player {playerID} pickup weapon out of ammo, switching to starting weapon");
                    SwitchToStartingWeapon();
                }
            }
            else if (!canShoot)
            {
                Debug.Log($"[PlayerCombat] Player {playerID} is silenced!");
            }
        }

        /// <summary>
        /// Switch back to starting weapon (called when pickup weapon runs out of ammo)
        /// </summary>
        public void SwitchToStartingWeapon()
        {
            if (startingWeaponInstance == null)
            {
                Debug.LogWarning($"[PlayerCombat] Player {playerID} has no starting weapon to switch to!");
                return;
            }

            // Destroy current pickup weapon
            if (currentWeapon != null && currentWeapon != startingWeaponInstance)
            {
                // Destroy(currentWeapon.gameObject);
                DropWeapon(currentWeapon);
            }

            // Switch to starting weapon
            startingWeaponInstance.gameObject.SetActive(true);
            currentWeapon = startingWeaponInstance;

            Debug.Log($"[PlayerCombat] Player {playerID} switched back to starting weapon");

            EventBus.Emit(GameEvent.WeaponChanged, basePlayer, currentWeapon);
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

        //Drop weapon
        private void DropWeapon(BaseWeapon weaponToDrop)
        {
            if (weaponToDrop == null) return;
            weaponToDrop.transform.SetParent(null);
            Rigidbody2D weaponRb = weaponToDrop.GetComponent<Rigidbody2D>();
            if (weaponRb == null)
                weaponRb = weaponToDrop.gameObject.AddComponent<Rigidbody2D>();
            weaponRb.gravityScale = 3f;
            weaponRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; //fast moving objects

            float throwDirection = basePlayer.transform.localEulerAngles.y == 180f ? -1f : 1f;
            Vector2 throwForce = new Vector2(throwDirection * 5f, 10f);
            weaponRb.AddForce(throwForce, ForceMode2D.Impulse);
            weaponRb.angularVelocity = Random.Range(-360f, 360f);

            Destroy(weaponToDrop.gameObject, 3f);
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

            if (bombWeaponInstance == null)
            {
                Debug.LogWarning($"[PlayerCombat] Player {playerID} has no bomb weapon instance!");
                return;
            }

            // Spawn bomb projectile (BombWeapon.Use() handles spawning)
            bombWeaponInstance.Use();

            // Consume bomb count
            currentBombCount--;

            Debug.Log($"[PlayerCombat] Player {playerID} used bomb. Remaining: {currentBombCount}/{maxBombsPerLife}");

            // Emit event for UI update
            EventBus.Emit(GameEvent.BombUsed, basePlayer, currentBombCount);
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
