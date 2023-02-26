using Riptide;
using Riptide.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ServerToClientId : ushort
{
    sendServerInfo = 1,
    spawnPlayer,
    playerMovement,
    serverCSPState,
    serverInterpolationState,
    serverSendtick,
}
public enum ClientToServerId : ushort
{
    playerName = 1,
    requestSpawn,
    playerInput,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
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
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;
    [SerializeField] private GameObject playerPrefab;

    public GameObject PlayerPrefab => playerPrefab;

    public Server Server { get; private set; }

    public int Tick;

    private void Awake()
    {
        Singleton = this;
    }
    public void StartServer()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 50;

        string GameModeToLoad = Util_Functions.GetArg("-Gamemode");
        if (GameModeToLoad == null)
        {
            GameModeToLoad = "TDM";
        } else
        {
            Console.WriteLine($"Gamemode parameter to load: {GameModeToLoad}");
        }

        if(!GameModeManager.LoadGameMode(GameModeToLoad))    //Attempt to load the gamemode, if the mode cant be found, aborting starting the server.
        {
            return;
        }

        #if UNITY_EDITOR
                RiptideLogger.Initialize(Debug.Log, false);
        #else
                Console.Title = "Server";
                Console.Clear();
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                RiptideLogger.Initialize(Debug.Log, true);
        #endif

        Server = new Server();
        Server.ClientConnected += NewPlayerConnected;
        Server.ClientDisconnected += PlayerLeft;
        SceneManager.sceneLoaded += OnServerMapLoaded;

        Server.Start(port, maxClientCount);
        LoadMap("Demo");
    }

    private void FixedUpdate()
    {
		Server.Update();
		if (GameManager.Singleton.GameState != GameState.offline || GameManager.Singleton.GameState != GameState.maploading)
        {
            Tick++;
        }
    }

    private void OnApplicationQuit()
    {
        Server.Stop();

        Server.ClientConnected -= NewPlayerConnected;
        Server.ClientDisconnected -= PlayerLeft;
        SceneManager.sceneLoaded -= OnServerMapLoaded;
    }

    private void NewPlayerConnected(object sender, ServerConnectedEventArgs e)
    {
		Debug.Log("Connection Accepted from" + e.Client.Id);
    }
    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
		if (Player.List.TryGetValue(e.Client.Id, out Player player))
			Destroy(player.gameObject);
    }

    private void LoadMap(string maploadname)
    {
        if(!GameManager.Singleton.mapsbyname.ContainsKey(maploadname))
        {
            Debug.Log($"Could not find map: {maploadname} in server's maplist! Stopping server...");
            Server.Stop();
            return;
        }
        MapFile maptoload = GameManager.Singleton.mapsbyname[maploadname];
        GameManager.Singleton.CurrentMap = maptoload;
        SceneManager.LoadScene(GameManager.Singleton.CurrentMap.SceneLoadName);
    }

    public void OnServerMapLoaded(Scene loadscene, LoadSceneMode loadSceneMode)
    {
        if (loadscene.name.Equals("Main"))
            return;

        RiptideLogger.Log(Riptide.Utils.LogType.Info, $"Finished loaded map: {GameManager.Singleton.CurrentMap.MapDisplayName}.");
        GameManager.Singleton.ServerGatherSpawnpoints();
        GameManager.Singleton.SetGameState(GameState.active); //Will be changed to prematch when gamemodes are added again...
    }

    /// <summary>Send the serverinfo (map, gametype and game settings) to the specified client.</summary>
    /// <param name="toClient">The client to send the message to.</param>
    public void SendServerInfo(ushort toClient)
    {
        Server.Send(GetServerInfo(toClient,Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientId.sendServerInfo)), toClient);
    }

    private Message GetServerInfo(ushort Id, Message message)
    {
        message.Add(Id);
        message.Add(GameManager.Singleton.CurrentMap.MapLoadName);
        message.Add(GameManager.Singleton.CurrentGameMode.GameModeLoadName);
        return message;
    }

    public void SendExistingPlayersForClient(ushort toClient)
    {
        foreach (Player player in Player.List.Values)
        {
            if (player.Id != toClient)
                player.SendSpawn(toClient);
        }
    }
}