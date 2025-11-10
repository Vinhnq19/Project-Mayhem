using UnityEngine;
using ProjectMayhem.Player;
using ProjectMayhem.Projectiles;

namespace ProjectMayhem.Projectiles
{
    public class BombProjectile : BaseProjectile
    {
        [Header("Bomb Physics")]
        [SerializeField] private float bombGravityScale = 1f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float bounceForce = 0.3f;

        [Header("Explosion Settings")]
        [SerializeField] private float explosionDelay = 2f;
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private Vector2 explosionEffectOffset = new Vector2(0f, 0.5f);

        private bool hasLanded = false;
        private bool hasExploded = false;
        private float explosionTimer = 0f;

        protected override void Awake()
        {
            // Get components
            rb = GetComponent<Rigidbody2D>();
            projectileCollider = GetComponent<Collider2D>();

            rb.gravityScale = bombGravityScale;
            rb.drag = 0f;
            rb.angularDrag = 0.05f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            projectileCollider.isTrigger = false;

            Debug.Log("[BombProjectile] Awake - Physics collision enabled, trigger disabled");
        }

        public override void Initialize(BasePlayer owner, float damage, float knockback, Vector2 velocity)
        {
            this.owner = owner;
            this.damage = damage;
            this.knockback = knockback;
            this.velocity = velocity;

            // Set velocity (có thể là Vector2.zero nếu thả thẳng)
            rb.velocity = velocity;

            // Reset state
            currentLifetime = lifetime;
            hasLanded = false;
            hasExploded = false;
            explosionTimer = explosionDelay;

            // Enable trail if present
            if (trailRenderer != null)
                trailRenderer.enabled = true;

            Debug.Log($"[BombProjectile] Initialized - Will explode in {explosionDelay}s");
        }

        // Vật lý collision với đất/obstacles
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Check nếu chạm đất
            if (IsGround(collision.gameObject))
            {
                hasLanded = true;
                rb.velocity *= bounceForce;

                Debug.Log($"[BombProjectile] Landed on ground: {collision.gameObject.name}");
            }
        }

        // KHÔNG dùng trigger collision - bomb không gây damage khi chạm
        // Chỉ gây damage khi NỔ (explosion)
        protected override void OnTriggerEnter2D(Collider2D other)
        {
        }

        private bool IsGround(GameObject obj)
        {
            // Check layer
            if (groundLayer != 0 && (groundLayer.value & (1 << obj.layer)) != 0)
                return true;

            return false;
        }

        protected override void Update()
        {
            // Update lifetime
            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0f)
            {
                if (!hasExploded)
                {
                    Explode();
                }
                return;
            }

            // Đếm ngược explosion timer
            if (!hasExploded)
            {
                explosionTimer -= Time.deltaTime;
                
                if (explosionTimer <= 0f)
                {
                    // Hết thời gian → NỔ!
                    Explode();
                }
            }
        }

        /// <summary>
        /// Bomb phát nổ - gây damage cho tất cả player trong bán kính
        /// </summary>
        private void Explode()
        {
            if (hasExploded) return;
            
            hasExploded = true;

            Debug.Log($"[BombProjectile] EXPLODING at {transform.position} with radius {explosionRadius}");

            // Tìm tất cả player trong explosion radius
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, playerLayer);

            foreach (Collider2D col in hitColliders)
            {
                BasePlayer player = col.GetComponent<BasePlayer>();
                if (player == null) continue;
                if (ignoreOwner && player == owner) continue;

                // Tính khoảng cách để scale damage/knockback
                float distance = Vector2.Distance(transform.position, player.transform.position);
                float distanceRatio = 1f - (distance / explosionRadius);

                // Scale damage và knockback theo khoảng cách
                float scaledDamage = damage * distanceRatio;
                float scaledKnockback = knockback * distanceRatio;

                // Tính hướng knockback từ bomb đến player
                Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;

                // Apply damage
                if (player.Combat != null)
                {
                    player.Combat.TakeDamage(scaledDamage, scaledKnockback, knockbackDirection);
                    Debug.Log($"[BombProjectile] Hit {player.name} - Distance: {distance:F2}, Damage: {scaledDamage:F1}");
                }
            }

            // Play explosion effects
            PlayExplosionEffects();

            // Destroy bomb
            DestroyProjectile();
        }

        private void PlayExplosionEffects()
        {
            // Play explosion VFX
            if (explosionEffectPrefab != null)
            {
                SoundManager.Instance.PlaySfx(1);

                // Tính vị trí spawn effect với offset (đẩy lên trên)
                Vector3 effectPosition = transform.position + (Vector3)explosionEffectOffset;
                
                // Instantiate GameObject prefab tại vị trí đã offset
                GameObject effectObj = Instantiate(explosionEffectPrefab, effectPosition, Quaternion.identity);
                
                // Get ParticleSystem component và play
                ParticleSystem ps = effectObj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                    
                    // Auto destroy sau khi particle xong
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                    Destroy(effectObj, totalDuration);
                }
                else
                {
                    // Nếu không có ParticleSystem, destroy sau 2s
                    Debug.LogWarning("[BombProjectile] Explosion effect has no ParticleSystem component!");
                    Destroy(effectObj, 2f);
                }
            }
            else
            {
                Debug.LogWarning("[BombProjectile] No explosion effect prefab assigned!");
            }

            // Play explosion sound
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }

            // Có thể thêm camera shake, screen flash, etc.
        }

        public override void ResetProjectile()
        {
            base.ResetProjectile();
            hasLanded = false;
            hasExploded = false;
            explosionTimer = explosionDelay;
        }

        // Vẽ gizmo để visualize explosion radius trong editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
