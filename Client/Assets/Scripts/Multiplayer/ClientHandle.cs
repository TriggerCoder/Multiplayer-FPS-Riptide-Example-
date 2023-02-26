using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
	[MessageHandler((ushort)ServerToClientId.sendServerInfo)]
	public static void PlayerLoadServerMap(Message message)
    {
        ushort playerId = message.GetUShort();
        GameManager.Singleton.LoadMap(message.GetString());
        if(!GameModeManager.LoadGameMode(message.GetString()))
        {
            NetworkManager.Singleton.Client.Disconnect();
            return;
        }
    }

	[MessageHandler((ushort)ServerToClientId.spawnPlayer)]
	public static void SpawnPlayer(Message message)
    {
        Player.Spawn(message.GetUShort(), message.GetString(), message.GetVector3(), message.GetQuaternion(), message.GetString(), message.GetString());
        NetworkManager.Singleton.EstimateClientServerStartTick(message.GetInt());
        GameManager.Singleton.SetGameState((GameState)message.GetInt());
    }

	[MessageHandler((ushort)ServerToClientId.playerMovement)]
	public static void PlayerMovement(Message message)
    {
        ushort playerId = message.GetUShort();
        Debug.Log("This should not appear.");
        if (Player.list.TryGetValue(playerId, out Player player))
            player.Move(message.GetVector3(), message.GetVector3());
    }

	[MessageHandler((ushort)ServerToClientId.serverCSPState)]
	public static void PlayerOnServerStateReceived(Message message)
    {
        ushort playerId = message.GetUShort();
        SimulationState serverstate = new SimulationState
        {
            position = message.GetVector3(),
            rotation = message.GetQuaternion(),
            velocity = message.GetVector3(),
            tick = message.GetInt(),
        };

        PlayerState serverclientState = new PlayerState
        {
            Grounded = message.GetBool(),
            Sprinting = message.GetBool(),
            Firing = message.GetBool(),
            Reloading = message.GetBool(),
            Aim = message.GetBool(),
            SwitchWeapon = message.GetBool(),
            PlayerStance = message.GetInt()
        };

        serverstate.clientstate = serverclientState;

        if (Player.list.TryGetValue(playerId, out Player player))
            player.clientprediction.OnClientServerStateReceived(serverstate);
    }
	[MessageHandler((ushort)ServerToClientId.serverInterpolationState)]
	public static void PlayerOnServerInterpolationStateReceived(Message message)
    {
        ushort playerId = message.GetUShort();
        InterpolationState serverstate = new InterpolationState
        {
            position = message.GetVector3(),
            rotation = message.GetQuaternion(),
            tick = message.GetInt(),
        };
        PlayerState serverplayerstate = new PlayerState
        {
            PlayerDirection = message.GetVector2(),
            Grounded = message.GetBool(),
            Sprinting = message.GetBool(),
            Firing = message.GetBool(),
            Reloading = message.GetBool(),
            Aim = message.GetBool(),
            SwitchWeapon = message.GetBool(),
            PlayerStance = message.GetInt(),
            ViewAngles = Quaternion.Euler(message.GetVector3()),
        };

        serverstate.clientstate = serverplayerstate;

        if (Player.list.TryGetValue(playerId, out Player player))
        {
            if(!player.isLocalplayer)
            {
                player.clientinterpolation.OnClientServerInterpolationStateReceived(serverstate);
                //player.clientstate = serverplayerstate;
            }
        }
    }
}
