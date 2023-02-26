using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    public Player Player;

    [Header("Viewmodel Handlers")]
    public Dictionary<string, ViewmodelInstance> WeaponInventory = new Dictionary<string, ViewmodelInstance>();
    public ViewmodelInstance VisibleWeapon;
    public Transform ViewmodelParent;
    public Transform ViewmodelSwayHolder;
    public Transform ViewModelADSHolder;
    public Transform ViewModelMovementHolder;
    private WeaponFile CurrentWeapon;
    public AudioSource ViewModelEffectsAudiosource;

    [Header("Viewmodel Idle Sway Variables")]
    public float IdleSwayAmountA = 1;
    public float IdleSwayAmountB = 2;
    public float IdleSwayScale = 600;
    public float IdleSwayLerpSpeed = 14;

    [Header("Viewmodel Movement Sway Variables")]
    public float RotationAmount = 4f;
    public float MaxRotationAmount = 5f;
    public float RotationSmoothAmount = 12f;

    [Space]
    public bool RotationX = true;
    public bool RotationY = true;
    public bool RotationZ = true;

    [Header("Viewmodel Movement Variables")]
    public float WalkMovementAmountA = 1;
    public float WalkMovementAmountB = 2;
    public float WalkMovementScale = 600;
    public float WalkMovementLerpSpeed = 14;

    [Header("Weapon handling Variables")]
    public Dictionary<string, WeaponInfo> WeaponStates = new Dictionary<string, WeaponInfo>();
    public List<string> InventoryOrder = new List<string>();
    private int CurrentIndex = 0;
    private bool SwitchingWeapons = false;
    private string TempWeapon;
    public bool ReleasedAttackbutton = true;

    private Quaternion InitialRotation;

    float IdleSwayTime;
    public float WalkTime;
    Vector3 WeaponSwayPosition;
    Vector3 WeaponWalkPosition;

    public void Start()
    {
        if (!Player)
        {
            Player = this.transform.root.GetComponent<Player>();
        }
        InitialRotation = ViewmodelSwayHolder.localRotation;
    }

    public void Update()
    {
        if (Player.isLocalplayer)
        {
            CalculateViewmodelIdleSway();
            CalculateViewmodelMovementSway();
            CalculateWalkAnimations();
        }
    }

    public void RegisterPlayerWeapon(string WeaponName)
    {
        WeaponFile spawnWeapon = ResourceManager.instance.loadedweapons[WeaponName];
        if (!WeaponInventory.ContainsKey(WeaponName))
        {
            WeaponInfo newWeapon = new WeaponInfo
            {
                MagazineAmmo = spawnWeapon.MaxMagazineAmmo,
                ReserveAmmo = spawnWeapon.MaxReserveAmmo,
                firstDraw = true,
                FireRate = 1f / (spawnWeapon.WeaponFireRate / 60f) //Calculate the fire rate per second and store it for performance.
            };

            WeaponStates.Add(WeaponName, newWeapon);
            InventoryOrder.Add(WeaponName);

            if (!ResourceManager.instance.loadedweapons.ContainsKey(WeaponName))
            {
                Debug.LogError($"Player tried to equip which is not loaded as resource for weapon with name {WeaponName}");
                return;
            }
        }
    }

    #region Viewmodel Handling
    public void SpawnViewModel(string WeaponName)
    {
        WeaponFile spawnWeapon = ResourceManager.instance.loadedweapons[WeaponName];
        if (!WeaponInventory.ContainsKey(WeaponName))
        {
            GameObject spawnedweapon = Instantiate(spawnWeapon.WeaponViewModel, ViewmodelParent);
            spawnedweapon.name = spawnWeapon.WeaponViewModel.name;
            ViewmodelInstance weaponinstance = spawnedweapon.GetComponent<ViewmodelInstance>();
            weaponinstance.MuzzleFlashEffect = Instantiate(spawnWeapon.Muzzleflash, weaponinstance.GetMuzzleTransform()).GetComponent<ParticleSystem>();
            weaponinstance.MuzzleSmokeEffect = Instantiate(spawnWeapon.MuzzleSmoke, weaponinstance.GetMuzzleTransform()).GetComponent<ParticleSystem>();
            weaponinstance.ShellEjectEffect = Instantiate(spawnWeapon.ShellEject, weaponinstance.GetShellEjectTransform()).GetComponent<ParticleSystem>();
            WeaponInventory.Add(WeaponName, weaponinstance);
            weaponinstance.WeaponAnimation.AddClip(spawnWeapon.Idle, "WeaponIdle");
            weaponinstance.WeaponAnimation.AddClip(spawnWeapon.FirstDraw, "WeaponFirstDraw");
            weaponinstance.WeaponAnimation.AddClip(spawnWeapon.Draw, "WeaponDraw");
            weaponinstance.WeaponAnimation.AddClip(spawnWeapon.Hide, "WeaponHide");
            weaponinstance.WeaponAnimation.AddClip(spawnWeapon.Sprinting, "WeaponSprint");
            weaponinstance.WeaponAnimation.AddClip(spawnWeapon.Fire, "WeaponFire");
            weaponinstance.WeaponAnimation.AddClip(spawnWeapon.ReloadFull, "WeaponReloadFull");
            weaponinstance.WeaponAnimation.AddClip(spawnWeapon.ReloadTactical, "WeaponReloadTactical");
            weaponinstance.HideWeaponModel();
        }
    }

    public void OnWeaponDraw(string WeaponName)
    {
        Player.CurrentWeapon = WeaponName;
        CurrentWeapon = ResourceManager.instance.loadedweapons[WeaponName];
    }

    public void ViewModelDraw(string WeaponName, bool FirstDraw)
    {
        if (VisibleWeapon == WeaponInventory[WeaponName])
        {
            VisibleWeapon.WeaponAnimation.CrossFade("WeaponIdle", 0.25f, PlayMode.StopAll);
            return;
        }
        if (Player.CurrentWeapon != "" && Player.CurrentWeapon != null)
        {
            WeaponInventory[Player.CurrentWeapon].HideWeaponModel(); //Hide our old weapon
        }
        WeaponInventory[WeaponName].ShowWeaponModel();
        VisibleWeapon = WeaponInventory[WeaponName];
        VisibleWeapon.transform.localPosition = CurrentWeapon.WeaponPosition;
        if (FirstDraw)
        {
            //SoundManager.instance?.PlayInterruptableSound(CurrentWeapon.WeaponFirstDrawSound, ViewModelEffectsAudiosource);
            //WeaponAnimation.AddClip(currentWeapon.FirstDraw, "WeaponFirstDraw");
            //WeaponAnimation.Play("WeaponFirstDraw");
            VisibleWeapon.WeaponAnimation.CrossFadeQueued("WeaponFirstDraw", 0.1f, QueueMode.PlayNow);
        }
        else
        {
            //SoundManager.instance?.PlayInterruptableSound(CurrentWeapon.WeaponDrawSound, ViewModelEffectsAudiosource);
            //WeaponAnimation.AddClip(currentWeapon.Draw, "WeaponDraw");
            //WeaponAnimation.Play(currentWeapon.Draw.name);
            VisibleWeapon.WeaponAnimation.CrossFadeQueued("WeaponDraw", 0.1f, QueueMode.PlayNow);
        }
    }

    public void CalculateViewmodelIdleSway()
    {
        var targetPosition = LissajousCurve(IdleSwayTime, IdleSwayAmountA, IdleSwayAmountB) / IdleSwayScale;

        WeaponSwayPosition = Vector3.Lerp(WeaponSwayPosition, targetPosition, IdleSwayLerpSpeed * Time.deltaTime);
        IdleSwayTime += Time.deltaTime;

        ViewmodelSwayHolder.localPosition = WeaponSwayPosition;
    }

    private Vector3 LissajousCurve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }

    public void CalculateViewmodelMovementSway()
    {
        float TiltX = 0f;
        float TiltY = 0f;
        if (!GameManager.Singleton.IsPaused)
        {
            TiltX = Mathf.Clamp(Player.clientinputs.mouseY * RotationAmount, -MaxRotationAmount, MaxRotationAmount);
            TiltY = Mathf.Clamp(Player.clientinputs.mouseX * RotationAmount, -MaxRotationAmount, MaxRotationAmount);
        } 

        Quaternion FinalRotation = Quaternion.Euler(new Vector3(
            RotationX ? -TiltX : 0f,
            RotationY ? TiltY : 0f,
            RotationZ ? TiltY : 0f
        ));

        ViewmodelSwayHolder.localRotation = Quaternion.Slerp(ViewmodelSwayHolder.localRotation, FinalRotation * InitialRotation, Time.deltaTime * RotationSmoothAmount);
    }

    public void CalculateWalkAnimations()
    {
        if(GameManager.Singleton.IsPaused)
        {
            ViewModelMovementHolder.localPosition = Vector3.Lerp(ViewModelMovementHolder.localPosition, Vector3.zero, 10f * Time.deltaTime);
            return;
        }

        if(Mathf.Abs(Player.clientinputs.forwardMove) > 0.1f || Mathf.Abs(Player.clientinputs.sideMove) > 0.1f)
        {
            var targetPosition = LissajousCurve(WalkTime, WalkMovementAmountA, WalkMovementAmountB) / WalkMovementScale;

            WeaponWalkPosition = Vector3.Lerp(WeaponWalkPosition, targetPosition, WalkMovementLerpSpeed * Time.deltaTime);
            WalkTime += Time.deltaTime;

            ViewModelMovementHolder.localPosition = WeaponWalkPosition;
        } else
        {
            ViewModelMovementHolder.localPosition = Vector3.Lerp(ViewModelMovementHolder.localPosition, Vector3.zero, 10f * Time.deltaTime);
        }
    }

    public void ToggleViewModelSprint(bool sprintState)
    {
        if (sprintState)
        {
            /*
            if (WeaponInventory[Player.CurrentWeapon].WeaponSoundeffect.isPlaying)
            {
                WeaponInventory[Player.CurrentWeapon].WeaponSoundeffect.Stop();
            }
            */

            VisibleWeapon.WeaponAnimation.CrossFade("WeaponSprint", 0.25f, PlayMode.StopAll);
        }
        else
        {
            VisibleWeapon.WeaponAnimation.CrossFade("WeaponIdle", 0.25f, PlayMode.StopAll);
        }
    }

    public void ViewmodelFireAnimation()
    {
        VisibleWeapon.WeaponAnimation.PlayQueued("WeaponFire", QueueMode.PlayNow);
        SoundManager.instance.PlaySoundWithRandomPitch(CurrentWeapon.WeaponFireSound, WeaponInventory[Player.CurrentWeapon].WeaponBarrel, 0.95f, 1.05f);

        WeaponInventory[Player.CurrentWeapon].MuzzleFlashEffect.Play();
        WeaponInventory[Player.CurrentWeapon].MuzzleSmokeEffect.Play();
        WeaponInventory[Player.CurrentWeapon].ShellEjectEffect.Play();
    }

    public void ToggleViewModelAim(bool AimToggle)
    {
        if(AimToggle)
        {
            ViewModelADSHolder.localPosition = Vector3.Lerp(ViewModelADSHolder.localPosition, CurrentWeapon.WeaponADSPosition, Time.deltaTime * 10f);
        }
        else
        {
            ViewModelADSHolder.localPosition = Vector3.Lerp(ViewModelADSHolder.localPosition, Vector3.zero, Time.deltaTime * 10f);
        }
    }

    public void ViewModelReload()
    {
        VisibleWeapon.WeaponAnimation.PlayQueued("WeaponReloadFull", QueueMode.PlayNow);
        SoundManager.instance.PlayInterruptableSound(CurrentWeapon.WeaponReloadFullSound, WeaponInventory[Player.CurrentWeapon].WeaponSoundeffect);
    }
    #endregion

    public void DetermineWeaponState(PlayerCMD inputState)
    {
        if (Player.CurrentWeapon == "" || Player.CurrentWeapon == null)
        {
            return;
        }

        if (!inputState.attack && ReleasedAttackbutton == false)
        {
            WeaponStates[Player.CurrentWeapon].Canfire = true;
            ReleasedAttackbutton = true;
        }

        if (Player.clientstate.Sprinting && Player.clientstate.Grounded)
        {
            CancelInvoke();
            OnReloadFinished();
            if (Player.isLocalplayer)
                ToggleViewModelSprint(true);
        }
        else 
        {
            if (Player.isLocalplayer && !Player.clientstate.Reloading)
                ToggleViewModelSprint(false);
        }

        if (inputState.attack)
        {
            CancelInvoke();
            OnReloadFinished();
            if (!Player.clientstate.Sprinting && WeaponStates[Player.CurrentWeapon].Canfire)
            {
                if (Time.time - WeaponStates[Player.CurrentWeapon].FireCounter > WeaponStates[Player.CurrentWeapon].FireRate)
                {
                    if (!Player.clientstate.Firing)
                    {
                        Player.clientstate.Firing = true;
                    }
                    WeaponStates[Player.CurrentWeapon].FireCounter = Time.time;

                    if (Player.isLocalplayer)
                    {
                        ViewmodelFireAnimation();
                    }
                }
            }
            if (ReleasedAttackbutton == false && CurrentWeapon.FiringMode != firingmode.automatic)
            {
                WeaponStates[Player.CurrentWeapon].Canfire = false;
                Player.clientstate.Firing = false;
            }
            ReleasedAttackbutton = false;
        }
        else
        {
            if (Player.clientstate.Firing)
            {
                Player.clientstate.Firing = false;
            }
        }

        if (inputState.aim && !Player.clientstate.Sprinting && !Player.clientstate.Reloading)
        {
            Player.clientstate.Aim = true;
            if (Player.isLocalplayer)
            {
                ToggleViewModelAim(true);
            }
        }
        else
        {
            Player.clientstate.Aim = false;
            if (Player.isLocalplayer)
            {
                ToggleViewModelAim(false);
            }
        }

        if (inputState.reload && !Player.clientstate.Reloading && !Player.clientstate.Sprinting)
        {
            Player.clientstate.Reloading = true;
            if (Player.isLocalplayer)
                ViewModelReload();
            Invoke("OnReloadFinished", CurrentWeapon.ReloadFull.length);
        }
    }

    public void OnReloadFinished()
    {
        Player.clientstate.Reloading = false;
        if (WeaponInventory[Player.CurrentWeapon].WeaponSoundeffect.isPlaying)
        {
            WeaponInventory[Player.CurrentWeapon].WeaponSoundeffect.Stop();
        }
    }
}

public class WeaponInfo
{
    public int MagazineAmmo;
    public int ReserveAmmo;
    public bool firstDraw;
    public float FireRate;
    public bool Canfire = false;
    public float FireCounter = 0f;
    public float ReloadCounter = 0f;
}
