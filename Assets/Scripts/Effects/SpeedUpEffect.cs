
using UnityEngine;
using ProjectMayhem.Player;

namespace ProjectMayhem.Effects
{
    [CreateAssetMenu(fileName = "SpeedUpEffect", menuName = "ProjectMayhem/Effects/Speed Up")]
    public class SpeedUpEffect : BaseEffect
    {
        [Header("Speed Settings")]
        public float speedMultiplier = 1.3f;  // +30% speed
        
        private float originalMultiplier;
        
        protected override void OnApply(BasePlayer player)
        {
            if (player == null) return;
            
            originalMultiplier = player.SpeedMultiplier;
            player.SetSpeedMultiplier(originalMultiplier * speedMultiplier);
            
            Debug.Log($"[SpeedUpEffect] Player {player.PlayerID} speed +30%");
        }
        
        protected override void OnRemove(BasePlayer player)
        {
            if (player == null) return;
            player.SetSpeedMultiplier(originalMultiplier);
        }
    }
}
