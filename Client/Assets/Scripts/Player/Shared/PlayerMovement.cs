using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Player player;
    public CharacterController controller;

    /*Movement Test*/
    /*Frame occuring factors*/
    public float gravity = 20.0f;
    public float friction = 5.5f; //Ground friction

    /* Movement stuff */
    public float baseMoveSpeed = 210;             // The base speed of the player
    public float jumpSpeed = 39;                  // The speed at which the character's up axis gains when hitting jump in cod units
    public float strafeSpeedScale = 0.8f;
    public float backwardSpeedScale = 0.7f;
    public float sprintSpeedScale = 1.5f;
    public float runSpeedScale = 1f;
    [SerializeField] private float jumpForce;
    [SerializeField] private float moveSpeed = 7.0f;                // Ground move speed
    public float runAcceleration = 14.0f;         // Ground accel
    public float runDeacceleration = 10.0f;       // Deacceleration that occurs when running on the ground
    public float airAcceleration = 2.0f;          // Air accel
    public float airDecceleration = 2.0f;         // Deacceleration experienced when ooposite strafing
    public float airControl = 0.3f;               // How precise air control is
    public float sideStrafeAcceleration = 50.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed when
    public float sideStrafeSpeed = 1.0f;          // What the max speed to generate when side strafing
    public float groundedCheckDistance = 0.1f;
    public bool WasGrounded = true;
    public bool CanJump = false;
    public bool CanSprint = true;
    public bool CanCrouch = true;

    public bool Onladder = false;

    public StanceProperties[] m_StanceProperties;     //0 is standing 1 is crouching and 2 is prone

    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    private Vector3 groundNormal = Vector3.zero;

    // Q3: players can queue the next jump just before he hits the ground
    private bool wishJump = false;

    // Used to display real time fricton values
    private float playerFriction = 0.0f;
    /* Test End*/

    bool hasreleasedjump = true;
    private Quaternion playerTargetRot;

    public Transform Aimdirection;
    public Transform CameraBase;

    private float CurrentPlayerHeight;
    private Vector3 CurrentPlayerCenter;
    private Vector3 CurrentCameraHeight;

    private void Start()
    {
        playerTargetRot = transform.localRotation;
        moveSpeed = ConvertFromCodUnits(baseMoveSpeed);
        jumpForce = CalculateJumpHeight();
        CurrentPlayerHeight = controller.height;
        CurrentPlayerCenter = controller.center;
        if (player.isLocalplayer)
            CurrentCameraHeight = CameraBase.localPosition;
    }

    public void Move(PlayerCMD inputs)
    {
        if (inputs == null)
        {
            return;
        }
        // Set orientation based on the view direction.
        Quaternion targetRotation = Quaternion.Euler(0f, inputs.viewDirection.y, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10);
        //transform.rotation = Quaternion.Euler(0, inputs.viewDirection.y, 0);

        player.clientstate.ViewAngles = Quaternion.Euler(inputs.viewDirection.x, inputs.viewDirection.y, 0f);
        Aimdirection.rotation = player.clientstate.ViewAngles;

        QueueJump(inputs);
        CheckForCrouch(inputs);
        if (player.clientstate.Grounded)
        {
            if (!WasGrounded)
            {
                if (!inputs.jump)
                {
                    CanJump = true;
                }
                //controller.height = 1.8f;
                WasGrounded = true;
            }
            if(Onladder)
            {
                LadderMove(inputs);
            } else
            {
                GroundMove(inputs);
            }
        }
        else if (!player.clientstate.Grounded)
        {
            if (WasGrounded)
            {
                CanJump = false;
                //controller.height = 1.4f;
                WasGrounded = false;
            }
            if (Onladder)
            {
                LadderMove(inputs);
            }
            else
            {
                AirMove(inputs);
            }
        }

        controller.height = CurrentPlayerHeight;
        controller.center = CurrentPlayerCenter;

        if (player.isLocalplayer)
        {
            // Set the player's camera height
            CameraBase.localPosition = Vector3.MoveTowards(CameraBase.localPosition, CurrentCameraHeight, Time.deltaTime * 3f);
        }

        player.clientprediction.MovementDirection = playerVelocity;
        player.clientthirdpersoncontroller.UpdatePlayerModelState(player.clientstate);

        if (controller.enabled)
        {
            // Move the controller, and set grounded true or false depending on whether we're standing on something
            player.clientstate.Grounded = (controller.Move(player.clientprediction.MovementDirection * Time.fixedDeltaTime) & CollisionFlags.Below) != 0;
        }
    }

    /**
     * Queues the next jump just like in Q3
     */
    private void QueueJump(PlayerCMD inputs)
    {
        if (inputs.jump && player.clientstate.PlayerStance != 0)
        {
            CanJump = false;
            SetPlayerStance(0);
            return;
        }

        if (inputs.jump && !wishJump && CanJump)
        {
            wishJump = true;
            CanJump = false;
        }

        if (!inputs.jump)
        {
            wishJump = false;
            if (player.clientstate.Grounded)
            {
                CanJump = true;
            }
        }
    }

    private void CheckForCrouch(PlayerCMD inputs)
    {
        if (player.clientstate.Sprinting)
        {
            SetPlayerStance(0);
            return;
        }
        //TODO add raycast check above the player's head
        if (inputs.crouch && CanCrouch)
        {
            CanCrouch = false;
            if (player.clientstate.PlayerStance == (int)PlayerStance.Standing)
            {
                SetPlayerStance(1);
            }
            else
            {
                SetPlayerStance(0);
            }
        }

        if (!inputs.crouch && !CanCrouch)
        {
            CanCrouch = true;
        }
    }

    private void SetPlayerStance(int Stance)
    {
        PlayerStance stance = (PlayerStance)Stance;

        switch (stance)
        {
            case PlayerStance.Standing:
                CurrentPlayerHeight = m_StanceProperties[0].PlayerStanceHeight;
                CurrentPlayerCenter = m_StanceProperties[0].PlayerStanceCenter;
                CurrentCameraHeight = m_StanceProperties[0].PlayerCameraOffset;
                player.clientstate.PlayerStance = 0;
                break;
            case PlayerStance.Crouched:
                CurrentPlayerHeight = m_StanceProperties[1].PlayerStanceHeight;
                CurrentPlayerCenter = m_StanceProperties[1].PlayerStanceCenter;
                CurrentCameraHeight = m_StanceProperties[1].PlayerCameraOffset;
                player.clientstate.PlayerStance = 1;
                break;
        }
    }

    /**
     * Execs when the player is in the air
    */
    private void AirMove(PlayerCMD inputs)
    {
        Vector3 wishdir;
        float wishvel = airAcceleration;
        float accel;

        if (player.clientstate.Sprinting)
        {
            player.clientstate.Sprinting = false;
        }

        wishdir = new Vector3(inputs.sideMove, 0, inputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        // CPM: Aircontrol
        float wishspeed2 = wishspeed;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
            accel = airDecceleration;
        else
            accel = airAcceleration;
        // If the player is ONLY strafing left or right
        if (inputs.forwardMove == 0 && inputs.sideMove != 0)
        {
            if (wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel);
        if (airControl > 0)
            AirControl(inputs, wishdir, wishspeed2);
        // !CPM: Aircontrol

        // Apply gravity
        playerVelocity.y -= gravity * Time.fixedDeltaTime;
    }

    /**
     * Air control occurs when the player is in the air, it allows
     * players to move side to side much faster rather than being
     * 'sluggish' when it comes to cornering.
     */
    private void AirControl(PlayerCMD inputs, Vector3 wishdir, float wishspeed)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if (Mathf.Abs(inputs.forwardMove) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;
        zspeed = playerVelocity.y;
        playerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        dot = Vector3.Dot(playerVelocity, wishdir);
        k = 32;
        k *= airControl * dot * dot * Time.fixedDeltaTime;

        // Change direction while slowing down
        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
            playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
            playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

            playerVelocity.Normalize();
            moveDirectionNorm = playerVelocity;
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zspeed; // Note this line
        playerVelocity.z *= speed;
    }

    /**
     * Called every frame when the engine detects that the player is on the ground
     */
    private void GroundMove(PlayerCMD inputs)
    {
        Vector3 wishdir;
        float accel = runAcceleration;
        float scale = Cmd_Scale(inputs);

        // Do not apply friction if the player is queueing up the next jump
        if (!wishJump)
            ApplyFriction(scale);
        else
            ApplyFriction(0);


        wishdir = new Vector3(inputs.sideMove, 0, inputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        var wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed * scale;


        Accelerate(wishdir, wishspeed, accel);

        player.clientstate.PlayerDirection = new Vector2(inputs.sideMove, inputs.forwardMove);

        playerVelocity.y = -2f; //This stops the bounce effects when walking on slopes

        if (wishJump)
        {
            playerVelocity.y = CalculateJumpHeight();
            wishJump = false;
        }
    }

    /**
     * Called every frame when the engine detects that the player is on a ladder
     */
    private void LadderMove(PlayerCMD inputs)
    {
        Vector3 wishdir;
        float accel = runAcceleration;
        float scale = Cmd_Scale(inputs);
        float upscale;

        upscale = (Aimdirection.localEulerAngles.x + 0.5f) * 2.5f;
        if (upscale > 1.0f)
        {
            upscale = 1.0f;
        }
        else if (upscale < -1.0f)
        {
            upscale = -1.0f;
        }

        ApplyFriction(scale);

        
        wishdir = new Vector3(inputs.sideMove, inputs.forwardMove, 0f);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        var wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed * scale * 0.9f * upscale * inputs.forwardMove;

        //playerVelocity.y = 0.9f * upscale * scale * inputs.forwardMove;

        LadderAccelerate(wishdir, wishspeed, accel);

        //Accelerate(wishdir, wishspeed, accel);

        player.clientstate.PlayerDirection = new Vector2(inputs.sideMove, inputs.forwardMove);
    }

    float CalculateJumpHeight()
    {
        return Mathf.Sqrt((ConvertFromCodUnits(jumpSpeed) * 2) * gravity);    //From Cod4 Unleashed source P_Move Jump_Start
    }

    private float Cmd_Scale(PlayerCMD inputs)
    {
        float scale = 1;

        if (player.clientstate.PlayerStance == (int)PlayerStance.Crouched)
        {
            if(player.clientstate.Grounded)
            {
                scale *= 0.3f;
            } else
            {
                scale *= 1.5f;
            }
        }

        if (inputs.sprint && inputs.forwardMove > 0.01f && CanSprint)
        {
            if (inputs.attack)
            {
                CanSprint = false;
            }
            scale *= sprintSpeedScale;
            inputs.sprint = true;
            player.clientstate.Sprinting = true;
        }
        else
        {
            if (!inputs.attack && !inputs.sprint)
            {
                CanSprint = true;
            }
            scale *= runSpeedScale;
            inputs.sprint = false;
            player.clientstate.Sprinting = false;
        }

        if (inputs.forwardMove < -0.01f && inputs.sideMove == 0)
        {
            scale *= backwardSpeedScale;
        }
        if (Mathf.Abs(inputs.sideMove) > 0.01f && inputs.forwardMove == 0)
        {
            scale *= strafeSpeedScale;
        }

        /*
        if (pm->ps->pm_type == PM_NOCLIP)
        {
            scale *= 3;
        }
        */

        return scale;
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ladder")
        {
            Onladder = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ladder")
        {
            Onladder = false;
        }
    }
    
    /**
     * Applies friction to the player, called in both the air and on the ground
     */
    private void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity; // Equivalent to: VectorCopy();
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        if (player.clientstate.Grounded)
        {
            control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * friction * Time.fixedDeltaTime * t;
        }

        newspeed = speed - drop;
        playerFriction = newspeed;
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }

    private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * Time.fixedDeltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }

    private void LadderAccelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * Time.fixedDeltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.y += accelspeed * wishdir.y;
        playerVelocity.z += accelspeed * wishdir.z;
    }

    public float ConvertToCodUnits(float unit)
    {
        return (unit / 0.0254f);
    }

    public float ConvertFromCodUnits(float unit)
    {
        return (unit * 0.0254f);
    }
}

[System.Serializable]
public class StanceProperties
{
    public string StanceName;
    public float PlayerStanceHeight;
    public Vector3 PlayerStanceCenter;
    public Vector3 PlayerCameraOffset;
}