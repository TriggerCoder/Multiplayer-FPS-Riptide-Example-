using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputfieldDialog : MonoBehaviour
{
    private static InputfieldDialog instance;

    public Button DialogCancelButton;
    public Button DialogConfirmButton;
    public Text DialogNameText;
    public InputField DialogTextInputField;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        Close();
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

    /// <summary>Interal function to open the InputfieldDialog</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DialogInputString">The default string to be displayed inside the inputfield of the dialog</param>
    /// <param name="ValidCharacters">Determines the valid characters which can be used. Leave empty to ignore filtering</param>
    /// <param name="CharacterLimit">Sets the amount of characters which can be used in the inputfield</param>
    /// <param name="MinCharacterAmount">Sets the amount of characters which can be used in the inputfield</param>
    /// <param name="OnConfirm">The event which is triggered when cancelling this dialog</param>
    /// <param name="OnCancel">The event which is triggered when cancelling this dialog</param>
    public void Open_Internal(string DialogTitleText, string DialogInputString, string ValidCharacters, int CharacterLimit, int MinCharacterAmount, Action OnCancel, Action<string> OnConfirm, string ConfirmButtonText)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        DialogNameText.text = DialogTitleText;

        DialogTextInputField.characterLimit = CharacterLimit;
        if(ValidCharacters != null && ValidCharacters != "")
        {
            DialogTextInputField.onValidateInput = (string text, int charIndex, char addedChar) => {
                return ValidateChar(ValidCharacters, addedChar);
            };
        }

        DialogTextInputField.placeholder.GetComponent<Text>().text = DialogInputString;
        DialogTextInputField.Select();

        if(ConfirmButtonText != null && ConfirmButtonText != "")
        {
            DialogConfirmButton.GetComponentInChildren<Text>().text = ConfirmButtonText;
        }

        DialogConfirmButton.onClick.AddListener(() => {
            if (DialogTextInputField.text.Length >= MinCharacterAmount)
            {
                Close();
                OnConfirm(DialogTextInputField.text);
            } else
            {
                Debug.Log("The input given is too short!");
            }
        });

        if(OnCancel != null)
        {
            DialogCancelButton.gameObject.SetActive(true);
            DialogCancelButton.onClick.AddListener(() => {
                Close();
                OnCancel();
            });
        } else
        {
            DialogCancelButton.gameObject.SetActive(false);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private char ValidateChar(string validCharacters, char addedChar)
    {
        if (validCharacters.IndexOf(addedChar) != -1)
        {
            // Valid
            return addedChar;
        }
        else
        {
            // Invalid
            return '\0';
        }
    }

    /// <summary>Exposed function to open the InputfieldDialog</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DialogInputString">The default string to be displayed inside the inputfield of the dialog</param>
    /// <param name="CharacterLimit">Sets the amount of characters which can be used in the inputfield</param>
    /// <param name="MinCharacterAmount">Sets the amount of characters neccessary to continue</param>
    /// <param name="OnConfirm">The event which is triggered when cancelling this dialog</param>
    /// <param name="OnCancel">The event which is triggered when cancelling this dialog</param>
    /// <param name="ConfirmButtonText">The text on the confirm button</param>
    public static void Open(string DialogTitleText, string DialogInputString, string ValidCharacters, int CharacterLimit, int MinCharacterAmount, string ConfirmButtonText, Action OnCancel, Action<string> OnConfirm)
    {
        instance.Open_Internal(DialogTitleText, DialogInputString, ValidCharacters, CharacterLimit, MinCharacterAmount, OnCancel, OnConfirm, ConfirmButtonText);
    }


    /// <summary>Exposed function to open the InputfieldDialog without a modified confirm button</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DialogInputString">The default string to be displayed inside the inputfield of the dialog</param>
    /// <param name="CharacterLimit">Sets the amount of characters which can be used in the inputfield</param>
    /// <param name="MinCharacterAmount">Sets the amount of characters neccessary to continue</param>
    /// <param name="OnConfirm">The event which is triggered when cancelling this dialog</param>
    /// <param name="OnCancel">The event which is triggered when cancelling this dialog</param>
    public static void Open(string DialogTitleText, string DialogInputString, string ValidCharacters, int CharacterLimit, int MinCharacterAmount, Action OnCancel, Action<string> OnConfirm)
    {
        instance.Open_Internal(DialogTitleText, DialogInputString, ValidCharacters, CharacterLimit, MinCharacterAmount, OnCancel, OnConfirm, null);
    }

    /// <summary>Exposed function to open the InputfieldDialog with only number support</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DefaultInt">The default number to be displayed inside the inputfield of the dialog</param>
    /// <param name="MinCharacterAmount">Sets the amount of characters neccessary to continue</param>
    /// <param name="OnConfirm">The event which is triggered when cancelling this dialog</param>
    /// <param name="OnCancel">The event which is triggered when cancelling this dialog</param>
    /// <param name="ConfirmButtonText">The text on the confirm button</param>
    public static void Open(string DialogTitleText, int DefaultInt, int MinCharacterAmount, string ConfirmButtonText, Action OnCancel, Action<int> OnConfirm)
    {
        instance.Open_Internal(DialogTitleText, DefaultInt.ToString(), "0123456789-", 20, MinCharacterAmount, OnCancel,
            (string inputText) =>
            {
                // Try to Parse input string
                if (int.TryParse(inputText, out int _i))
                {
                    OnConfirm(_i);
                }
                else
                {
                    OnConfirm(DefaultInt);
                }
            }
        , ConfirmButtonText);
    }

    /// <summary>Exposed function to open the InputfieldDialog with only number support without a modified confirm button</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DefaultInt">The default number to be displayed inside the inputfield of the dialog</param>
    /// <param name="MinCharacterAmount">Sets the amount of characters neccessary to continue</param>
    /// <param name="OnConfirm">The event which is triggered when cancelling this dialog</param>
    /// <param name="OnCancel">The event which is triggered when cancelling this dialog</param>
    public static void Open(string DialogTitleText, int DefaultInt, int MinCharacterAmount, Action OnCancel, Action<int> OnConfirm)
    {
        instance.Open_Internal(DialogTitleText, DefaultInt.ToString(), "0123456789-", 20, MinCharacterAmount, OnCancel,
            (string inputText) => {
                // Try to Parse input string
                if (int.TryParse(inputText, out int _i))
                {
                    OnConfirm(_i);
                }
                else
                {
                    OnConfirm(DefaultInt);
                }
            }
        , null);
    }

    /// <summary>Exposed function to open the InputfieldDialog with only a confirm option</summary>
    /// <param name="DialogTitleText">The title text object of this dialog</param>
    /// <param name="DialogInputString">The default string to be displayed inside the inputfield of the dialog</param>
    /// <param name="ValidCharacters">Determines the valid characters which can be used. Leave empty to ignore filtering</param>
    /// <param name="CharacterLimit">Sets the amount of characters which can be used in the inputfield</param>
    /// <param name="MinCharacterAmount">Sets the amount of characters neccessary to continue</param>
    /// <param name="OnConfirm">The event which is triggered when cancelling this dialog</param>
    /// <param name="ConfirmButtonText">The text on the confirm button</param>
    public static void Open(string DialogTitleText, string DialogInputString, string ValidCharacters, int CharacterLimit, int MinCharacterAmount, string ConfirmButtonText, Action<string> OnConfirm)
    {
        instance.Open_Internal(DialogTitleText, DialogInputString, ValidCharacters, CharacterLimit, MinCharacterAmount, null, OnConfirm, ConfirmButtonText);
    }
}
