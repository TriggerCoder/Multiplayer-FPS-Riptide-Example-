using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_NavigationBar : MonoBehaviour
{
    private static UI_NavigationBar instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        GetComponent<RectTransform>().offsetMin = new Vector2(0f, GetComponent<RectTransform>().offsetMin.y);

        Close();
    }

    /// <summary>Opens the UI Navigation Bar</summary>
    public static void Open()
    {
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
    }

    /// <summary>Closes the UI Navigation Bar</summary>
    public static void Close()
    {
        instance.gameObject.SetActive(false);
    }
}
