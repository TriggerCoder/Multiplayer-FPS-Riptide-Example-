using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorShifter : MonoBehaviour
{
    public float speed;
    public Color StartColor;
    public Color EndColor;
    public Image Material;
    Color lerpedColor = Color.white;

    // Update is called once per frame
    void Update()
    {
        lerpedColor = Color.Lerp(StartColor, EndColor, Mathf.PingPong(Time.time / speed, 1f));
        Material.color = lerpedColor;
    }
}
