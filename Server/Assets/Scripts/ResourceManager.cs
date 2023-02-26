using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;
    //Weapons
    public List<WeaponFile> WeaponFileList = new List<WeaponFile>();    //Add weaponfile for the game to load in here.
    public Dictionary<string, WeaponFile> loadedweapons = new Dictionary<string, WeaponFile>(); //The weaponfiles accesible by loadname.
    //Impacts
    public List<Impact> ImpactsList = new List<Impact>();    //Add Impacts for the game to load in here.
    public Dictionary<string, Impact> loadedimpacts = new Dictionary<string, Impact>(); //The Impacts accesible by loadname.

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            DestroyImmediate(this);
        }
    }

    public void LoadGameResources()
    {
        if(WeaponFileList.Count == 0)
        {
            Debug.LogError($"Client has no available weapons to load into cache");
            return;
        }
        foreach (WeaponFile weapon in WeaponFileList)
        {
            if(weapon != null && weapon.WeaponLoadName != null)
            {
                Debug.Log($"Loaded weapon resource for weapon: {weapon.WeaponLoadName}");
                loadedweapons.Add(weapon.WeaponLoadName, weapon);
            } else
            {
                Debug.LogError($"Could not load weaponfile for weapon: {weapon.WeaponName}");
            }
        }

        foreach (Impact impact in ImpactsList)
        {
            if (impact != null)
            {
                Debug.Log($"Loaded impact resource for impact: {impact.name}");
                loadedimpacts.Add(impact.name, impact);
            }
            else
            {
                Debug.LogError($"Could not load impactfile for impact: {impact.name}");
            }
        }
    }

    public void VerifyServerLoadedWeaponWithClient(string[] serverWeapons)
    {
        int serverWeaponsLength = serverWeapons.Length;
        foreach (string serverweapon in serverWeapons)
        {
            if (loadedweapons.ContainsKey(serverweapon))
            {
                continue;
            }
            else
            {
                //GameManager.instance.DisconnectFromServerError($"Client weaponfiles doesn't match the server!");
                //NetworkManager.Singleton.
            }
        }
    }
}
