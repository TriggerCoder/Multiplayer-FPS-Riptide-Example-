using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Menu : MonoBehaviour
{
    public string MenuName;
    public bool Open = false;
    public GameObject MenuGameObject;

    public void CloseMenu()
    {
        Open = false;
        MenuGameObject.SetActive(false);
    }

    public void OpenMenu()
    {
        Open = true;
        MenuGameObject.SetActive(true);
    }
}
