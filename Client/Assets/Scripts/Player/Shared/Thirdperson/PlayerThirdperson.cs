using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThirdperson : MonoBehaviour
{
    [Header("Third person Model Settings")]
    public ModelVisibility ThirdpersonVisibility;
    public Renderer[] PlayerModelRenderers;

    public Animator playerAnimator;
    public Player player;

    public BodyController BodyIKSolver;
    [SerializeField] private Vector2 CurrentPlayerMovement;
    [SerializeField] private Renderer[] ModelRenderers;
    public Transform WeaponHolder;
    public Dictionary<string, WorldmodelInstance> weaponinventory = new Dictionary<string, WorldmodelInstance>();
    public WorldmodelInstance CurrentThirdPersonWeapon;

    public Transform Aimdirection;

    public void OnValidate()
    {
        SetVisibility(ThirdpersonVisibility);
        player = transform.GetComponent<Player>();
    }

    public void SetVisibility(ModelVisibility visibility)
    {
        if (PlayerModelRenderers.Length == 0)
            return;

        switch (visibility)
        {
            case ModelVisibility.Visible:
                {
                    foreach (Renderer renderer in PlayerModelRenderers)
                    {
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    }
                    break;
                }
            case ModelVisibility.OnlyShadows:
                {
                    foreach (Renderer renderer in PlayerModelRenderers)
                    {
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    }
                    break;
                }
            default:
                {
                    foreach (Renderer renderer in PlayerModelRenderers)
                    {
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    }
                    break;
                }
        }
    }

    public void UpdatePlayerModelState(PlayerState newclientstate)
    {
        //Aimdirection.rotation = newclientstate.ViewAngles;
        Aimdirection.rotation = Quaternion.Lerp(Aimdirection.rotation, newclientstate.ViewAngles, Time.deltaTime * 10);
        playerAnimator?.SetFloat("Horizontal", newclientstate.PlayerDirection.x, 1f, Time.deltaTime * 10f);
        playerAnimator?.SetFloat("Vertical", newclientstate.PlayerDirection.y, 1f, Time.deltaTime * 10f);
        if (playerAnimator?.GetBool("Sprinting") != newclientstate.Sprinting)
        {
            playerAnimator?.SetBool("Sprinting", newclientstate.Sprinting);
        }
        if (playerAnimator?.GetBool("isGrounded") != newclientstate.Grounded)
        {
            playerAnimator?.SetBool("isGrounded", newclientstate.Grounded);
        }
        if (playerAnimator?.GetInteger("PlayerStance") != newclientstate.PlayerStance)
        {
            playerAnimator?.SetInteger("PlayerStance", newclientstate.PlayerStance);
        }
        if (newclientstate.Firing)
        {
            if(!player.isLocalplayer)
                PlayerFired();
        }
    }

    public void PlayerFired()
    {
        CurrentThirdPersonWeapon?.MuzzleFlashEffect.Play();
    }

    public void SetThirdPersonWeapon(string WeaponName, string OldWeaponName)
    {
        if (!ResourceManager.instance.loadedweapons.ContainsKey(WeaponName))
        {
            Debug.LogError($"Player tried to equip which is not loaded as resource for weapon with name {WeaponName}");
            return;
        }
        BodyIKSolver.gunAimpoint = null;
        if (OldWeaponName != "" && OldWeaponName != null)
        {
            CurrentThirdPersonWeapon = null;
            //ThirdPersonAnimator.SetTrigger("WeaponDraw");
            if (weaponinventory.ContainsKey(OldWeaponName))
            {
                weaponinventory[OldWeaponName].HideWeaponModel();
            }
            else
            {
                WeaponFile previousWeapon = ResourceManager.instance.loadedweapons[OldWeaponName];
                GameObject worldWeapon = Instantiate(previousWeapon.WeaponWorldModel, WeaponHolder);
                WorldmodelInstance weaponworldinstance = worldWeapon.GetComponent<WorldmodelInstance>();
                weaponworldinstance.MuzzleFlashEffect = Instantiate(previousWeapon.Muzzleflash_Thirdperson, weaponworldinstance.GetMuzzleTransform()).GetComponent<ParticleSystem>();
                //weaponworldinstance.ShellEjectEffect = Instantiate(previousWeapon.ShellEject, weaponworldinstance.GetShellEjectTransform()).GetComponent<ParticleSystem>();
                weaponinventory.Add(OldWeaponName, weaponworldinstance);
                weaponworldinstance.HideWeaponModel();
            }
        }
        WeaponFile CurrentWeapon = ResourceManager.instance.loadedweapons[WeaponName];
        playerAnimator.SetInteger("WeaponType", (int)CurrentWeapon.WeaponHoldType);
        if (weaponinventory.ContainsKey(WeaponName))
        {
            weaponinventory[WeaponName].ShowWeaponModel();
            BodyIKSolver.gunAimpoint = weaponinventory[WeaponName].GunAimpoint;
            CurrentThirdPersonWeapon = weaponinventory[WeaponName];
        }
        else
        {
            GameObject worldWeapon = Instantiate(CurrentWeapon.WeaponWorldModel, WeaponHolder);
            WorldmodelInstance weaponworldinstance = worldWeapon.GetComponent<WorldmodelInstance>();
            weaponworldinstance.MuzzleFlashEffect = Instantiate(CurrentWeapon.Muzzleflash_Thirdperson, weaponworldinstance.GetMuzzleTransform()).GetComponent<ParticleSystem>();
            //weaponworldinstance.ShellEjectEffect = Instantiate(CurrentWeapon.ShellEject, weaponworldinstance.GetShellEjectTransform()).GetComponent<ParticleSystem>();
            weaponinventory.Add(WeaponName, weaponworldinstance);
            if (player.isLocalplayer)
            {
                weaponworldinstance.SetShadowRendereringMode(2);
            }
            BodyIKSolver.gunAimpoint = weaponworldinstance.GunAimpoint;
            CurrentThirdPersonWeapon = weaponinventory[WeaponName];
            //worldWeapon.SetActive(false);
        }
        //ThirdPersonAnimator.SetBool("WeaponDraw", false);
    }
}

public enum ModelVisibility
{
    Visible,
    OnlyShadows
}