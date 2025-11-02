using UnityEngine;
using ProjectMayhem.Player;

namespace ProjectMayhem.Effects
{
    [CreateAssetMenu(fileName = "TripleJumpEffect", menuName = "ProjectMayhem/Effects/Triple Jump")]
    public class TripleJumpEffect : BaseEffect
    {
        protected override void OnApply(BasePlayer player)
        {
            if (player == null) return;
            player.EnableTripleJump();  // BasePlayer đã có method này!
            Debug.Log($"[TripleJumpEffect] Player {player.PlayerID} triple jump ON");
        }
        
        protected override void OnRemove(BasePlayer player)
        {
            if (player == null) return;
            player.DisableTripleJump();  // BasePlayer đã có method này!
        }
    }
}