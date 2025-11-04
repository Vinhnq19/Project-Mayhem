using System.Collections;
using System.Collections.Generic;
using ProjectMayhem.Effects;
using ProjectMayhem.Player;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponPickup", menuName = "ProjectMayhem/Effects/Weapon Pickup")]
public class WeaponPickupEffect : BaseEffect
{
    [Header("Weapon Settings")]
    public WeaponData weaponToGive;
    
    protected override void OnApply(BasePlayer player)
    {
        if (player == null || player.Combat == null) return;
        
        if (weaponToGive != null)
        {
            player.Combat.EquipWeaponFromData(weaponToGive);
            Debug.Log($"[WeaponPickupEffect] Gave {weaponToGive.weaponName} to Player {player.PlayerID}");
        }
        
        // Weapon pickup là instant effect, tự remove ngay
        Remove(player);
    }

    protected override void OnRemove(BasePlayer player)
    {
        // Nothing to remove
    }
}