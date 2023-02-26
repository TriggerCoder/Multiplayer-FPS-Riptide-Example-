using Riptide;
using Riptide.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> List { get; private set; } = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }

    public string CurrentWeapon;
    public bool isLocalplayer = false;

    public Connection ServerClient;

    public PlayerCMD clientinputs;
    public PlayerState clientstate;
    public SV_PlayerPrediction clientprediction;
    public PlayerMovement clientmovement;
    public PlayerWeaponController clientweaponcontroller;
    public PlayerThirdperson clientthirdpersoncontroller;


    private void Start()
    {

    }

    public Connection returnServerClientFromPlayer()
    {
        if (ServerClient == null)
        {
            for (int i = 0; i < NetworkManager.Singleton.Server.Clients.Length; i++)
            {
                if (NetworkManager.Singleton.Server.Clients[i].Id == Id)
                {
                    ServerClient = NetworkManager.Singleton.Server.Clients[i];
                    return ServerClient;
                }
            }
            RiptideLogger.Log(Riptide.Utils.LogType.Error, $"Can't find client entry for client with id: {Id}");
            return null;
        } else
        {
            return ServerClient;
        }
    }

    public void SetInput(PlayerCMD _clientinputs)
    {
        clientinputs = _clientinputs;
        clientprediction.clientInputs.Enqueue(_clientinputs);
    }

    private void OnDestroy()
    {
        List.Remove(Id);
    }

    public static void Spawn(ushort id, string username, string PrimaryWeapon, string SecondaryWeapon)
    {

        Spawnpoint spawnpoint = GameManager.Singleton.ReturnRandomSpawnpoint();
        Player player;
        if(spawnpoint)
        {
            player = Instantiate(NetworkManager.Singleton.PlayerPrefab, spawnpoint.transform.position, spawnpoint.transform.rotation).GetComponent<Player>();
        } else
        {
            player = Instantiate(NetworkManager.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        }
        player.name = $"Player {id} ({(username == "" ? "Guest" : username)})";
        player.Id = id;
        player.Username = username;

        if (PrimaryWeapon != null || PrimaryWeapon != "")
        {
            player.GiveWeapon(PrimaryWeapon);
            player.SwitchToWeapon(PrimaryWeapon);
        }

        player.SendSpawn();
        List.Add(player.Id, player);
    }

    #region Player Weapon
    public void GiveWeapon(string WeaponName)
    {
        clientweaponcontroller.RegisterPlayerWeapon(WeaponName);
        //clientweaponcontroller.SpawnViewModel(WeaponName);
        clientthirdpersoncontroller.SetThirdPersonWeapon(WeaponName, null);
    }

    public void SwitchToWeapon(string WeaponName)
    {
        clientweaponcontroller.OnWeaponDraw(WeaponName);
    }

    public void TakeWeapon()
    {

    }
    #endregion

    #region Messages
    /// <summary>Sends a player's info to the given client.</summary>
    /// <param name="toClient">The client to send the message to.</param>
    public void SendSpawn(ushort toClient)
    {
        NetworkManager.Singleton.Server.Send(GetSpawnData(Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientId.spawnPlayer)), toClient);
    }
    /// <summary>Sends a player's info to all clients.</summary>
    private void SendSpawn()
    {
        NetworkManager.Singleton.Server.SendToAll(GetSpawnData(Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientId.spawnPlayer)));
    }

    private Message GetSpawnData(Message message)
    {
        message.Add(Id);
        message.Add(Username);
        message.Add(transform.position);
        message.Add(transform.rotation);
        message.Add("pistol45");
        message.Add("pistol50");
        message.Add(NetworkManager.Singleton.Tick);
        message.Add((int)GameManager.Singleton.GameState);
        return message;
    }

    public void SendMovement()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToClientId.playerMovement);
        message.Add(Id);
        message.Add(transform.position);
        message.Add(transform.forward);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
    #endregion
}

[System.Serializable]
public class PlayerCMD
{
    public float forwardMove;
    public float sideMove;
    public float upMove;
    public Vector3 viewDirection;
    public bool jump;
    public bool sprint;
    public bool crouch;
    public bool prone;
    public bool attack;
    public bool aim;
    public bool reload;
    public bool switchweapon;
    public float mouseX;
    public float mouseY;
    public int tick;
}

[System.Serializable]
public class PlayerState
{
    public Vector2 PlayerDirection;
    public bool Grounded;
    public bool Sprinting;
    public bool Firing;
    public bool Reloading;
    public bool Aim;
    public bool SwitchWeapon;
    public int PlayerStance = 0;
    public Quaternion ViewAngles;
}

public enum PlayerStance
{
    Standing,
    Crouched
}