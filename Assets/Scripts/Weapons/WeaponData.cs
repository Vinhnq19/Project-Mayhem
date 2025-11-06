using System.Collections;
using System.Collections.Generic;
using ProjectMayhem.Projectiles;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectMayhem/Weapon Data", fileName = "Weapon - ")]

public class WeaponData : ScriptableObject
{
    [Header("Basic info")]
    public string weaponName;
    public Sprite weaponIcon;
    public WeaponType weaponType;

    [Header("Combat stats")]
    public float baseDamage = 10f;
    public float baseKnockback = 5f;
    public float fireRate = 1f; // Shots per second
    [Header("Ammo stats")]
    public int maxAmmo = 10;
    public float reloadTime = 2f; // Time to reload in seconds
    public bool infiniteAmmo = false;

    [Header("Projectile settings - for gun/bomb")]
    public BaseProjectile projectilePrefab;
    public float projectileSpeed = 20f;
    public int projectilesPerShot = 1;  // > 1 = shotgun spread

    [Header("Visual")]
    public GameObject impactEffectPrefab;

}

public enum WeaponType
{
    Gun, Bomb
}


