using UnityEngine;
using ProjectMayhem.Player;

namespace ProjectMayhem.Effects
{
    [CreateAssetMenu(fileName = "ShieldEffect", menuName = "ProjectMayhem/Effects/Shield")]
    public class ShieldEffect : BaseEffect
    {
        [Header("Shield VFX")]
        public GameObject shieldVisualPrefab;
        
        private GameObject shieldInstance;
        
        protected override void OnApply(BasePlayer player)
        {
            if (player == null || player.Combat == null) return;
            
            player.Combat.SetHasShield(true);
            
            if (shieldVisualPrefab != null)
            {
                shieldInstance = Instantiate(shieldVisualPrefab, player.transform);
                shieldInstance.transform.localPosition = Vector3.zero;
            }
            
            Debug.Log($"[ShieldEffect] Player {player.PlayerID} shield ON");
        }
        
        protected override void OnRemove(BasePlayer player)
        {
            if (player == null || player.Combat == null) return;
            
            player.Combat.SetHasShield(false);
            if (shieldInstance != null) Destroy(shieldInstance);
        }
    }
}