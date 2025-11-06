using UnityEngine;
using ProjectMayhem.Player;

namespace ProjectMayhem.Effects
{
    /// <summary>
    /// Xác định đối tượng mục tiêu của hiệu ứng
    /// </summary>
    public enum EffectTargetType
    {
        Self,
        Others
    }

    public abstract class BaseEffect : ScriptableObject
    {
        [Header("Effect Settings")]
        [SerializeField] protected float duration = 5f;
        [SerializeField] protected bool isBuff = true;
        [SerializeField] protected EffectTargetType targetType = EffectTargetType.Self;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected string effectName = "Effect";
        [SerializeField] protected string description = "Effect description";

        // Effect state
        protected float currentDuration;
        protected bool isActive = false;
        protected BasePlayer targetPlayer;

        // Properties
        public float Duration => duration;
        public bool IsBuff => isBuff;
        public EffectTargetType TargetType => targetType;
        public Sprite Icon => icon;
        public string EffectName => effectName;
        public string Description => description;
        public float CurrentDuration => currentDuration;
        public bool IsActive => isActive;
        public BasePlayer TargetPlayer => targetPlayer;

        public virtual void Apply(BasePlayer player)
        {
            if (player == null)
            {
                Debug.LogError("[BaseEffect] Cannot apply effect to null player");
                return;
            }

            targetPlayer = player;
            currentDuration = duration;
            isActive = true;

            OnApply(player);
            Debug.Log($"[BaseEffect] Applied {effectName} to Player {player.PlayerID} for {duration} seconds");
        }

        public virtual void Remove(BasePlayer player)
        {
            if (player == null || !isActive)
            {
                Debug.LogWarning("[BaseEffect] Cannot remove effect from null player or inactive effect");
                return;
            }

            OnRemove(player);
            isActive = false;
            currentDuration = 0f;
            targetPlayer = null;

            Debug.Log($"[BaseEffect] Removed {effectName} from Player {player.PlayerID}");
        }

        public virtual void UpdateEffect(BasePlayer player)
        {
            if (!isActive || player == null) return;

            currentDuration -= Time.deltaTime;
            OnUpdate(player);

            if (currentDuration <= 0f)
            {
                Remove(player);
            }
        }

        public virtual bool IsExpired()
        {
            return !isActive || currentDuration <= 0f;
        }

        public virtual void Reset()
        {
            isActive = false;
            currentDuration = 0f;
            targetPlayer = null;
        }

        public virtual void ExtendDuration(float additionalTime)
        {
            if (isActive)
            {
                currentDuration += additionalTime;
                Debug.Log($"[BaseEffect] Extended {effectName} duration by {additionalTime} seconds");
            }
        }

        public virtual float GetRemainingDurationPercentage()
        {
            if (!isActive || duration <= 0f) return 0f;
            return Mathf.Clamp01(currentDuration / duration);
        }

        protected abstract void OnApply(BasePlayer player);
        protected abstract void OnRemove(BasePlayer player);

        protected virtual void OnUpdate(BasePlayer player) { }

        public virtual BaseEffect CreateCopy()
        {
            BaseEffect copy = CreateInstance(GetType()) as BaseEffect;
            copy.duration = duration;
            copy.isBuff = isBuff;
            copy.targetType = targetType;
            copy.icon = icon;
            copy.effectName = effectName;
            copy.description = description;
            return copy;
        }
    
        public virtual bool ValidateSettings()
        {
            if (duration <= 0f)
            {
                Debug.LogError($"[BaseEffect] {effectName} has invalid duration: {duration}");
                return false;
            }

            if (string.IsNullOrEmpty(effectName))
            {
                Debug.LogError("[BaseEffect] Effect name cannot be null or empty");
                return false;
            }

            return true;
        }

        private void OnValidate()
        {
            ValidateSettings();
        }
    }
}
