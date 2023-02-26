using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_PlayerPrediction : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerMovement movementcontroller;
    [SerializeField] private PlayerWeaponController playerweaponcontroller;

    [Header("Client Prediction:")]
    /*Client prediction variables */
    public float Distancetolerance = 0.02f; //The amount of distance in units that we will allow the client's prediction to drift from it's position on the server, before a correction is necessary. 
    public float Snapdistance = 2f; //The amount of distance in units when we just snap to the server position.
    public SimulationState serverSimulationState;   //Latest simulation state from the server
    private static PlayerCMD DefaultInputState = new PlayerCMD();
    [HideInInspector] public int CurrentSimulationTick = 0;
    private const int STATE_CACHE_SIZE = 256;
    public SimulationState[] simulationStateCache = new SimulationState[STATE_CACHE_SIZE];
    public PlayerCMD[] inputStateCache = new PlayerCMD[STATE_CACHE_SIZE];
    private int lastCorrectedTick;
    public Vector3 MovementDirection;

    [Space(10)]

    [Header("Client Prediction Debug:")]
    public SimulationState clientSimulationState;  //Only for debug
    private Vector3 ReconciliationDebug;
    private int ReconciliationCorrections;
    [HideInInspector] public float DifferenceDistance;

    // FixedUpdate is called once per physics frame
    void FixedUpdate()
    {
        if(player.isLocalplayer)
        {
            //CurrentSimulationTick = GameManager.Singleton.Tick;
            player.clientinputs.tick = NetworkManager.Singleton.clientPredictedTick;

            PlayerCMD playerinputs;

            if(!GameManager.Singleton.IsPaused)
            {
                playerinputs = player.clientinputs;
                movementcontroller.Move(playerinputs);
                playerweaponcontroller.DetermineWeaponState(playerinputs);
            } else
            {
                playerinputs = new PlayerCMD();
                playerinputs.viewDirection = player.clientinputs.viewDirection;
                playerinputs.tick = NetworkManager.Singleton.clientPredictedTick;
            }

            SendInput(playerinputs);

            // Reconciliate if there's a message from the server.
            if (serverSimulationState != null) Reconciliate();

            // Get the current simulation state.
            SimulationState simulationState = CurrentSimulationState(player.clientinputs);

            clientSimulationState = simulationState;

            // Determine the cache index based on on modulus operator.
            int cacheIndex = NetworkManager.Singleton.clientPredictedTick % STATE_CACHE_SIZE;

            // Store the SimulationState into the simulationStateCache 
            simulationStateCache[cacheIndex] = simulationState;

            // Store the ClientInputState into the inputStateCache
            inputStateCache[cacheIndex] = player.clientinputs;
            //CurrentSimulationTick++;
        }
    }

    private void Reconciliate()
    {
        // Sanity check, don't reconciliate for old states.
        if (serverSimulationState.tick <= lastCorrectedTick)
        {
            //Debug.LogWarning($"Serversimulated tick is older than lastCorrectedTick for Tick {serverSimulationState.tick}.");
            return;
        }

        // Determine the cache index 
        int cacheIndex = serverSimulationState.tick % STATE_CACHE_SIZE;

        // Obtain the cached input and simulation states.
        PlayerCMD cachedInputState = inputStateCache[cacheIndex];
        SimulationState cachedSimulationState = simulationStateCache[cacheIndex];

        // If there's missing cache data for either input or simulation 
        // snap the player's position to match the server.
        if (cachedInputState == null || cachedSimulationState == null)
        {
            //ReconciliationCorrections++;
            /*
            this.transform.position = serverSimulationState.position;
            this.MovementDirection = serverSimulationState.velocity;
            //this.transform.rotation = serverSimulationState.rotation;

            // Set the last corrected frame to equal the server's frame.
            lastCorrectedTick = serverSimulationState.tick;

            Debug.LogWarning($"Snapped to server position for tick {lastCorrectedTick}.");
            */
            return;
        }

        Vector3 ReconciliationDebug = (cachedSimulationState.position - serverSimulationState.position);
        DifferenceDistance = ReconciliationDebug.magnitude;

        if (DifferenceDistance > Snapdistance)
        {
            ReconciliationCorrections++;
            this.transform.position = serverSimulationState.position;
            this.MovementDirection = serverSimulationState.velocity;
            Debug.LogWarning($"Client's values are over snapping treshold of {Snapdistance} units! Snapped to server position for tick {NetworkManager.Singleton.clientPredictedTick}.");

            // Set the last corrected frame to equal the server's frame.
            lastCorrectedTick = serverSimulationState.tick;
            return;
        }
        else if (DifferenceDistance > Distancetolerance)
        {
            ReconciliationCorrections++;
            // Set the player's position to match the server's state. 
            this.transform.position = serverSimulationState.position;
            this.MovementDirection = serverSimulationState.velocity;

            // Declare the rewindFrame as we're about to resimulate our cached inputs. 
            int rewindFrame = serverSimulationState.tick;

            // Loop through and apply cached inputs until we're 
            // caught up to our current simulation frame. 
            while (rewindFrame < NetworkManager.Singleton.clientPredictedTick)
            {
                // Determine the cache index 
                int rewindCacheIndex = rewindFrame % STATE_CACHE_SIZE;

                // Obtain the cached input and simulation states.
                PlayerCMD rewindCachedInputState = inputStateCache[rewindCacheIndex];
                SimulationState rewindCachedSimulationState = simulationStateCache[rewindCacheIndex];

                // If there's no state to simulate, for whatever reason, 
                // increment the rewindFrame and continue.
                if (rewindCachedInputState == null || rewindCachedSimulationState == null)
                {
                    ++rewindFrame;
                    continue;
                }

                // Process the cached inputs. 
                movementcontroller.Move(rewindCachedInputState);
                playerweaponcontroller.DetermineWeaponState(rewindCachedInputState);

                // Replace the simulationStateCache index with the new value.
                SimulationState rewoundSimulationState = CurrentSimulationState(rewindCachedInputState);
                rewoundSimulationState.tick = rewindFrame;
                simulationStateCache[rewindCacheIndex] = rewoundSimulationState;

                // Increase the amount of frames that we've rewound.
                ++rewindFrame;
            }
            if (DifferenceDistance > Distancetolerance) //If we have still a difference in the predictions.
            {
                this.transform.position = serverSimulationState.position;
                this.MovementDirection = serverSimulationState.velocity;
                // Set the last corrected frame to equal the server's frame.
                lastCorrectedTick = serverSimulationState.tick;
            }
        }

        // Once we're complete, update the lastCorrectedTick to match.
        // NOTE: Set this even if there's no correction to be made. 
        lastCorrectedTick = serverSimulationState.tick;

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

    public void OnClientServerStateReceived(SimulationState serverState)
    {
        if (serverSimulationState?.tick < serverState.tick)
        {
            serverSimulationState = serverState;
        }
    }

    #region Messages
    private void SendInput(PlayerCMD inputstate)
    {
        Message message = Message.Create(MessageSendMode.Unreliable, (ushort)ClientToServerId.playerInput);
        message.Add(inputstate.forwardMove);
        message.Add(inputstate.sideMove);
        message.Add(inputstate.viewDirection);
        message.Add(inputstate.jump);
        message.Add(inputstate.sprint);
        message.Add(inputstate.crouch);
        message.Add(inputstate.prone);
        message.Add(inputstate.attack);
        message.Add(inputstate.aim);
        message.Add(inputstate.reload);
        message.Add(inputstate.switchweapon);
        message.Add(inputstate.tick);
        NetworkManager.Singleton.Client.Send(message);
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
