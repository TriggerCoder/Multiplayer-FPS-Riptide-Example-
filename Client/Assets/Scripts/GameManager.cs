using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                DontDestroyOnLoad(value.gameObject);
            }
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameManager)} instance already exists, destroying object!");
                DestroyImmediate(value.gameObject);
            }
        }
    }

    #region Events
    public delegate void PauseStateChanged(bool pauseState);
    public static event PauseStateChanged OnClientPauseStateChangedEvent;
    #endregion

    public GameState GameState = GameState.offline;

    public List<MapFile> maps = new List<MapFile>();
    public Dictionary<string, MapFile> mapsbyname = new Dictionary<string, MapFile>();
    public MapFile CurrentMap;
    public GameMode CurrentGameMode;

    public bool IsPaused = false;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        ResourceManager.instance.LoadGameResources();
        LoadMaps();
        SceneManager.sceneLoaded += OnClientMapLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameState != GameState.offline && GameState != GameState.maploading)
        {
            IsPaused = !IsPaused;
            SetPausedState(IsPaused);
        }
    }

    public void SetPausedState(bool pauseState)
    {
        OnClientPauseStateChangedEvent?.Invoke(pauseState);

        if (pauseState == true)
        {
            SetCursorLocked(false);
        } else
        {
            SetCursorLocked(true);
        }
    }

    public void SetCursorLocked(bool lockState)
    {
        Debug.Log($"Cursor lockstate set to {lockState}");
        if (lockState == true)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        } else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnApplicationQuit()
    {
        
    }

    public void InitMainMenu()
    {
        if (PlayerPrefs.GetString("UserName") == null || PlayerPrefs.GetString("UserName") == "")
        {
            UI_Background.Open();
            InputfieldDialog.Open("Enter a UserName:", "Username...", "", 15, 3, "Connect", (string UserNameText) =>
            {
                // Confirm
                UI_Blocker.Close();
                PlayerPrefs.SetString("UserName", UserNameText);
                UIManager.DisplayMainMenu();
            });
        }
        else
        {
            UIManager.DisplayMainMenu();
        }
    }

    void LoadMaps()
    {
        if (maps.Count == 0)
        {
            return;
        }
        foreach (MapFile map in maps)
        {
            mapsbyname.Add(map.name, map);
            Debug.Log($"Added map {map.name} to maplist!");
        }
    }

    public void SetGameState(GameState state)
    {
        GameState = state;
    }

    public void LoadMap(string maploadname)
    {
        if (!GameManager.Singleton.mapsbyname.ContainsKey(maploadname))
        {
            Debug.Log($"Could not find map: {maploadname} in client's maplist! Stopping client...");
            NetworkManager.Singleton.Client.Disconnect();
            return;
        }
        Debug.Log($"(CLIENT): Loading server map: {maploadname}");
        MapFile maptoload = GameManager.Singleton.mapsbyname[maploadname];
        GameManager.Singleton.CurrentMap = maptoload;
        GameManager.Singleton.SetGameState(GameState.maploading);
        SceneManager.LoadScene(GameManager.Singleton.CurrentMap.SceneLoadName);
    }

    public void OnClientMapLoaded(Scene loadscene, LoadSceneMode loadSceneMode)
    {
        if (loadscene.name.Equals("MainMenu"))
        {
            InitMainMenu();
            return;
        }
        UIManager.HideMainMenu();
        NetworkManager.Singleton.RequestSpawn();
    }

    public void PlayerOnDisconnected()
    {
        Debug.Log($"Disconnected from the server!");
        GameModeManager.UnLoadCurrentGameMode();
        GameState = GameState.offline;
        NetworkManager.Singleton.ResetTicks();
        IsPaused = false;
        SetPausedState(false);
        SetCursorLocked(false);
        SceneManager.LoadScene("MainMenu");
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
