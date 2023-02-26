using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInterpolation : MonoBehaviour
{
    [SerializeField] private Player player;
    public InterpolationState lastReceivedServerSimulationState;

    [Header("Tom's Interpolation")]
    private List<InterpolationState> futureTransformUpdates = new List<InterpolationState>(); // Oldest first

    private InterpolationState to;
    private InterpolationState from;
    private InterpolationState previous;

    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float timeToReachTarget = 0.1f;

    [SerializeField] private int teleportDistance = 5;
    [SerializeField] private int SnapshotDifference;


    public void Start()
    {
        to = CreateInterpolationState(transform.position, transform.rotation, NetworkManager.Singleton.serverEstimatedTick, new PlayerState());
        from = CreateInterpolationState(transform.position, transform.rotation, NetworkManager.Singleton.DelayTick, new PlayerState());
        previous = CreateInterpolationState(transform.position, transform.rotation, NetworkManager.Singleton.DelayTick, new PlayerState());
    }

    private void Update()
    {
        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (NetworkManager.Singleton.serverEstimatedTick >= futureTransformUpdates[i].tick)
            {
                previous = to;
                to = futureTransformUpdates[i];
                from = CreateInterpolationState(transform.position, transform.rotation, NetworkManager.Singleton.DelayTick, new PlayerState());
                futureTransformUpdates.RemoveAt(i);
                timeElapsed = 0;
                timeToReachTarget = (to.tick - from.tick) * 0.02f;
            }
        }

        timeElapsed += Time.deltaTime;
        Interpolate(timeElapsed / timeToReachTarget);
        this.player.clientthirdpersoncontroller.UpdatePlayerModelState(to.clientstate);
        this.player.clientstate = to.clientstate;
    }

    private void Interpolate(float _lerpAmount)
    {
        InterpolatePosition(_lerpAmount);
        InterpolateRotation(_lerpAmount);
    }

    private void InterpolatePosition(float _lerpAmount)
    {
        if (to.position == previous.position)
        {
            // If this object isn't supposed to be moving, we don't want to interpolate and potentially extrapolate
            if (to.position != from.position)
            {
                // If this object hasn't reached it's intended position
                transform.position = Vector3.Lerp(from.position, to.position, _lerpAmount); // Interpolate with the _lerpAmount clamped so no extrapolation occurs
            }
            return;
        }

        if(Mathf.Abs(Vector3.Distance(to.position,this.transform.position)) > teleportDistance)
        {
            transform.position = to.position;
        } else
        {
            transform.position = Vector3.LerpUnclamped(from.position, to.position, _lerpAmount); // Interpolate with the _lerpAmount unclamped so it can extrapolate
        }
    }

    private void InterpolateRotation(float _lerpAmount)
    {
        if (to.rotation == previous.rotation)
        {
            // If this object isn't supposed to be rotating, we don't want to interpolate and potentially extrapolate
            if (to.rotation != from.rotation)
            {
                // If this object hasn't reached it's intended rotation
                transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, _lerpAmount); // Interpolate with the _lerpAmount clamped so no extrapolation occurs
            }
            return;
        }

        transform.rotation = Quaternion.SlerpUnclamped(from.rotation, to.rotation, _lerpAmount); // Interpolate with the _lerpAmount unclamped so it can extrapolate
    }

    public void OnClientServerInterpolationStateReceived(InterpolationState serverState)
    {
        SnapshotDifference = serverState.tick - NetworkManager.Singleton.DelayTick;
        serverState.tick = NetworkManager.Singleton.EstimateServerTick(serverState.tick);
        if (serverState.tick <= NetworkManager.Singleton.DelayTick)
        {
            return;
        }

        if (futureTransformUpdates.Count == 0)
        {
            futureTransformUpdates.Add(CreateInterpolationState(serverState.position, serverState.rotation, serverState.tick, serverState.clientstate));
            return;
        }

        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (serverState.tick < futureTransformUpdates[i].tick)
            {
                // Transform update is older
                futureTransformUpdates.Insert(i, CreateInterpolationState(serverState.position, serverState.rotation, serverState.tick, serverState.clientstate));
                break;
            }
        }
    }

    public InterpolationState CreateInterpolationState(Vector3 position, Quaternion rotation, int tick, PlayerState playerstate)
    {
        return new InterpolationState
        {
            position = position,
            rotation = rotation,
            tick = tick,
            clientstate = playerstate,
        };
    }
}

[System.Serializable]
public class InterpolationState
{
    public Vector3 position;
    public Quaternion rotation;
    public int tick;
    public PlayerState clientstate;
}
