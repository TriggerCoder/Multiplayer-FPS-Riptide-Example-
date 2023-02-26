using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "impact", menuName = "ModernFPS/Impact", order = 1)]
public class Impact : ScriptableObject
{
    public GameObject HitParticlePrefab;
    public AudioClip[] ImpactSounds;

    public void SpawnHitParticle(Vector3 pos, Vector3 normal, Transform hit)
    {
        GameObject par = Instantiate(HitParticlePrefab, pos, Quaternion.LookRotation(normal));
        par.GetComponent<AudioSource>().PlayOneShot(ImpactSounds[0]);
        par.transform.SetParent(hit);
    }
}
