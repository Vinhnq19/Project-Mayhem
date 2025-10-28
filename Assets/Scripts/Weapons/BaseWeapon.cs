using UnityEngine;
using ProjectMayhem.Player;
using ProjectMayhem.Manager;
using ProjectMayhem.Projectiles;
using ProjectMayhem.Utilities;

namespace ProjectMayhem.Weapons
{

    public abstract class BaseWeapon : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] protected float baseDamage = 10f;
        [SerializeField] protected float baseKnockback = 5f;
        [SerializeField] protected float fireRate = 1f;
        [SerializeField] protected int maxAmmo = 10;
        [SerializeField] protected float reloadTime = 2f;

        [Header("Audio Settings")]
        [SerializeField] protected AudioClip shootSound;
        [SerializeField] protected AudioClip reloadSound;
        [SerializeField] protected AudioClip emptySound;

        // Weapon state
        protected int currentAmmo;
        protected float lastFireTime;
        protected bool isReloading = false;
        protected BasePlayer owner;

        // Properties
        public float BaseDamage => baseDamage;
        public float BaseKnockback => baseKnockback;
        public float FireRate => fireRate;
        public int MaxAmmo => maxAmmo;
        public int CurrentAmmo => currentAmmo;
        public bool IsReloading => isReloading;
        public BasePlayer Owner => owner;
        public bool CanFire => !isReloading && currentAmmo > 0 && Time.time >= lastFireTime + (1f / fireRate);

        protected virtual void Awake()
        {
            owner = GetComponentInParent<BasePlayer>();
        }

        protected virtual void Start()
        {
            currentAmmo = maxAmmo;
            lastFireTime = 0f;
            isReloading = false;
        }

        public abstract void Use();

        public virtual void Reload()
        {
            if (isReloading || currentAmmo >= maxAmmo) return;

            isReloading = true;
            Invoke(nameof(FinishReload), reloadTime);

            if (reloadSound != null)
            {
                // EventBus.Raise(new PlaySoundEvent(reloadSound, transform.position));
                AudioSource.PlayClipAtPoint(reloadSound, transform.position);
            }

            Debug.Log($"[BaseWeapon] {GetType().Name} reloading...");
        }

        protected virtual void FinishReload()
        {
            currentAmmo = maxAmmo;
            isReloading = false;
            Debug.Log($"[BaseWeapon] {GetType().Name} reloaded. Ammo: {currentAmmo}/{maxAmmo}");
        }

        protected virtual void ConsumeAmmo()
        {
            currentAmmo--;
            lastFireTime = Time.time;
        }

        protected virtual void PlayShootSound()
        {
            if (shootSound != null)
            {
                // EventBus.Raise(new PlaySoundEvent(shootSound, transform.position));
                AudioSource.PlayClipAtPoint(shootSound, transform.position);
            }
        }

        protected virtual void PlayEmptySound()
        {
            if (emptySound != null)
            {
                // EventBus.Raise(new PlaySoundEvent(emptySound, transform.position));
                AudioSource.PlayClipAtPoint(emptySound, transform.position);
            }
        }

        public virtual void SetOwner(BasePlayer player)
        {
            owner = player;
        }

        public virtual string GetWeaponInfo()
        {
            return $"{GetType().Name} - Ammo: {currentAmmo}/{maxAmmo}, Damage: {baseDamage}, Knockback: {baseKnockback}";
        }
    }

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
}
