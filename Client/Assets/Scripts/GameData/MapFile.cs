using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "mapname", menuName = "ModernFPS/Map", order = 0)]
public class MapFile : ScriptableObject
{
    public string MapDisplayName;
    public string SceneLoadName;
    public Sprite Loadscreen;
}
