using UnityEngine;
using ProjectMayhem.Player;
using ProjectMayhem.Manager;
using ProjectMayhem.Utilities;

namespace ProjectMayhem.Projectiles
{
    /// <summary>
    /// Base class for all projectiles fired by weapons
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class BaseProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] protected float lifetime = 5f;
        [SerializeField] protected bool destroyOnHit = true;
        [SerializeField] protected LayerMask hitLayerMask = -1;
        [SerializeField] protected bool ignoreOwner = true;

        [Header("Visual Settings")]
        [SerializeField] protected ParticleSystem hitEffect;
        [SerializeField] protected TrailRenderer trailRenderer;


        [Header("Audio Settings")]
        [SerializeField] protected AudioClip hitSound;

        // Projectile data
        protected BasePlayer owner;
        protected float damage;
        protected float knockback;
        protected Vector2 velocity;
        protected float currentLifetime;

        // Components
        protected Rigidbody2D rb;
        protected Collider2D projectileCollider;

        // Properties
        public BasePlayer Owner => owner;
        public float Damage => damage;
        public float Knockback => knockback;
        public Vector2 Velocity => velocity;
        public bool IsActive => gameObject.activeInHierarchy;

        protected virtual void Awake()
        {
            // Get required components
            rb = GetComponent<Rigidbody2D>();
            projectileCollider = GetComponent<Collider2D>();

            // Set up rigidbody
            rb.gravityScale = 0f;
            rb.drag = 0f;
            rb.angularDrag = 0f;

            // Set up collider
            projectileCollider.isTrigger = true;
        }

        protected virtual void Start()
        {
            // Initialize projectile
            currentLifetime = lifetime;
        }

        protected virtual void Update()
        {
            // Update lifetime
            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0f)
            {
                DestroyProjectile();
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            // Check if we should ignore the owner
            if (ignoreOwner && owner != null && other.transform == owner.transform)
                return;

            // Check if we hit a valid target
            if (!IsValidTarget(other))
                return;

            // Handle hit
            HandleHit(other);
        }

        /// <summary>
        /// Initialize the projectile with data
        /// </summary>
        /// <param name="owner">Player who fired this projectile</param>
        /// <param name="damage">Damage this projectile deals</param>
        /// <param name="knockback">Knockback force this projectile applies</param>
        /// <param name="velocity">Initial velocity of the projectile</param>
        public virtual void Initialize(BasePlayer owner, float damage, float knockback, Vector2 velocity)
        {
            this.owner = owner;
            this.damage = damage;
            this.knockback = knockback;
            this.velocity = velocity;

            // Set velocity
            rb.velocity = velocity;

            // Reset lifetime
            currentLifetime = lifetime;

            // Enable trail if present
            if (trailRenderer != null)
                trailRenderer.enabled = true;

            Debug.Log($"[BaseProjectile] Initialized with damage: {damage}, knockback: {knockback}, velocity: {velocity}");
        }

        /// <summary>
        /// Check if the collider is a valid target
        /// </summary>
        /// <param name="other">Collider to check</param>
        /// <returns>True if target is valid</returns>
        protected virtual bool IsValidTarget(Collider2D other)
        {
            // Check layer mask
            if (hitLayerMask != -1 && (hitLayerMask.value & (1 << other.gameObject.layer)) == 0)
                return false;

            // Check if it's a player
            BasePlayer player = other.GetComponent<BasePlayer>();
            if (player == null)
                return false;

            // Check if it's not the owner
            if (ignoreOwner && player == owner)
                return false;

            return true;
        }

        /// <summary>
        /// Handle hitting a target
        /// </summary>
        /// <param name="other">Collider that was hit</param>
        protected virtual void HandleHit(Collider2D other)
        {
            BasePlayer hitPlayer = other.GetComponent<BasePlayer>();
            if (hitPlayer == null) return;

            //Check shield before applying damage
            if (hitPlayer.Combat != null && hitPlayer.Combat.HasShield)
            {
                Debug.Log("[BaseProjectile] Hit shield! Destroyed.");
                PlayHitEffects(transform.position);
                DestroyProjectile();
                return;
            }

            // Get player's combat component
            PlayerCombat playerCombat = hitPlayer.Combat;
            if (playerCombat == null)
            {
                Debug.LogWarning($"[BaseProjectile] Player {hitPlayer.PlayerID} has no PlayerCombat component");
                return;
            }

            // Calculate knockback direction - use projectile velocity direction for consistent knockback
            // This prevents weird knockback when projectile hits from behind (shotgun spread, etc)
            Vector2 knockbackDirection = velocity.normalized;
            
            // Fallback to position-based direction if velocity is zero
            if (knockbackDirection == Vector2.zero)
            {
                knockbackDirection = (hitPlayer.transform.position - transform.position).normalized;
            }

            // Apply damage and knockback
            playerCombat.TakeDamage(damage, knockback, knockbackDirection);

            // Play hit effects
            PlayHitEffects(hitPlayer.transform.position);

            Debug.Log($"[BaseProjectile] Hit Player {hitPlayer.PlayerID} for {damage} damage");

            // Destroy projectile if configured to do so
            if (destroyOnHit)
            {
                DestroyProjectile();
            }
        }

        /// <summary>
        /// Play hit effects
        /// </summary>
        /// <param name="hitPosition">Position where hit occurred</param>
        protected virtual void PlayHitEffects(Vector3 hitPosition)
        {
            // Play hit sound
            if (hitSound != null)
            {
                // EventBus.Raise(new PlaySoundEvent(hitSound, hitPosition));
                AudioSource.PlayClipAtPoint(hitSound, hitPosition);
            }

            // Play hit particle effect
            if (hitEffect != null)
            {
                hitEffect.transform.position = hitPosition;
                hitEffect.Play();
            }
        }

        /// <summary>
        /// Destroy the projectile
        /// </summary>
        protected virtual void DestroyProjectile()
        {
            // Disable trail
            if (trailRenderer != null)
                trailRenderer.enabled = false;

            // Return to pool or destroy
            ObjectPooler pooler = ObjectPooler.Instance;
            if (pooler != null)
            {
                pooler.ReturnToPool(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Set projectile lifetime
        /// </summary>
        /// <param name="newLifetime">New lifetime value</param>
        public virtual void SetLifetime(float newLifetime)
        {
            lifetime = newLifetime;
            currentLifetime = newLifetime;
        }

        /// <summary>
        /// Set projectile velocity
        /// </summary>
        /// <param name="newVelocity">New velocity vector</param>
        public virtual void SetVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
            rb.velocity = newVelocity;
        }

        /// <summary>
        /// Set projectile damage
        /// </summary>
        /// <param name="newDamage">New damage value</param>
        public virtual void SetDamage(float newDamage)
        {
            damage = newDamage;
        }

        /// <summary>
        /// Set projectile knockback
        /// </summary>
        /// <param name="newKnockback">New knockback value</param>
        public virtual void SetKnockback(float newKnockback)
        {
            knockback = newKnockback;
        }

        /// <summary>
        /// Set projectile owner
        /// </summary>
        /// <param name="newOwner">New owner player</param>
        public virtual void SetOwner(BasePlayer newOwner)
        {
            owner = newOwner;
        }

        /// <summary>
        /// Reset projectile to initial state
        /// </summary>
        public virtual void ResetProjectile()
        {
            // Reset velocity
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Reset lifetime
            currentLifetime = lifetime;

            // Disable trail
            if (trailRenderer != null)
                trailRenderer.enabled = false;

            // Clear owner
            owner = null;
        }

        /// <summary>
        /// Get remaining lifetime
        /// </summary>
        /// <returns>Remaining lifetime in seconds</returns>
        public virtual float GetRemainingLifetime()
        {
            return Mathf.Max(0f, currentLifetime);
        }

        /// <summary>
        /// Check if projectile is expired
        /// </summary>
        /// <returns>True if projectile has expired</returns>
        public virtual bool IsExpired()
        {
            return currentLifetime <= 0f;
        }

        /// <summary>
        /// Force destroy the projectile
        /// </summary>
        public virtual void ForceDestroy()
        {
            DestroyProjectile();
        }
    }
}
