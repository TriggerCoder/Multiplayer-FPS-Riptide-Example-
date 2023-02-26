using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "weapon", menuName = "ModernFPS/Weapon", order = 1)]
public class WeaponFile : ScriptableObject
{
    [Header("Weapon Properties:")]
    public string WeaponName;
    public string WeaponLoadName;
    public weapontype WeaponType;
    public firingmode FiringMode;
    [Tooltip("How does the third person model hold this weapon?")]
    public weaponholdtype WeaponHoldType = weaponholdtype.Rifle;
    [Tooltip("The aiming mode for the weapon, use standard for most weapons")]
    public ADSMode WeaponAimMode;
    [Tooltip("The localposition of the weapon while aiming.")]
    public Vector3 WeaponADSPosition;
    [Tooltip("The localposition of the weapon.")]
    public Vector3 WeaponPosition;
    [Tooltip("The fire rate of the weapon in rounds per minute")]
    public int WeaponFireRate;
    [Tooltip("The damage each bullet does")]
    public int WeaponDamage;
    [Tooltip("The range of each bullet")]
    public int WeaponRange;
    [Tooltip("Used for multiple firing modes or attaching a grenade launcher")]
    public WeaponFile AltWeapon;
    [Tooltip("How many bullets/ projectiles are fired each time the trigger is bullet? Primarly used for a shotgun/ burst-fire type of weapon.")]
    [Range(0, 10)] public int BulletsPerShot;
    [Tooltip("Works only with BulletPerShot. Defines the time between each shot bullet. Used for burst-firing types of weapons.")]
    [Range(0.0f, 1.0f)] public float BulletBetweenTime;
    [Tooltip("How does the weapon reload? For normal weapons, select magazine. For single loaded weapons (bolt-action rifles/shotguns with out magazines), select single. For melee or non-reloadables, select none")]
    public reloadtype WeaponReloadType;
    [Tooltip("Defines the time needed for the weapon to be considered reloaded.")]
    public float MinimumReloadTime = 1.5f;
    [Tooltip("Defines the time needed for the weapon to be drawn.")]
    public float DrawTime = 1f;
    [Tooltip("Defines the time needed for the weapon to be undrawn.")]
    public float HideTime = 0.5f;
    [Tooltip("Defines the max. amount of ammo a single magazine holds.")]
    public int MaxMagazineAmmo;
    [Tooltip("Defines the max. amount of ammo the weapons reserve holds.")]
    public int MaxReserveAmmo;
    [Header("Weapon Effects:")]
    public GameObject Muzzleflash;
    public GameObject Muzzleflash_Thirdperson;
    public GameObject MuzzleSmoke;
    public GameObject ShellEject;
    [Header("Weapon Sway:")]
    public float WeaponSway = 1f;
    public bool SwayYInverted;
    public bool SwayXInverted;
    public float SwaySmoothing;
    [Header("Weapon Recoil:")]
    public Vector3 RecoilRotation;
    public Vector3 RecoilKickback;
    [Header("Weapon Sounds:")]
    public AudioClip WeaponDrawSound;
    public AudioClip WeaponFirstDrawSound;
    public AudioClip WeaponFireSound;
    public AudioClip WeaponReloadFullSound;
    public AudioClip WeaponReloadTacticalSound;
    [Header("Location Damage Multipliers:")]
    public float Head = 2f;
    public float UpperTorso = 1f;
    public float LowerTorso = 1f;
    public float LeftUpperArm = 0.5f;
    public float LeftLowerArm = 0.5f;
    public float RightUpperArm = 0.5f;
    public float RightLowerArm = 0.5f;
    public float LeftUpperLeg = 0.5f;
    public float LeftLowerLeg = 0.5f;
    public float RightUpperLeg = 0.5f;
    public float RightLowerLeg = 0.5f;

    [Header("Weapon Animations:")]
    public AnimationClip Idle;
    public AnimationClip Sprinting;
    public AnimationClip Fire;
    public AnimationClip DryFire;
    public AnimationClip FirstDraw;
    public AnimationClip Draw;
    public AnimationClip Hide;
    public AnimationClip Melee;
    public AnimationClip ReloadFull;
    public AnimationClip ReloadTactical;
    public AnimationClip RechamberStart;
    public AnimationClip RechamberRound;
    public AnimationClip RechamberEnd;

    [Header("Weapon Hud:")]
    public Sprite HudIcon;
    public Sprite KillIcon;
    public Sprite AmmoIcon;

    [Header("Weapon Models:")]
    public GameObject WeaponViewModel;
    public GameObject WeaponWorldModel;
}

[System.Serializable]
public enum weapontype
{
    bullet,
    grenade,
    rocket,
    melee
}

[System.Serializable]
public enum firingmode
{
    semi_automatic,
    automatic,
    burst,
    shotgun
}

[System.Serializable]
public enum reloadtype
{
    magazine,
    single,
    none
}

[System.Serializable]
public enum ADSMode
{
    standard,
    overlay,
    none
}

[System.Serializable]
public enum weaponholdtype
{
    Rifle,
    Pistol,
    Launcher
}