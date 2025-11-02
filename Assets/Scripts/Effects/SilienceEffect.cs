using UnityEngine;
using ProjectMayhem.Player;

namespace ProjectMayhem.Effects
{
    [CreateAssetMenu(fileName = "SilenceEffect", menuName = "ProjectMayhem/Effects/Silence")]
    public class SilenceEffect : BaseEffect
    {
        protected override void OnApply(BasePlayer player)
        {
            if (player == null || player.Combat == null) return;
            player.Combat.SetCanShoot(false);  // PlayerCombat đã có method này!
            Debug.Log($"[SilenceEffect] Player {player.PlayerID} silenced");
        }
        
        protected override void OnRemove(BasePlayer player)
        {
            if (player == null || player.Combat == null) return;
            player.Combat.SetCanShoot(true);
        }
    }
}
