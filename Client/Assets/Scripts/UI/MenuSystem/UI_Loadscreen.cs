using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Loadscreen : MonoBehaviour
{
    private static UI_Loadscreen instance;

    public Text StatusText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        Close();
    }

    /// <summary>Opens the UI Loadscreen</summary>
    public static void Open()
    {
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
    }

    /// <summary>Closes the UI Loadscreen</summary>
    public static void Close()
    {
        instance.gameObject.SetActive(false);
    }


    /// <summary>Set the status text on the loadingscreen</summary>
    public static void SetStatusText(string text)
    {
        instance.StatusText.text = text;
    }
}