using Riptide;
using UnityEngine;
public class ServerHandle : MonoBehaviour
{
	[MessageHandler((ushort)ClientToServerId.playerName)]
	public static void PlayerName(ushort fromClient, Message message)
    {
        NetworkManager.Singleton.SendServerInfo(fromClient);
    }

	[MessageHandler((ushort)ClientToServerId.requestSpawn)]
	public static void RequestSpawn(ushort fromClient, Message message)
    {
        NetworkManager.Singleton.SendExistingPlayersForClient(fromClient);
        Player.Spawn(fromClient, message.GetString(), "Pistol45", "Pistol50");
    }

	[MessageHandler((ushort)ClientToServerId.playerInput)]
	public static void PlayerInput(ushort fromClient, Message message)
    {
        PlayerCMD _clientinputs = new PlayerCMD
        {
            forwardMove = message.GetFloat(),
            sideMove = message.GetFloat(),
            viewDirection = message.GetVector3(),
            jump = message.GetBool(),
            sprint = message.GetBool(),
            crouch = message.GetBool(),
            prone = message.GetBool(),
            attack = message.GetBool(),
            aim = message.GetBool(),
            reload = message.GetBool(),
            switchweapon = message.GetBool(),
            tick = message.GetInt(),
        };
        Player.List[fromClient].SetInput(_clientinputs);
    }
}
