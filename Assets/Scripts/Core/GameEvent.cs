using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEvent
{
    PlayerChangeState,
    WeaponChanged,
    WeaponFired,
    WeaponReloadStarted,  // (BasePlayer player, BaseWeapon weapon)
    WeaponReloaded,       // (BasePlayer player, BaseWeapon weapon)
    WeaponOutOfAmmo,
    EffectApplied,
    EffectRemoved,
    ItemSpawned,
    ItemPickedUp,
    ItemDestroyed,
    BombUsed  // (BasePlayer player, int remainingBombs)
}