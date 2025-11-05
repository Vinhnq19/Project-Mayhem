using System.Collections;
using System.Collections.Generic;
using ProjectMayhem.Projectiles;
using ProjectMayhem.Utilities;
using ProjectMayhem.Weapons;
using UnityEngine;

public class GunWeapon : BaseWeapon
{

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

    public override void LoadFromWeaponData(WeaponData data)
    {
        base.LoadFromWeaponData(data);

        if (data != null)
        {
            projectilePrefab = data.projectilePrefab;
            projectileSpeed = data.projectileSpeed;
            projectilesPerShot = data.projectilesPerShot;
            Debug.Log($"[GunWeapon] Loaded gun-specific data from {data.weaponName}");
        }
    }

    public override void Use()
    {
        // If reloading, can't shoot
        if (isReloading) return;

        // If out of ammo, auto-reload
        if (currentAmmo <= 0)
        {
            // PlayEmptySound();
            TryAutoReload();
            return;
        }

        // Check fire rate
        if (Time.time < lastFireTime + (1f / fireRate)) return;

        for (int i = 0; i < projectilesPerShot; i++)
        {
            FireProjectile();
        }

        ConsumeAmmo();
        // PlayShootSound();

        Debug.Log($"[GunWeapon] Fired {projectilesPerShot} projectile(s). Ammo: {currentAmmo}/{maxAmmo}");

        // Auto-reload if just ran out of ammo
        if (currentAmmo <= 0)
        {
            TryAutoReload();
        }
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
        Vector2 direction = player.transform.localEulerAngles.y == 180f ? Vector2.left : Vector2.right;
        projectile.Initialize(player, baseDamage, baseKnockback, projectileSpeed * direction);
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