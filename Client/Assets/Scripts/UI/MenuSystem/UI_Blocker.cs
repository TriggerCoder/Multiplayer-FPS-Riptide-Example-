using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Blocker : MonoBehaviour
{
    private static UI_Blocker instance;

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

    /// <summary>Opens the UI Blocker</summary>
    public static void Open()
    {
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
    }

    /// <summary>Closes the UI Blocker</summary>
    public static void Close()
    {
        instance.gameObject.SetActive(false);
    }

}