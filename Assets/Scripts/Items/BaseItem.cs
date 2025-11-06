using UnityEngine;
using ProjectMayhem.Player;
using ProjectMayhem.Effects;
using ProjectMayhem.Manager;

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

            bool effectApplied = false;

            switch (effectToApply.TargetType)
            {
                case EffectTargetType.Self:
                    // Áp dụng lên người nhặt (buff bản thân)
                    effectApplied = ApplyEffectToTarget(player);
                    if (effectApplied)
                    {
                        Debug.Log($"[BaseItem] Player {player.PlayerID} picked up SELF effect: {effectToApply.EffectName}");
                    }
                    break;

                case EffectTargetType.Others:
                    // Áp dụng lên tất cả người khác (không phải người nhặt)
                    BasePlayer[] allPlayers = FindObjectsOfType<BasePlayer>();
                    int affectedCount = 0;
                    
                    foreach (BasePlayer targetPlayer in allPlayers)
                    {
                        // Bỏ qua người nhặt
                        if (targetPlayer.PlayerID == player.PlayerID) continue;

                        if (ApplyEffectToTarget(targetPlayer))
                        {
                            affectedCount++;
                        }
                    }

                    effectApplied = affectedCount > 0;
                    if (effectApplied)
                    {
                        Debug.Log($"[BaseItem] Player {player.PlayerID} picked up OTHERS effect: {effectToApply.EffectName} - Affected {affectedCount} players");
                    }
                    break;

                // case EffectTargetType.AllPlayers:
                //     // Áp dụng lên tất cả người chơi (bao gồm cả người nhặt)
                //     BasePlayer[] allPlayersIncludingSelf = FindObjectsOfType<BasePlayer>();
                //     int totalAffected = 0;
                    
                //     foreach (BasePlayer targetPlayer in allPlayersIncludingSelf)
                //     {
                //         if (ApplyEffectToTarget(targetPlayer))
                //         {
                //             totalAffected++;
                //         }
                //     }

                //     effectApplied = totalAffected > 0;
                //     if (effectApplied)
                //     {
                //         Debug.Log($"[BaseItem] Player {player.PlayerID} picked up ALL PLAYERS effect: {effectToApply.EffectName} - Affected {totalAffected} players");
                //     }
                //     break;
            }
            
            if (effectApplied)
            {
                PlayPickupEffects();
                HandleItemPickup();
            }
            else
            {
                Debug.LogWarning($"[BaseItem] Failed to apply effect {effectToApply.EffectName}");
            }
        }

        /// <summary>
        /// Áp dụng effect lên một player cụ thể
        /// </summary>
        private bool ApplyEffectToTarget(BasePlayer targetPlayer)
        {
            if (targetPlayer == null) return false;

            PlayerEffectManager effectManager = targetPlayer.EffectManager;
            if (effectManager == null)
            {
                Debug.LogWarning($"[BaseItem] Player {targetPlayer.PlayerID} has no PlayerEffectManager component");
                return false;
            }

            return effectManager.ApplyEffect(effectToApply);
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
            // if (pickupSound != null)
            // {
            //     // EventBus.Raise(new PlaySoundEvent(pickupSound, transform.position));
            //     AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            // }

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

        private void OnDestroy()
{
    // Notify spawn manager when destroyed
    ItemSpawnManager spawnManager = ItemSpawnManager.Instance;
    if (spawnManager != null)
    {
        spawnManager.RemoveItem(this);
    }
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

        // public void SetPickupSound(AudioClip sound)
        // {
        //     pickupSound = sound;
        // }

        public void SetPickupEffect(ParticleSystem effect)
        {
            pickupEffect = effect;
        }
    }
}
