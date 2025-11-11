using System.Collections.Generic;
using ProjectMayhem.Effects;
using UnityEngine;
using ProjectMayhem.Player;

namespace ProjectMayhem.Player
{
    public class PlayerEffectManager : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField] private int maxActiveEffects = 10;

        private List<BaseEffect> activeEffects = new List<BaseEffect>();
        private BasePlayer basePlayer;

        public List<BaseEffect> ActiveEffects => new List<BaseEffect>(activeEffects);
        public int ActiveEffectCount => activeEffects.Count;
        public bool HasActiveEffects => activeEffects.Count > 0;

        private void Awake()
        {
            basePlayer = GetComponent<BasePlayer>();
        }

        private void Start()
        {
            activeEffects.Clear();
        }

        private void Update()
        {
            UpdateActiveEffects();
        }

        public bool ApplyEffect(BaseEffect effectToApply)
        {
            if (effectToApply == null)
            {
                Debug.LogWarning("[PlayerEffectManager] Attempted to apply null effect");
                return false;
            }

            if (activeEffects.Count >= maxActiveEffects)
            {
                Debug.LogWarning($"[PlayerEffectManager] Maximum active effects ({maxActiveEffects}) reached");
                return false;
            }

            if (activeEffects.Contains(effectToApply))
            {
                Debug.LogWarning($"[PlayerEffectManager] Effect {effectToApply.name} is already active");
                return false;
            }

            effectToApply.Apply(basePlayer);
            activeEffects.Add(effectToApply);

            Debug.Log($"[PlayerEffectManager] Applied effect: {effectToApply.name}");
            return true;
        }

        public bool RemoveEffect(BaseEffect effect)
        {
            if (effect == null)
            {
                Debug.LogWarning("[PlayerEffectManager] Attempted to remove null effect");
                return false;
            }

            if (!activeEffects.Contains(effect))
            {
                Debug.LogWarning($"[PlayerEffectManager] Effect {effect.name} is not active");
                return false;
            }

            effect.Remove(basePlayer);
            activeEffects.Remove(effect);

            Debug.Log($"[PlayerEffectManager] Removed effect: {effect.name}");
            return true;
        }

        public void RemoveAllEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                BaseEffect effect = activeEffects[i];
                effect.Remove(basePlayer);
                activeEffects.RemoveAt(i);
            }

            Debug.Log("[PlayerEffectManager] Removed all active effects");
        }

        public bool HasEffect(BaseEffect effect)
        {
            return activeEffects.Contains(effect);
        }

        public List<T> GetEffectsOfType<T>() where T : BaseEffect
        {
            List<T> effectsOfType = new List<T>();
            
            foreach (BaseEffect effect in activeEffects)
            {
                if (effect is T effectOfType)
                {
                    effectsOfType.Add(effectOfType);
                }
            }

            return effectsOfType;
        }

        public T GetEffectOfType<T>() where T : BaseEffect
        {
            foreach (BaseEffect effect in activeEffects)
            {
                if (effect is T effectOfType)
                {
                    return effectOfType;
                }
            }

            return null;
        }

        public int RemoveEffectsOfType<T>() where T : BaseEffect
        {
            int removedCount = 0;

            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                BaseEffect effect = activeEffects[i];
                if (effect is T)
                {
                    effect.Remove(basePlayer);
                    activeEffects.RemoveAt(i);
                    removedCount++;
                }
            }

            Debug.Log($"[PlayerEffectManager] Removed {removedCount} effects of type {typeof(T).Name}");
            return removedCount;
        }

        private void UpdateActiveEffects()
        {
            List<BaseEffect> effectsToUpdate = new List<BaseEffect>(activeEffects);

            foreach (BaseEffect effect in effectsToUpdate)
            {
                if (effect != null)
                {
                    effect.UpdateEffect(basePlayer);
                    
                    if (effect.IsExpired())
                    {
                        RemoveEffect(effect);
                    }
                }
            }
        }

        public int GetEffectCountByType(bool isBuff)
        {
            int count = 0;
            foreach (BaseEffect effect in activeEffects)
            {
                if (effect.IsBuff == isBuff)
                {
                    count++;
                }
            }
            return count;
        }

        public int GetTotalEffectCount()
        {
            return activeEffects.Count;
        }

        public bool HasBuffEffects()
        {
            return GetEffectCountByType(true) > 0;
        }

        public bool HasDebuffEffects()
        {
            return GetEffectCountByType(false) > 0;
        }

        public void RemoveExpiredEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                BaseEffect effect = activeEffects[i];
                if (effect != null && effect.IsExpired())
                {
                    RemoveEffect(effect);
                }
            }
        }

        /// <summary>
        /// Clear all active effects (used when respawn)
        /// </summary>
        public void ClearAllEffects()
        {
            // Remove all effects in reverse order
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                BaseEffect effect = activeEffects[i];
                if (effect != null)
                {
                    effect.Remove(basePlayer);
                }
            }
            
            activeEffects.Clear();
            Debug.Log("[PlayerEffectManager] Cleared all active effects");
        }
    }
}
