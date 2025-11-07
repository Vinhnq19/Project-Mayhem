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
    [SerializeField] private float explosionDamage = 20f;
    [SerializeField] private float explosionKnockback = 10f;

    [Header("Object Pooling")]
    [SerializeField] private string bombPoolTag = "BombProjectile";
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
            explosionDamage = data.baseDamage;
            explosionKnockback = data.baseKnockback;
        }
    }

    public override void Use()
    {
        SpawnBomb();
    }

    private void SpawnBomb()
    {
        if (bombPrefab == null)
        {
            Debug.LogError("[BombWeapon] Bomb prefab not assigned!");
            return;
        }

        GameObject bombObj = null;
        
        if (objectPooler != null && !string.IsNullOrEmpty(bombPoolTag))
        {
            bombObj = objectPooler.SpawnFromPool(bombPoolTag, firePoint.position, firePoint.rotation);
            if (bombObj == null)
            {
                objectPooler.AddPool(bombPoolTag, bombPrefab.gameObject, 5);
                bombObj = objectPooler.SpawnFromPool(bombPoolTag, firePoint.position, firePoint.rotation);
            }
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
        if (bomb != null)
        {
            bomb.Initialize(player, explosionDamage, explosionKnockback, Vector2.zero);
            Debug.Log($"[BombWeapon] Spawned bomb at {firePoint.position}");
        }
    }
}

