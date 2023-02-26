using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;

public class GameModePauseMenu : MonoBehaviour
{
    private static GameModePauseMenu instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"Second gamemode pause menu detected? This ain't right");
        }

        //GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        //GetComponent<RectTransform>().sizeDelta = Vector2.zero;
    }

    private void Start()
    {
        GameManager.OnClientPauseStateChangedEvent += DisplayPauseMenu;
        Debug.Log("TEST INIT");
        Close();
    }

    /// <summary>Opens the Gamemode Pause Menu</summary>
    public static void Open()
    {
        UI_Blocker.Open();
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
    }

    /// <summary>Closes the Gamemode Pause Menu</summary>
    public static void Close()
    {
        UI_Blocker.Close();
        instance.gameObject.SetActive(false);
    }

    public void DisplayPauseMenu(bool pauseState)
    {
        Debug.Log(pauseState);
        if(pauseState)
        {
            Open();
        } else
        {
            Close();
        }
    }

    public void ResumeGame()
    {
        GameManager.Singleton.IsPaused = false;
        GameManager.Singleton.SetPausedState(false);
    }

    public void DisconnectFromServer()
    {
        Close();
        NetworkManager.Singleton.DisconnectFromServer();
        DestroyInstance();
    }

    public void DestroyInstance()
    {
        GameManager.OnClientPauseStateChangedEvent -= DisplayPauseMenu;
        instance = null;
        Destroy(gameObject);
    }
}
