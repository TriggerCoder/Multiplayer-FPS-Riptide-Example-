using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform camTransform;
    [SerializeField] private Player player;
    public C_PlayerPrediction clientprediction;

    private void Update()
    {
        player.clientinputs = new PlayerCMD
        {
            forwardMove = Input.GetAxisRaw("Vertical"),
            sideMove = Input.GetAxisRaw("Horizontal"),
            viewDirection = camTransform.rotation.eulerAngles,
            jump = Input.GetKey(KeyCode.Space),
            sprint = Input.GetKey(KeyCode.LeftShift),
            crouch = Input.GetKey(KeyCode.C),
            prone = Input.GetKey(KeyCode.LeftControl),
            attack = Input.GetKey(KeyCode.Mouse0),
            aim = Input.GetKey(KeyCode.Mouse1),
            reload = Input.GetKey(KeyCode.R),
            switchweapon = Input.GetKey(KeyCode.Alpha1),
            mouseX = Input.GetAxisRaw("Mouse X"),
            mouseY = Input.GetAxisRaw("Mouse Y"),
        };
    }
}

