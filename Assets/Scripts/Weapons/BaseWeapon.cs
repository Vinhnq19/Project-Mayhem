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

        [Header("Weapon Settings")]
        [SerializeField] protected WeaponData weaponData;

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

        protected virtual void LoadFromWeaponData()
        {
            baseDamage = weaponData.baseDamage;
            baseKnockback = weaponData.baseKnockback;
            fireRate = weaponData.fireRate;
            maxAmmo = weaponData.maxAmmo;
            reloadTime = weaponData.reloadTime;
            shootSound = weaponData.shootSound;
            reloadSound = weaponData.reloadSound;
            emptySound = weaponData.emptySound;
            currentAmmo = maxAmmo;
        }

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
        public void SetCurrentAmmo(int amount)
        {
            currentAmmo = Mathf.Clamp(amount, 0, maxAmmo * 2);
            Debug.Log($"[BaseWeapon] Ammo set to {currentAmmo}");
        }
    }
}



