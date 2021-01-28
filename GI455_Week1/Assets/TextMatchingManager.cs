using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class TextMatchingManager : MonoBehaviour
{
    // Properties
    [SerializeField] private Text TextDataUi;
    [SerializeField] private Text ResultTextUi;
    [SerializeField] private InputField TextInputField;

    private string textInput;
    [SerializeField] private string[] dataText;

    // Behavior
    void Start()
    {
        dataText = new string[] {
            "Unity",
            "Unreal",
            "ResidentEvil",
            "Minecraft",
            "Google",
            "MongoDB",
            "Nutthanon"
        };
        
        UpdateDataTextUI();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            MatchingText(TextInputField.text);
        }
    }

    public void UpdateDataTextUI()
    {
        foreach (var text in dataText)
        {
            TextDataUi.text += text + "\n";
        }
    }

    public void OnFindClick()
    {
        MatchingText(TextInputField.text);
    }
    
    public void MatchingText(string text)
    {
        if (dataText.Contains(text))
        {
            ResultTextUi.text = $"\" <color=#3CB371>{text}</color> \" is found.\n";
        }
        else
        {
            ResultTextUi.text = $"\" <color=#DC143C>{text}</color> \" is not found.\n";
        }
    }
}
