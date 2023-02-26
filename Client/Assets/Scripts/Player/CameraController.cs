using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float XSensitivity = 2f;
    [SerializeField] private float YSensitivity = 2f;
    [SerializeField] private float MinimumX = -90F;
    [SerializeField] private float MaximumX = 90F;
    [SerializeField] private bool smooth;
    [SerializeField] private float smoothTime = 5f;

    private float rotX = 0.0f;
    private float rotY = 0.0f;

    private void Start()
    {
        Transform scenecamera = GameObject.Find("SceneCamera").transform;
        scenecamera.position = this.transform.position;
        scenecamera.rotation = this.transform.rotation;
        scenecamera.parent = this.transform;
        Init(player.transform);
    }

    public void Init(Transform character)
    {
        rotY = character.eulerAngles.y;
    }

    private void Update()
    {
        Look();

        Debug.DrawRay(transform.position, transform.forward * 2, Color.green);
    }

    public void Look()
    {
        if (Cursor.lockState == CursorLockMode.Locked && !GameManager.Singleton.IsPaused)
        {
            rotX -= player.clientinputs.mouseY * XSensitivity;
            rotY += player.clientinputs.mouseX * YSensitivity;
        } else
        {
            rotX -= 0f;
            rotY += 0f;
        }

        rotX = Mathf.Clamp(rotX, MinimumX, MaximumX);

        // Set orientation.
        // The camera is allowed to move freely not dependent on any tick rate or anything.
        // The network layer will snapshot the camera direction along with player inputs.
        // But this means the player can freely look around at say, 144hz, even if we're running
        // a much lower physics or network tick rate.
        transform.rotation = Quaternion.Euler(rotX, rotY, 0);
    }

    /*
    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
    */
}
