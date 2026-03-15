using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(WeaponAnimatonController))]
public class GunScript : Subject, IWorldSwitchListener
{
    [Header("Weapon Data")]
    [SerializeField, Tooltip("Create Weapon Data in Menu (Create/Custom Menu/Weapon Data)")]
    private WeaponData weaponData;

    [Header("Objects")]
    [SerializeField] private RaycastBullet raycastBullet;
    [SerializeField] private GameObject bulletSpawnPoint;
    [SerializeField] private UnityEvent shoot;
    [SerializeField] private UnityEvent<bool> enableCrystal;
    [SerializeField] private LayerMask opponentLayer;
    [SerializeField] private GameObject gunModel;

    private WeaponAnimatonController animator;

    public static Action reloading;
    public static Action<float, float> shot;
    public static Action startAnimationFinished;
    public static Action<int, int> updateCrystalInWeapon;

    private int maxAmmo;
    private int currentAmmo;
    private bool isReloading;
    private bool isTransitioning;
    private bool isInStartAnimation;
    private float currentEquipTime;
    private float currentReloadTime;
    private float xSpread;
    private float ySpread;
    private float nextFireTime;
    private bool pendingHeavenAnimation;
    private bool isInHellWorld;

    public int ammoReserve { get; set; }

    // Getters
    public GunType GetGunType => weaponData.gunType;
    public int MagazineCapacity => weaponData.magazineCapacity;
    public int CurrentAmmo => currentAmmo;
    public string WeaponName => weaponData.weaponName;
    public float CurrentReloadTime => currentReloadTime;
    public float ReloadTime => weaponData.reloadTime;
    public bool IsReloading => isReloading;
    public int AmmoReserve => ammoReserve;
    public int MaxAmmo => maxAmmo;

    public enum ReloadSoundIndex { CoverUp = 1, ClipIn = 2, CoverDown = 3, ClipOut = 4 }


    public void Start()
    {
        Initialize();
        OnSwitchWorld(isInHellWorld);
    }

    private void OnEnable()
    {
        WorldSwitcher.switchWorld += OnSwitchWorld;
        TutorialHandler.triggerStartAnimation += OnTriggerStartAnimation;
    }

    private void OnDisable()
    {
        WorldSwitcher.switchWorld -= OnSwitchWorld;
        TutorialHandler.triggerStartAnimation -= OnTriggerStartAnimation;
    }

    public void Update()
    {
        nextFireTime -= Time.deltaTime;

        if (isTransitioning)
        {
            currentEquipTime -= Time.deltaTime;
            if (currentEquipTime <= 0)
                isTransitioning = false;
        }
    }

    // Setup

    private void Initialize()
    {
        maxAmmo = weaponData.maxAmmo;
        currentAmmo = weaponData.currentAmmo;
        ammoReserve = weaponData.ammoInReserve;
        xSpread = weaponData.maxXSpread;
        ySpread = weaponData.maxYSpread;
        animator = GetComponent<WeaponAnimatonController>();
        updateAmmo?.Invoke(currentAmmo, ammoReserve, weaponData.magazineCapacity, weaponData.weaponName, isReloading, false);
    }

    // Shoot

    public void PullTrigger()
    {
        if (isReloading || currentAmmo <= 0 || !isInHellWorld || isTransitioning) return;
        Shoot();
    }

    public void Shoot()
    {
        if (!CanShoot()) return;

        FireBullets();
        shoot?.Invoke();
        UpdateAmmo(true);
        NotifyAmmoChanged();

        if (currentAmmo <= 0)
            Reload();
    }

    private bool CanShoot()
    {
        return nextFireTime < 0
            && currentAmmo > 0
            && gameObject.activeInHierarchy
            && !isTransitioning;
    }

