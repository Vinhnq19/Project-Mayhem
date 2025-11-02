using UnityEngine;
using ProjectMayhem.Player;

namespace ProjectMayhem.Effects
{
    [CreateAssetMenu(fileName = "SlowDownEffect", menuName = "ProjectMayhem/Effects/Slow Down")]
    public class SlowDownEffect : BaseEffect
    {
        [Header("Speed Settings")]
        public float speedMultiplier = 0.7f;  // -30% speed
        
        private float originalMultiplier;
        
        protected override void OnApply(BasePlayer player)
        {
            if (player == null) return;
            
            originalMultiplier = player.SpeedMultiplier;
            player.SetSpeedMultiplier(originalMultiplier * speedMultiplier);
            
            Debug.Log($"[SlowDownEffect] Player {player.PlayerID} speed -30%");
        }
        
        protected override void OnRemove(BasePlayer player)
        {
            if (player == null) return;
            player.SetSpeedMultiplier(originalMultiplier);
        }
    }
}