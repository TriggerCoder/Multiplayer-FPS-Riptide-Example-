using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



public class GameModeManager : MonoBehaviour
{
    private static GameModeManager _singleton;
    public static GameModeManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
            {
                _singleton = value;
            }
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    public delegate void GameModeLoaded();
    public static event GameModeLoaded OnGameModeLoadedEvent;

    public delegate void GameModeUnloaded();
    public static event GameModeUnloaded OnGameModeUnloadedEvent;

    public GameMode[] AllGameModes;

    public static GameMode CurrentGameMode()
    {
        if (Singleton == null)
        {
            return null;
        }
        return GameManager.Singleton.CurrentGameMode;
    }

    private void Awake()
    {
        Singleton = this;
    }

    private bool LoadGameMode_Internal(string GamemodeName)
    {
        for (int i = 0; i < AllGameModes.Length; i++)
        {
            if (AllGameModes[i].GameModeLoadName == GamemodeName)
            {
                AllGameModes[i].enabled = true;
                Debug.Log($"Loading GameMode: {AllGameModes[i].GameModeLoadName}");
                Console.WriteLine($"Loading GameMode: {AllGameModes[i].GameModeLoadName}");
                GameManager.Singleton.CurrentGameMode = AllGameModes[i];
            }
            else
            {
                AllGameModes[i].enabled = false;
            }
        }
        if (GameManager.Singleton.CurrentGameMode == null)
        {
            Debug.Log($"Failed loading gamemode: {GamemodeName}. Aborting this game...");
            Console.WriteLine($"Failed loading gamemode: {GamemodeName}. Aborting this game...");
            return false;
        } else
        {
            OnGameModeLoadedEvent?.Invoke();
            return true;
        }
    }

    private void UnloadCurrentGameMode_Internal()
    {
        for (int i = 0; i < AllGameModes.Length; i++)
        {
            if(AllGameModes[i] == GameManager.Singleton.CurrentGameMode)
            {
                AllGameModes[i].enabled = false;
                Debug.Log($"Unloading GameMode: {AllGameModes[i].GameModeLoadName}");
                Console.WriteLine($"Unloading GameMode: {AllGameModes[i].GameModeLoadName}");
                if (OnGameModeUnloadedEvent != null)
                    OnGameModeUnloadedEvent.Invoke();
            }
        }
    }

    public static bool LoadGameMode(string GamemodeName)
    {
        return Singleton.LoadGameMode_Internal(GamemodeName);
    }

    public static void UnLoadCurrentGameMode()
    {
        Singleton.UnloadCurrentGameMode_Internal();
    }
}