    private void FireBullets()
    {
        for (int i = 0; i < weaponData.bulletsPerShot; i++)
        {
            ySpread = weaponData.maxYSpread;
            var newBullet = Instantiate(raycastBullet, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
            newBullet.Fire(xSpread, ySpread, opponentLayer, weaponData.damage);
        }

        if (!weaponData.infiniteAmmo)
            currentAmmo--;

        nextFireTime = weaponData.fireRate;
        animator.ForcePlay("Shooting");
        SoundManager.PlaySound(SoundType.SHOOT);
    }

    // Reload

    public void Reload()
    {
        if (!CanReload()) return;
        reloading?.Invoke();
        StartCoroutine(CoroutineReload());
    }

    private bool CanReload()
    {
        return ammoReserve > 0
            && currentAmmo < weaponData.magazineCapacity
            && !isTransitioning
            && !isReloading
            && isInHellWorld;
    }

    private void InstantReload()
    {
        reloading?.Invoke();
        ConsumeReloadAmmo();
        UpdateAmmo(true);
        enableCrystal?.Invoke(true);
    }

    private IEnumerator CoroutineReload()
    {
        isReloading = true;
        animator.ChangeAnimation("Reload", 0.0f, 0.0f);
        ConsumeReloadAmmo();
        UpdateAmmo(true);
        NotifyAmmoChanged();
        yield return new WaitForSeconds(weaponData.reloadTime);
        isReloading = false;

        if (pendingHeavenAnimation && !isInHellWorld)
        {
            animator.ChangeAnimation("SwitchToHeavenWorld", 0.5f);
            pendingHeavenAnimation = false;
        }
    }

    private void ConsumeReloadAmmo()
    {
        ammoReserve -= 2;
        currentAmmo = weaponData.magazineCapacity;
    }

    // Ammo

    public void AddAmmoAmount(int amount)
    {
        ammoReserve += amount;
        UpdateAmmo(false);
        if (currentAmmo == 0)
            InstantReload();
    }

    private void UpdateAmmo(bool updateCurrentCrystalImage)
    {
        currentAmmo = Mathf.Clamp(currentAmmo, 0, weaponData.magazineCapacity);
        ammoReserve = Mathf.Clamp(ammoReserve, 0, 9999);
        updateAmmo?.Invoke(currentAmmo, ammoReserve, weaponData.magazineCapacity, weaponData.weaponName, isReloading, updateCurrentCrystalImage);
    }

    private void NotifyAmmoChanged()
    {
        updateCrystalInWeapon?.Invoke(currentAmmo, ammoReserve);
        shot?.Invoke(ammoReserve, currentAmmo);
    }

    // Sound

    public void OnPlayReloadSound(int index)
    {
        var soundMap = new Dictionary<ReloadSoundIndex, SoundType>
        {
            { ReloadSoundIndex.CoverUp,   SoundType.COVERUP   },
            { ReloadSoundIndex.ClipIn,    SoundType.CLIPIN    },
            { ReloadSoundIndex.CoverDown, SoundType.COVERDOWN },
            { ReloadSoundIndex.ClipOut,   SoundType.CLIPOUT   },
        };

        if (soundMap.TryGetValue((ReloadSoundIndex)index, out var sound))
            SoundManager.PlaySoundWithDelay(sound, 0.5f, 0f);
    }

    // World Switch

    public void OnSwitchWorld(bool isInHellWorld)
    {
        if (isInStartAnimation) return;

        this.isInHellWorld = isInHellWorld;

        if (isInHellWorld && !isReloading)
            SoundManager.PlaySound(SoundType.DRAW);

        if (isReloading && !isInHellWorld)
        {
            pendingHeavenAnimation = true;
        }
        else if (!isReloading)
        {
            if (animator != null)
                animator.ForcePlay(isInHellWorld ? "SwitchToHellWorld" : "SwitchToHeavenWorld");

            isTransitioning = true;
            currentEquipTime = weaponData.equipTime;
        }
    }

    // Tutorial / Start Animation

    private void OnTriggerStartAnimation()
    {
        gunModel.SetActive(false);
        StartCoroutine(TriggerStartAnimationWithDelay());
        enableCrystal?.Invoke(false);
        isInStartAnimation = true;
    }

    private IEnumerator TriggerStartAnimationWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        gunModel.SetActive(true);
        animator.ForcePlay("GameStart");
    }

    public void OnEnableCrystalInStartAnim()
    {
        enableCrystal?.Invoke(true);
    }

    public void OnStartAnimationFinished()
    {
        startAnimationFinished?.Invoke();
        isInStartAnimation = false;
        OnSwitchWorld(isInHellWorld);
    }
}