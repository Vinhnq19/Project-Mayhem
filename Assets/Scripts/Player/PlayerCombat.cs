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

        [SerializeField] private Transform weaponHolder;  // Vị trí gắn weapon (child của player)
        [SerializeField] private BaseWeapon startingWeapon;  // Weapon ban đầu (optional)

        private BaseWeapon currentWeapon;
        private bool canShoot = true;



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

        private void Awake()
        {
            basePlayer = GetComponent<BasePlayer>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            currentDamagePercent = 0f;
            isInvulnerable = false;
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

            // // Decay damage over time
            // if (currentDamagePercent > 0f)
            // {
            //     currentDamagePercent -= damageDecayRate * Time.deltaTime;
            //     currentDamagePercent = Mathf.Max(0f, currentDamagePercent);
            // }
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
    }
}
