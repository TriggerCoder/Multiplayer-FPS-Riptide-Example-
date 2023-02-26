using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    [Header("Player Network Stats")]
    [SerializeField] private ushort id;
    [SerializeField] private string username;
    [SerializeField] private short ping;
    public string CurrentWeapon;
    public bool isLocalplayer = false;

    [Header("Player Network Components")]
    public PlayerCMD clientinputs;
    public PlayerState clientstate;
    public C_PlayerPrediction clientprediction;
    public PlayerInterpolation clientinterpolation;
    public PlayerWeaponController clientweaponcontroller;
    public PlayerThirdperson clientthirdpersoncontroller;

    [Header("Player Other Components")]
    public Text NameText;

    private void FixedUpdate()
    {
        if (isLocalplayer)
            ping = NetworkManager.Singleton.Client.RTT;
    }

    public void Move(Vector3 newPosition, Vector3 forward)
    {
        transform.position = newPosition;

        if (id != NetworkManager.Singleton.Client.Id) // Don't overwrite local player's forward direction to avoid noticeable rotational snapping
            transform.forward = forward;
    }

    private void OnDestroy()
    {
        list.Remove(id);
    }

    public static void Spawn(ushort id, string username, Vector3 position, Quaternion rotation, string PrimaryWeapon, string SecondaryWeapon)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(NetworkManager.Singleton.LocalPlayerPrefab, position, rotation).GetComponent<Player>();
            player.isLocalplayer = true;
        }
        else
            player = Instantiate(NetworkManager.Singleton.PlayerPrefab, position, rotation).GetComponent<Player>();

        player.name = $"Player {id} ({username})";
        player.id = id;
        player.username = username;
        if (!player.isLocalplayer)
            player.NameText.text = username;

        if(PrimaryWeapon != null || PrimaryWeapon != "")
        {
            Debug.Log(PrimaryWeapon);
            player.GiveWeapon(PrimaryWeapon);
            player.SwitchToWeapon(PrimaryWeapon);
        }
        list.Add(player.id, player);
    }

    #region Player Weapon
    public void GiveWeapon(string WeaponName)
    {
        if (this.isLocalplayer)
        {
            clientweaponcontroller.RegisterPlayerWeapon(WeaponName);
            clientweaponcontroller.SpawnViewModel(WeaponName);
        }

        clientthirdpersoncontroller.SetThirdPersonWeapon(WeaponName, null);
    }

    public void SwitchToWeapon(string WeaponName)
    {
        if (this.isLocalplayer)
        {
            clientweaponcontroller.OnWeaponDraw(WeaponName);
            clientweaponcontroller.ViewModelDraw(WeaponName, false);
        }
    }

    public void TakeWeapon()
    {

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