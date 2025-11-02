using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectMayhem.Player;

namespace ProjectMayhem.Effects
{
    [CreateAssetMenu(fileName = "ReverseEffect", menuName = "ProjectMayhem/Effects/Reverse")]
    public class ReverseEffect : BaseEffect
    {
        protected override void OnApply(BasePlayer player)
        {
            if (player == null) return;
            player.SetMovementReversed(true);
            Debug.Log($"[ReverseEffect] Player {player.PlayerID} movement reversed");
        }

        protected override void OnRemove(BasePlayer player)
        {
            if (player == null) return;
            player.SetMovementReversed(false);
        }
    }

}
