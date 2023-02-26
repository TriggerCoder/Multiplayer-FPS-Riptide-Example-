using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Riptide;

/* Base class for creating a GameMode. Functions and variables are available to all gamemodes. */
public class GameMode : MonoBehaviour
{
    public string GameModeLoadName;
    public string GameModeDisplayName;

    public GameObject GameModeSpawnMenuPrefab;
    public GameObject GameModePauseMenuPrefab;

    public GameModePauseMenu PauseMenu;

    //Leave this function blank, this is in order to be able to disable the script when not in use.
    private void Start(){}

    public void Init()
    {
        Debug.Log("Client Gamemode Init");
        if (GameModeSpawnMenuPrefab)
        {
            GameObject.Instantiate(GameModeSpawnMenuPrefab, UIManager.Singleton.Transform);
        }
        if (GameModePauseMenuPrefab)
        {
            PauseMenu = GameObject.Instantiate(GameModePauseMenuPrefab, UIManager.Singleton.Transform).GetComponent<GameModePauseMenu>();
        }
        UI_Loadscreen.Close();
    }

    public void Unload()
    {
        Debug.Log("Client Gamemode Unload");
        Destroy(PauseMenu.gameObject);
    }

    /// <summary>Callback when a level is loading</summary>
    public void OnLevelLoading()
    {

    }

    /// <summary>Callback when a level has been loaded</summary>
    public void OnLevelLoaded()
    {

    }

    /// <summary>Returns the Display name of this gamemode</summary>
    public string GetGameModeDisplayName()
    {
        return this.GameModeDisplayName;
    }

    /// <summary>Call back when the Match has started</summary>
    public void OnMatchStart()
    {

    }

    /// <summary>Call back when the player has been spawned</summary>
    public void OnPlayerSpawn()
    {

    }

    /// <summary>Call back when the player has been respawned</summary>
    public void OnPlayerRespawn()
    {

    }

    /// <summary>Call back when the Match has ended</summary>
    public void OnMatchEnded()
    {

    }

    /// <summary>Call back after the Match has ended</summary>
    public void OnMatchSummary()
    {

    }

    /// <summary>Call back when a player has been killed</summary>
    public void OnPlayerKilledCallback()
    {

    }

    /// <summary>Call back when the currently local player has been killed</summary>
    public void OnPlayerDeathCallback()
    {

    }
}
