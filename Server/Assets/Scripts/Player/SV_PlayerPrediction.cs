using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SV_PlayerPrediction : MonoBehaviour
{
    [SerializeField] private Player player;

    [Header("Client Prediction")]
    public Queue<PlayerCMD> clientInputs = new Queue<PlayerCMD>();
    public Vector3 MovementDirection;
    private static PlayerCMD DefaultInputState = new PlayerCMD();

    private void Start()
    {
       
    }

    // FixedUpdate is called once per physics frame
    void FixedUpdate()
    {
        // Declare the ClientInputState that we're going to be using.
        PlayerCMD inputState = null;

        // Obtain CharacterInputState's from the queue. 
        while (clientInputs.Count > 0 && (inputState = clientInputs.Dequeue()) != null)
        {
            // Process the input.
            player.clientmovement.Move(inputState);
            player.clientweaponcontroller.DetermineWeaponState(inputState);

            // Obtain the current SimulationState.
            SimulationState state = CurrentSimulationState(inputState);

            // Send the state back to the client.
            SendServerStateToClient(state);
            state.tick = NetworkManager.Singleton.Tick;
            SendServerStateToRemoteClients(state, inputState.viewDirection);
        }
    }


    public void OnServerClientInputsReceived(PlayerCMD playerinputs)
    {
        player.clientinputs = playerinputs;
        clientInputs.Enqueue(playerinputs);
    }

    public SimulationState CurrentSimulationState(PlayerCMD inputState)
    {
        return new SimulationState
        {
            position = transform.position,
            rotation = transform.rotation,
            velocity = MovementDirection,
            clientstate = player.clientstate,
            tick = inputState.tick
        };
    }

    #region Messages
    public void SendServerStateToClient(SimulationState serverState) //, PlayerCMD serverinputstate
    {
        Message message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToClientId.serverCSPState);
        //Sending the simulation state
        message.Add(player.Id);
        message.Add(serverState.position);
        message.Add(serverState.rotation);
        message.Add(serverState.velocity);
        message.Add(serverState.tick);

        message.Add(serverState.clientstate.Grounded);
        message.Add(serverState.clientstate.Sprinting);
        message.Add(serverState.clientstate.Firing);
        message.Add(serverState.clientstate.Reloading);
        message.Add(serverState.clientstate.Aim);
        message.Add(serverState.clientstate.SwitchWeapon);
        message.Add(serverState.clientstate.PlayerStance);
        /*
        //Sending the input state
        message.Add(serverinputstate.forwardMove);
        message.Add(serverinputstate.sideMove);
        message.Add(serverinputstate.viewDirection);
        message.Add(serverinputstate.rotation);
        message.Add(serverinputstate.jump);
        message.Add(serverinputstate.sprint);
        */
        NetworkManager.Singleton.Server.Send(message, this.player.returnServerClientFromPlayer());
    }

    public void SendServerStateToRemoteClients(SimulationState serverState, Vector3 viewDirection) //, PlayerCMD serverinputstate
    {
        Message message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToClientId.serverInterpolationState);
        //Sending the interpolation state
        message.Add(player.Id);
        message.Add(serverState.position);
        message.Add(serverState.rotation);
        //message.Add(serverState.velocity);
        message.Add(serverState.tick);
        /*
        //Sending the input state
        message.Add(serverinputstate.forwardMove);
        message.Add(serverinputstate.sideMove);
        message.Add(serverinputstate.viewDirection);
        message.Add(serverinputstate.rotation);
        message.Add(serverinputstate.jump);
        message.Add(serverinputstate.sprint);
        */
        //Send clientstate
        message.Add(serverState.clientstate.PlayerDirection);
        message.Add(serverState.clientstate.Grounded);
        message.Add(serverState.clientstate.Sprinting);
        message.Add(serverState.clientstate.Firing);
        message.Add(serverState.clientstate.Reloading);
        message.Add(serverState.clientstate.Aim);
        message.Add(serverState.clientstate.SwitchWeapon);
        message.Add(serverState.clientstate.PlayerStance);
        //Send view direction
        message.Add(viewDirection);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
    #endregion
}

[System.Serializable]
public class SimulationState
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public PlayerState clientstate;
    public int tick;
}