using Riptide;
using Riptide.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                DestroyImmediate(value);
            }
        }
    }

    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;

    public Client Client { get; private set; }

    [Header("Ingame Tick Synchronization")]
    public int clientPredictedTick;
    public int serverEstimatedTick;
    public int DelayTick;
    public bool receivedServerStartTick = false;


    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, false);

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;
	}

    private void FixedUpdate()
    {
		Client.Update();
		if (receivedServerStartTick)
        {
            serverEstimatedTick++;
            clientPredictedTick++;
            DelayTick = serverEstimatedTick - 5 - ((Client.RTT / 2) / 20);
            if (DelayTick < 0)
            {
                DelayTick = 0;
            }
        }
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();

        Client.Connected -= DidConnect;
        Client.ConnectionFailed -= FailedToConnect;
        Client.ClientDisconnected -= PlayerLeft;
        Client.Disconnected -= DidDisconnect;
    }

    public void Connect()
    {
        Client.Connect(ip+":"+port);
        GameManager.Singleton.SetCursorLocked(true);
        MessageDialog.Open("Info:", "Connecting to server...");
    }

    public void DisconnectFromServer()
    {
        Client.Disconnect();
        GameManager.Singleton.SetCursorLocked(false);
        GameManager.Singleton.PlayerOnDisconnected();
    }

    private void DidConnect(object sender, EventArgs e)
    {
        MessageDialog.Close();
        SendName();
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        MessageDialog.Close();
        UIManager.DisplayMainMenu();
        GameManager.Singleton.SetCursorLocked(false);
        MessageDialog.Open("Connection timeout:", "Server could not be reached!", null, () =>
        {
            // Confirm
            UI_Blocker.Close();
        }, "Confirm"); 
    }
    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Id].gameObject);
    }

    public void EstimateClientServerStartTick(int serverTick)
    {
        serverEstimatedTick = Mathf.RoundToInt(serverTick + ((Client.RTT / 2) / 20));
        clientPredictedTick = Mathf.RoundToInt(serverTick + ((Client.RTT / 2) / 20) * 2);
        receivedServerStartTick = true;
    }

    public void OnGUI()
    {
        //GUI.Label(new Rect(10, 10,500, 20), $"STick: {serverEstimatedTick} CTick: {clientPredictedTick} DTick: {DelayTick} SCDIF: {clientPredictedTick - serverEstimatedTick}");
    }

    public int EstimateServerTick(int serverTick)
    {
        if (serverEstimatedTick > serverTick)
            return serverTick;

        int serverCalculatedTick = Mathf.RoundToInt(serverTick + ((Client.RTT / 2) / 20));
        if (serverEstimatedTick != serverCalculatedTick)
        {
            return serverCalculatedTick;
        } else
        {
            return serverTick;
        }
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        Debug.Log($"Server closes the connection, returning to main menu!");
        GameManager.Singleton.PlayerOnDisconnected();
        MessageDialog.Open("Server Notice:", "Server closed the connection", null, () =>
        {
            // Confirm
            UI_Blocker.Close();
        }, "Confirm");
    }

    public void ResetTicks()
    {
        receivedServerStartTick = false;
        clientPredictedTick = 0;
        serverEstimatedTick = 0;
        DelayTick = 0;
    }

    #region Messages
    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerId.playerName);
        message.Add(PlayerPrefs.GetString("PlayerName"));
        Client.Send(message);
    }

    public void RequestSpawn()
    {
        Debug.Log($"Player requested spawn from the server.");
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerId.requestSpawn);
        message.Add(PlayerPrefs.GetString("PlayerName"));
        Client.Send(message);
    }
    #endregion
}
