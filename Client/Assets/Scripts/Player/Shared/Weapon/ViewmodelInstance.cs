using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewmodelInstance : MonoBehaviour
{
    public AudioSource WeaponSoundeffect;
    public AudioSource WeaponBarrel;
    public Animation WeaponAnimation;
    [SerializeField] private Transform MuzzlePos;
    [SerializeField] private Transform ShellEjectPos;
    [HideInInspector] public ParticleSystem MuzzleSmokeEffect;
    [HideInInspector] public ParticleSystem MuzzleFlashEffect;
    [HideInInspector] public ParticleSystem ShellEjectEffect;

    public void HideWeaponModel()
    {
        WeaponAnimation.Play();
        this.gameObject.SetActive(false);
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
