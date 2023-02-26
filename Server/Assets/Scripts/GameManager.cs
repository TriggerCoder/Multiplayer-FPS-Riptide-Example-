using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

public class GameManager : MonoBehaviour
{
    private static GameManager _singleton;
    public static GameManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
            {
                _singleton = value;
                DontDestroyOnLoad(value);
            }
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    public GameState GameState = GameState.offline;

    public List<MapFile> maps = new List<MapFile>();
    public Dictionary<string, MapFile> mapsbyname = new Dictionary<string, MapFile>();
    public MapFile CurrentMap;
    public GameMode CurrentGameMode;
    public List<Spawnpoint> MapSpawnpoints;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        ResourceManager.instance.LoadGameResources();
        LoadMaps();
    }

    void LoadMaps()
    {
        if(maps.Count == 0)
        {
            return;
        }
        foreach (MapFile map in maps)
        {
            mapsbyname.Add(map.name, map);
            Debug.Log($"Added map {map.name} to maplist!");
        }
        NetworkManager.Singleton.StartServer();
    }

    public void ServerGatherSpawnpoints()
    {
        Spawnpoint[] spawnpoints = FindObjectsOfType<Spawnpoint>();
        if(spawnpoints.Length == 0)
        {
            RiptideLogger.Log(Riptide.Utils.LogType.Warning,$"Could not find any spawnpoints in map: {CurrentMap.MapDisplayName}!");
            return;
        }
        foreach (Spawnpoint spawnpoint in spawnpoints)
        {
            MapSpawnpoints.Add(spawnpoint);
        }
    }

    public Spawnpoint ReturnRandomSpawnpoint()
    {
        if (MapSpawnpoints.Count == 0)
            return null;
        return MapSpawnpoints[Random.Range(0, MapSpawnpoints.Count - 1)];
    }

    public void SetGameState(GameState state)
    {
        GameState = state;
    }
}

public enum GameState
{
    offline,
    maploading,
    pregame,
    preround,
    active,
    roundended,
    mapended
}