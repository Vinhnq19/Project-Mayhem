using UnityEngine;
using ProjectMayhem.Player;
using ProjectMayhem.Effects;

namespace ProjectMayhem.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class BaseItem : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] private BaseEffect effectToApply;
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private float respawnTime = 10f;
        [SerializeField] private bool canRespawn = false;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem pickupEffect;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip pickupSound;

        private bool isPickedUp = false;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float respawnTimer = 0f;

        private Collider2D itemCollider;

        public BaseEffect EffectToApply => effectToApply;
        public bool IsPickedUp => isPickedUp;
        public bool CanRespawn => canRespawn;
        public float RespawnTime => respawnTime;

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
            if (effectToApply == null)
            {
                Debug.LogError($"[BaseItem] {gameObject.name} has no effect assigned!");
                enabled = false;
                return;
            }

            isPickedUp = false;
            respawnTimer = 0f;
        }

        private void Update()
        {
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

            ApplyEffectToPlayer(player);
        }

        private void ApplyEffectToPlayer(BasePlayer player)
        {
            if (player == null || effectToApply == null) return;

            PlayerEffectManager effectManager = player.EffectManager;
            if (effectManager == null)
            {
                Debug.LogWarning($"[BaseItem] Player {player.PlayerID} has no PlayerEffectManager component");
                return;
            }

            bool effectApplied = effectManager.ApplyEffect(effectToApply);
            
            if (effectApplied)
            {
                PlayPickupEffects();

                HandleItemPickup();

                Debug.Log($"[BaseItem] Player {player.PlayerID} picked up {effectToApply.EffectName}");
            }
            else
            {
                Debug.LogWarning($"[BaseItem] Failed to apply effect {effectToApply.EffectName} to Player {player.PlayerID}");
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

        private void PlayPickupEffects()
        {
            if (pickupSound != null)
            {
                // EventBus.Raise(new PlaySoundEvent(pickupSound, transform.position));
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            if (pickupEffect != null)
            {
                pickupEffect.Play();
            }

            if (animator != null)
            {
                animator.SetTrigger("Pickup");
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

            Debug.Log($"[BaseItem] {gameObject.name} respawned");
        }

        public void SetEffect(BaseEffect effect)
        {
            effectToApply = effect;
        }

        public void SetRespawnSettings(bool canRespawn, float respawnTime)
        {
            this.canRespawn = canRespawn;
            this.respawnTime = respawnTime;
        }

        public void ForceRespawn()
        {
            if (canRespawn)
            {
                RespawnItem();
            }
        }

        public float GetRemainingRespawnTime()
        {
            return Mathf.Max(0f, respawnTimer);
        }

        public bool IsAvailableForPickup()
        {
            return !isPickedUp && effectToApply != null;
        }

        public void SetPickupSound(AudioClip sound)
        {
            pickupSound = sound;
        }

        public void SetPickupEffect(ParticleSystem effect)
        {
            pickupEffect = effect;
        }
    }
}
