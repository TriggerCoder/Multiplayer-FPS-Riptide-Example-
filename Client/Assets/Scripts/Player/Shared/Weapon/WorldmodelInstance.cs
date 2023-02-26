using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldmodelInstance : MonoBehaviour
{
    public Renderer[] MeshRenderers;
    public Transform GunAimpoint;
    [SerializeField] private Transform MuzzlePos;
    [SerializeField] private Transform ShellEjectPos;
    public ParticleSystem MuzzleFlashEffect;
    public ParticleSystem ShellEjectEffect;
    public void HideWeaponModel()
    {
        this.gameObject.SetActive(false);
    }

    public void SetShadowRendereringMode(int Mode)
    {
        /*
        if(Mode.Equals(1))
        {
            foreach (Renderer weaponrenderer in MeshRenderers)
            {
                weaponrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }
        if (Mode.Equals(2))
        {
            foreach (Renderer weaponrenderer in MeshRenderers)
            {
                weaponrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
        */
        
        switch (Mode)
        {
            case 1:
                foreach (Renderer weaponrenderer in MeshRenderers)
                {
                    weaponrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
                break;
            case 2:
                foreach (Renderer weaponrenderer in MeshRenderers)
                {
                    weaponrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
                break;
            case 3:
                foreach (Renderer weaponrenderer in MeshRenderers)
                {
                    weaponrenderer.enabled = false;
                }
                break;
        }
    }

    public void ShowWeaponModel()
    {
        this.gameObject.SetActive(true);
    }

    public Transform GetMuzzleTransform()
    {
        if (!MuzzlePos)
            return null;
        return MuzzlePos;
    }

    public Transform GetShellEjectTransform()
    {
        if (!ShellEjectPos)
            return null;
        return ShellEjectPos;
    }
}
