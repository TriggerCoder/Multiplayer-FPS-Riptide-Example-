using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewmodelTest : MonoBehaviour
{
    public Animation Viewmodelanimation;

    [Header("Animations")]
    public AnimationClip Idle;
    public AnimationClip Sprint;
    public AnimationClip Show;
    public AnimationClip Hide;
    public AnimationClip ReloadFull;
    public AnimationClip ReloadTactical;
    public AnimationClip Fire;

    //public AudioSource source;
    public AudioClip reloadClip;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Viewmodelanimation.CrossFade(Idle.name, 0.25f);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Viewmodelanimation.CrossFade(Sprint.name, 0.25f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1) && !Viewmodelanimation.IsPlaying(Hide.name))
        {
            Viewmodelanimation.CrossFade(Hide.name, 0.25f);
        } else if(Input.GetKeyDown(KeyCode.Alpha1) && Viewmodelanimation.IsPlaying(Hide.name))
        {
            Viewmodelanimation.CrossFade(Idle.name, 0.25f);
        } 
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Viewmodelanimation.CrossFade(Fire.name, 0.1f);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Viewmodelanimation.CrossFade(ReloadFull.name, 0.25f);
            //SoundManager.instance.PlayInterruptableSound(reloadClip, source);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Viewmodelanimation.CrossFade(ReloadTactical.name, 0.25f);
        }
    }
}
