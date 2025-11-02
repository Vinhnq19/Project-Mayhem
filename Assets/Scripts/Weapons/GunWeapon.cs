using System.Collections;
using System.Collections.Generic;
using ProjectMayhem.Projectiles;
using ProjectMayhem.Utilities;
using ProjectMayhem.Weapons;
using UnityEngine;

public class GunWeapon : BaseWeapon
{
    [Header("Gun Settings")]
    [SerializeField] private BaseProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private int projectilesPerShot = 1;
    [SerializeField] private float spreadAngle = 0f;

    [Header("Object Pooling")]
    [SerializeField] private ObjectPooler objectPooler;

    protected override void Start()
    {
        base.Start();

        if (objectPooler == null)
            objectPooler = ObjectPooler.Instance;

        if (firePoint == null)
            firePoint = transform;
    }

    protected override void LoadFromWeaponData()
    {
        base.LoadFromWeaponData();
        if (weaponData != null)
        {
            projectilePrefab = weaponData.projectilePrefab;
            projectileSpeed = weaponData.projectileSpeed;
            projectilesPerShot = weaponData.projectilesPerShot;
            spreadAngle = weaponData.spreadAngle;
        }
    }

    public override void Use()
    {
        if (!CanFire) return;

        if (currentAmmo <= 0)
        {
            PlayEmptySound();
            return;
        }

        for (int i = 0; i < projectilesPerShot; i++)
        {
            FireProjectile();
        }

        ConsumeAmmo();
        PlayShootSound();

        Debug.Log($"[GunWeapon] Fired {projectilesPerShot} projectile(s). Ammo: {currentAmmo}/{maxAmmo}");
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[GunWeapon] Projectile prefab is not assigned!");
            return;
        }

        GameObject projectileObj;
        if (objectPooler != null)
        {
            projectileObj = objectPooler.SpawnFromPool(projectilePrefab.name, firePoint.position, firePoint.rotation);
        }
        else
        {
            projectileObj = Instantiate(projectilePrefab.gameObject, firePoint.position, firePoint.rotation);
        }

        if (projectileObj == null)
        {
            Debug.LogError("[GunWeapon] Failed to spawn projectile!");
            return;
        }

        BaseProjectile projectile = projectileObj.GetComponent<BaseProjectile>();
        if (projectile == null)
        {
            Debug.LogError("[GunWeapon] Projectile does not have BaseProjectile component!");
            return;
        }

        Vector2 direction = firePoint.right;
        if (spreadAngle > 0f)
        {
            float randomSpread = Random.Range(-spreadAngle, spreadAngle);
            direction = Quaternion.Euler(0, 0, randomSpread) * direction;
        }

        projectile.Initialize(owner, baseDamage, baseKnockback, direction * projectileSpeed);
    }

    public void SetProjectilePrefab(BaseProjectile prefab)
    {
        projectilePrefab = prefab;
    }

    public void SetFirePoint(Transform point)
    {
        firePoint = point;
    }

    public void SetProjectileSpeed(float speed)
    {
        projectileSpeed = speed;
    }
}