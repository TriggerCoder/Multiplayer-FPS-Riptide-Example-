using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;
    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
            {
                _singleton = value;
                DontDestroyOnLoad(value.gameObject);
            }
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying object!");
                DestroyImmediate(value.gameObject);
            }
        }
    }

    public List<Menu> Menus = new List<Menu>();     //List of menu's to assign by the inspector.
    public Dictionary<string, Menu> MenuByName = new Dictionary<string, Menu>();    //Dictionary to get direct access to a menu by it's name
    public List<Menu> CurrentlyOpenMenus = new List<Menu>(); //List of opened menu's to keep track of in which order to close them again.
    public Transform MainMenuParent;  //Parent of the UI for the main menu
    [HideInInspector] public Transform Transform;  //Parent of the UI when ingame

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        UpdateMenusToNameDictionary();                  //Update our dictionary with menu's by name
        Transform = this.transform;
        GameManager.Singleton.InitMainMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) //&& !Developer_Console.GetActiveState()
        {
            CloseLastOpenedMenu_Internal();
        }
        /*
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            ToggleDeveloperConsole_Internal();
        }
        */
    }

    /// <summary>Retrieves all menu's from the Menus List and add them to the dictionary using their name as key</summary>
    private void UpdateMenusToNameDictionary() //Update our dictionary with menu's by name
    {
        MenuByName.Clear();
        if (Menus.Count < 1)
        {
            return;
        }
        for (int i = 0; i < Menus.Count; i++)
        {
            Debug.Log($"Loaded menu {Menus[i].MenuName}");
            MenuByName.Add(Menus[i].MenuName, Menus[i]);
        }
    }


    /// <summary>Internal function to open a menu using it's name</summary>
    /// <param name="MenuName"The name of the menu</param>
    private void OpenMenu_Internal(string MenuName)
    {
        MenuByName[MenuName].OpenMenu();    //Open the selected menu
        if (!MenuName.Equals("MainMenu"))
        {
            CurrentlyOpenMenus.Add(MenuByName[MenuName]);
        }
    }

    /// <summary>Internal function to close a menu using it's name</summary>
    /// <param name="MenuName"The name of the menu</param>
    private void CloseMenu_Internal(string MenuName)
    {
        MenuByName[MenuName].CloseMenu();    //Open the selected menu
    }

    /// <summary>Internal function to close the latest opened menu</summary>
    private void CloseLastOpenedMenu_Internal()
    {
        if (CurrentlyOpenMenus.Count == 0)
        {
            OpenMenu_Internal("MainMenu");
            return;
        }
        CloseMenu_Internal(CurrentlyOpenMenus[CurrentlyOpenMenus.Count - 1].MenuName); //Close the latest menu in the Open Menu list
        CurrentlyOpenMenus.RemoveAt(CurrentlyOpenMenus.Count - 1);  //Remove the last opened menu in the list.
        if (CurrentlyOpenMenus.Count > 0)    //If there is a previous opened menu
        {
            OpenMenu_Internal(CurrentlyOpenMenus[CurrentlyOpenMenus.Count - 1].MenuName); //Open the now latest menu in the Open Menu list
        }
        else
        {
            OpenMenu_Internal("MainMenu");
        }
    }

    /// <summary>Internal function to remove a menu from  the currently open menu list using its name</summary>
    private void RemoveMenuFromOpenList_Internal(string MenuName)
    {
        for (int i = 0; i < CurrentlyOpenMenus.Count; i++)
        {
            if (CurrentlyOpenMenus[i].MenuName == MenuName)
            {
                CurrentlyOpenMenus.RemoveAt(i);
            }
        }
        if (CurrentlyOpenMenus.Count < 1)
        {
            OpenMenu_Internal("MainMenu");
        }
    }

    /// <summary>Internal function to close all opened menu's</summary>
    private void CloseAllMenus_Internal()
    {
        foreach (Menu OpenedMenu in CurrentlyOpenMenus)
        {
            OpenedMenu.CloseMenu();
        }
    }

    /// <summary>Internal function to toggle the console</summary>
    private void ToggleDeveloperConsole_Internal()
    {
        /*
        if (Developer_Console.GetActiveState())
        {
            UI_Blocker.Close();
            Developer_Console.Close();
        }
        else
        {
            UI_Blocker.Open();
            Developer_Console.Open();
        }
        */
    }

    /// <summary>Public exposed function to open a menu using it's name</summary>
    /// <param name="MenuName"The name of the menu</param>
    public static void OpenMenu(string MenuName)
    {
        _singleton.OpenMenu_Internal(MenuName);
    }

    /// <summary>Public exposed function to close a menu using it's name</summary>
    /// <param name="MenuName"The name of the menu</param>
    public static void CloseMenu(string MenuName)
    {
        _singleton.CloseMenu_Internal(MenuName);
    }

    /// <summary>Public exposed function to close the latest menu (used mostly for back buttons)</summary>
    public static void CloseLatestMenu()
    {
        _singleton.CloseLastOpenedMenu_Internal();
    }

    /// <summary>Public exposed function to start a server</summary>
    public static void CloseAllMenus()
    {
        _singleton.CloseAllMenus_Internal();
    }

    /// <summary>Public exposed function to display the main menu</summary>
    public static void DisplayMainMenu()
    {
        Singleton.MainMenuParent.gameObject.SetActive(true);
        OpenMenu("MainMenu");
        UI_NavigationBar.Open();
        UI_Background.Open();
    }

    /// <summary>Public exposed function to hide the main menu</summary>
    public static void HideMainMenu()
    {
        Singleton.MainMenuParent.gameObject.SetActive(false);
        Singleton.CloseAllMenus_Internal();
        CloseMenu("MainMenu");
        UI_NavigationBar.Close();
        UI_Background.Close();
    }

    public static void ConnectToGame()
    {
        NetworkManager.Singleton.Connect();
    }
}
