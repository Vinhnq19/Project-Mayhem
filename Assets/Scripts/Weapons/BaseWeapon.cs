using UnityEngine;
using ProjectMayhem.Player;
using ProjectMayhem.Manager;
using ProjectMayhem.Projectiles;
using ProjectMayhem.Utilities;

namespace ProjectMayhem.Weapons
{

    public abstract class BaseWeapon : MonoBehaviour
    {
        [Header("Gun Settings")]
        [SerializeField] protected BaseProjectile projectilePrefab;
        [SerializeField] protected Transform firePoint;
        [SerializeField] protected float projectileSpeed = 20f;
        [SerializeField] protected int projectilesPerShot = 1;
        [Header("Weapon Settings")]
        [SerializeField] protected float baseDamage = 10f;
        [SerializeField] protected float baseKnockback = 5f;
        [SerializeField] protected float fireRate = 1f;
        [SerializeField] protected int maxAmmo = 10;
        [SerializeField] protected float reloadTime = 2f;

        [Header("Weapon Settings")]
        [SerializeField] protected WeaponData weaponData;

        // Weapon state
        protected int currentAmmo;
        protected float lastFireTime;
        protected bool isReloading = false;
        protected BasePlayer player;
        protected bool isStartingWeapon = false;  // Track if this is the starting weapon (infinite ammo with reload)

        // Properties
        public float BaseDamage => baseDamage;
        public float BaseKnockback => baseKnockback;
        public float FireRate => fireRate;
        public int MaxAmmo => maxAmmo;
        public int CurrentAmmo => currentAmmo;
        public bool IsReloading => isReloading;
        public BasePlayer Owner => player;
        public bool CanFire => !isReloading && currentAmmo > 0 && Time.time >= lastFireTime + (1f / fireRate);

        protected virtual void Awake()
        {
            player = GetComponentInParent<BasePlayer>();
        }

        protected virtual void Start()
        {
            if (weaponData != null)
            {
                LoadFromWeaponData(weaponData);
            }
            else
            {

                currentAmmo = maxAmmo;
                lastFireTime = 0f;
                isReloading = false;
            }
        }

        public abstract void Use();

        public virtual void LoadFromWeaponData(WeaponData data)
        {
            if (data == null) return;

            weaponData = data;
            baseDamage = data.baseDamage;
            baseKnockback = data.baseKnockback;
            fireRate = data.fireRate;
            maxAmmo = data.maxAmmo;
            reloadTime = data.reloadTime;

            currentAmmo = maxAmmo;
            lastFireTime = 0f;
            isReloading = false;

            Debug.Log($"[BaseWeapon] Loaded weapon data: {data.weaponName}");
        }

        public virtual void Reload()
        {
            if (isReloading || currentAmmo >= maxAmmo) return;

            isReloading = true;
            Invoke(nameof(FinishReload), reloadTime);

            // if (reloadSound != null)
            // {
            //     AudioSource.PlayClipAtPoint(reloadSound, transform.position);
            // }

            // Emit reload started event
            EventBus.Emit(GameEvent.WeaponReloadStarted, player, this);

            Debug.Log($"[BaseWeapon] {GetType().Name} reloading...");
        }

        protected virtual void FinishReload()
        {
            currentAmmo = maxAmmo;
            isReloading = false;

            // Emit reload finished event
            EventBus.Emit(GameEvent.WeaponReloaded, player, this);

            Debug.Log($"[BaseWeapon] {GetType().Name} reloaded. Ammo: {currentAmmo}/{maxAmmo}");
        }

        protected virtual void TryAutoReload()
        {
            // Only auto-reload for starting weapon
            if (isStartingWeapon && currentAmmo <= 0 && !isReloading)
            {
                Reload();
            }
        }

        /// <summary>
        /// Mark this weapon as the starting weapon (enables auto-reload)
        /// </summary>
        public virtual void SetAsStartingWeapon(bool isStarting)
        {
            isStartingWeapon = isStarting;
            Debug.Log($"[BaseWeapon] {GetType().Name} set as starting weapon: {isStarting}");
        }

        protected virtual void ConsumeAmmo()
        {
            currentAmmo--;
            lastFireTime = Time.time;
        }

        public virtual void SetOwner(BasePlayer player)
        {
            this.player = player;
        }

        public virtual string GetWeaponInfo()
        {
            string name = weaponData != null ? weaponData.weaponName : GetType().Name;
            return $"{name} - Ammo: {currentAmmo}/{maxAmmo}, Damage: {baseDamage}, Knockback: {baseKnockback}";
        }
        public void SetCurrentAmmo(int amount)
        {
            currentAmmo = Mathf.Clamp(amount, 0, maxAmmo * 2);
            Debug.Log($"[BaseWeapon] Ammo set to {currentAmmo}");
        }
    }
}



