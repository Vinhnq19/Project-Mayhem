using System.Collections;
using System.Collections.Generic;
using ProjectMayhem.Projectiles;
using ProjectMayhem.Utilities;
using ProjectMayhem.Weapons;
using UnityEngine;

public class BombWeapon : BaseWeapon
{
    [Header("Bomb Settings")]
    [SerializeField] private BaseProjectile bombPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwAngle = 45f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 20f;
    [SerializeField] private float explosionKnockback = 10f;

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
            bombPrefab = data.projectilePrefab;
            throwForce = data.throwForce;
            throwAngle = data.throwAngle;
            explosionRadius = data.explosionRadius;
            explosionDamage = data.baseDamage;  // Dùng baseDamage cho explosion
            explosionKnockback = data.baseKnockback;

            Debug.Log($"[BombWeapon] Loaded bomb-specific data from {data.weaponName}");
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

        ThrowBomb();

        ConsumeAmmo();
        PlayShootSound();

        Debug.Log($"[BombWeapon] Threw bomb. Ammo: {currentAmmo}/{maxAmmo}");
    }

    private void ThrowBomb()
    {
        if (bombPrefab == null)
        {
            Debug.LogError("[BombWeapon] Bomb prefab is not assigned!");
            return;
        }

        GameObject bombObj;
        if (objectPooler != null)
        {
            bombObj = objectPooler.SpawnFromPool(bombPrefab.name, firePoint.position, firePoint.rotation);
        }
        else
        {
            bombObj = Instantiate(bombPrefab.gameObject, firePoint.position, firePoint.rotation);
        }

        if (bombObj == null)
        {
            Debug.LogError("[BombWeapon] Failed to spawn bomb!");
            return;
        }

        BaseProjectile bomb = bombObj.GetComponent<BaseProjectile>();
        if (bomb == null)
        {
            Debug.LogError("[BombWeapon] Bomb does not have BaseProjectile component!");
            return;
        }

        Vector2 throwDirection = CalculateThrowDirection();

        bomb.Initialize(owner, explosionDamage, explosionKnockback, throwDirection * throwForce);
    }

    private Vector2 CalculateThrowDirection()
    {
        float angleRad = throwAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        if (owner != null)
        {
            float facingDirection = Mathf.Sign(owner.transform.localScale.x);
            direction.x *= facingDirection;
        }

        return direction.normalized;
    }

    public void SetBombPrefab(BaseProjectile prefab)
    {
        bombPrefab = prefab;
    }

    public void SetThrowForce(float force)
    {
        throwForce = force;
    }

    public void SetThrowAngle(float angle)
    {
        throwAngle = angle;
    }

    public void SetExplosionRadius(float radius)
    {
        explosionRadius = radius;
    }
}

