using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageDialog : MonoBehaviour
{
    private static MessageDialog instance;

    public Button DialogCancelButton;
    public Button DialogConfirmButton;
    public Text DialogNameText;
    public Text DialogMessageText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        Internal_Close();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            DialogConfirmButton.onClick.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(DialogCancelButton.isActiveAndEnabled)
            {
                DialogCancelButton.onClick.Invoke();
            }
        }
    }

    /// <summary>Interal function to open the MessageDialog</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DialogMessage">The message to be displayed inside the dialog</param>
    /// <param name="OnConfirm">The event which is triggered when cancelling this dialog</param>
    /// <param name="OnCancel">The event which is triggered when cancelling this dialog</param>
    /// <param name="ConfirmButtonText">The text on the confirm button</param>
    public void Open_Internal(string DialogTitleText, string DialogMessage, Action OnCancel, Action OnConfirm, string ConfirmButtonText)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        DialogNameText.text = DialogTitleText;

        DialogMessageText.text = DialogMessage;


        if (ConfirmButtonText != null && ConfirmButtonText != "")
        {
            DialogConfirmButton.GetComponentInChildren<Text>().text = ConfirmButtonText;
        }

        if (OnConfirm != null)
        {
            DialogConfirmButton.gameObject.SetActive(true);
            DialogConfirmButton.onClick.AddListener(() => {
                Internal_Close();
                OnConfirm();
            });
        } else
        {
            DialogConfirmButton.gameObject.SetActive(false);
        }

            

        if(OnCancel != null)
        {
            DialogCancelButton.gameObject.SetActive(true);
            DialogCancelButton.onClick.AddListener(() => {
                Internal_Close();
                OnCancel();
            });
        } else
        {
            DialogCancelButton.gameObject.SetActive(false);
        }
    }

    public void Internal_Close()
    {
        gameObject.SetActive(false);
    }

    /// <summary>Exposed function to open the MessageDialog</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DialogMessage">The message to be displayed inside the dialog</param>
    /// <param name="OnConfirm">The event which is triggered when cancelling this dialog</param>
    /// <param name="OnCancel">The event which is triggered when cancelling this dialog</param>
    /// <param name="ConfirmButtonText">The text on the confirm button</param>
    public static void Open(string DialogTitleText, string DialogMessage, Action OnCancel, Action OnConfirm, string ConfirmButtonText)
    {
        instance.Open_Internal(DialogTitleText, DialogMessage, OnCancel, OnConfirm, ConfirmButtonText);
    }


    /// <summary>Exposed function to open the MessageDialog without a modified confirm button</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DialogMessage">The message to be displayed inside the dialog</param>
    /// <param name="OnConfirm">The event which is triggered when cancelling this dialog</param>
    /// <param name="OnCancel">The event which is triggered when cancelling this dialog</param>
    public static void Open(string DialogTitleText, string DialogMessage, Action OnCancel, Action OnConfirm)
    {
        instance.Open_Internal(DialogTitleText, DialogMessage, OnCancel, OnConfirm, null);
    }

    /// <summary>Exposed function to open the MessageDialog with only a confirm option</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DialogMessage">The message to be displayed inside the dialog</param>
    /// <param name="OnConfirm">The event which is triggered when cancelling this dialog</param>
    /// <param name="ConfirmButtonText">The text on the confirm button</param>
    public static void Open(string DialogTitleText, string DialogMessage, string ConfirmButtonText, Action OnConfirm)
    {
        instance.Open_Internal(DialogTitleText, DialogMessage, null, OnConfirm, ConfirmButtonText);
    }

    /// <summary>Exposed function to open the MessageDialog with only text</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DialogMessage">The message to be displayed inside the dialog</param>
    public static void Open(string DialogTitleText, string DialogMessage)
    {
        instance.Open_Internal(DialogTitleText, DialogMessage, null, null, null);
    }

    /// <summary>Exposed function to close the MessageDialog</summary>
    public static void Close()
    {
        instance.Internal_Close();
    }
}
