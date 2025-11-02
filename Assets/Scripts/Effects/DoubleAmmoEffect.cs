using UnityEngine;
using ProjectMayhem.Player;
using ProjectMayhem.Weapons;

namespace ProjectMayhem.Effects
{
    [CreateAssetMenu(fileName = "DoubleAmmoEffect", menuName = "Project Mayhem/Effects/Double Ammo")]
    public class DoubleAmmoEffect : BaseEffect
    {
        protected override void OnApply(BasePlayer player)
        {
            if (player == null || player.Combat == null) return;
            
            BaseWeapon weapon = player.Combat.CurrentWeapon;
            if (weapon != null)
            {
                int newAmmo = Mathf.Min(weapon.MaxAmmo * 2, weapon.CurrentAmmo * 2);
                weapon.SetCurrentAmmo(newAmmo);
                Debug.Log($"[DoubleAmmoEffect] Ammo doubled: {newAmmo}");
            }
            
            // Instant effect - tự remove
            Remove(player);
        }
        
        protected override void OnRemove(BasePlayer player)
        {
            // Nothing to do
        }
    }
}