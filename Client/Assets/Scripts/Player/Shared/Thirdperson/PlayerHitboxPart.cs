using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitboxPart : MonoBehaviour
{
    public BodyPart Location;
    public Collider LocationCollider;
    public Rigidbody LocationRigidbody;

    public void ToggleHitbox(bool toggle)
    {
        LocationCollider.enabled = toggle;
        LocationRigidbody.isKinematic = !toggle;
    }
}

public enum BodyPart
{
    head,
    chest,
    abdoment,
    upperleftarm,
    lowerleftarm,
    upperleftleg,
    lowerleftleg,
    upperrightarm,
    lowerrightarm,
    upperrightleg,
    lowerrightleg,
}
