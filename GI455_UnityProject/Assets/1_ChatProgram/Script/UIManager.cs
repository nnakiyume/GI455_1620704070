using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WebSocketSharp;
using Random = UnityEngine.Random;

namespace ChatProgram
{
    public class UIManager : MonoBehaviour
    {
        // UI
        [Header("Input/Output UI")] 
        public GameObject ConnectServerPanel;
        public GameObject ChatPanel;
        public InputField usernameField;
        public InputField ipField;
        public InputField portField;
        public InputField chatMessageField;
        public Text ProgramMessageText;
        public Text ChatLogText;
        public Text ChatAlignLeftText;
        public Text ChatAlignRightText;

        [Header("Theme Mode UI")]
        [SerializeField] private Image ThemeModeButtonBackground;
        [SerializeField] private Image ConnectButtonBackground;
        [SerializeField] private Image CloseButtonBackground;
        [SerializeField] private Image SendButtonBackground;
        [SerializeField] private Image ThemeBackground;
        [SerializeField] private Image ModeIcon;
        [SerializeField] private Sprite lightModeIcon;
        [SerializeField] private Sprite darkModeIcon;

        //--Color parameter
        [Header("Color")] 
        public Color lightGrey = new Color(211, 211, 211, 200.0f);
        public Color dark_1 = new Color(47, 49, 51,255.0f);
        public Color dark_2 = new Color(30, 37, 41,255.0f);
        public Color dark_3 = new Color(23, 29, 32,255.0f);
        public Color lightWhite = new Color(240.0f, 240.0f, 240.0f,255.0f);
        /*public Color warning = new Color(255.0f, 200.0f, 0.0f,255.0f);
        public Color crimson = new Color(220.0f, 20.0f, 60.0f,255.0f);
        public Color mediumseagreen = new Color(60.0f, 179.0f, 113.0f,255.0f);
        public Color steelblue = new Color(70.0f, 130.0f, 180.0f,255.0f);*/
        private Color textColor;
        
        private bool _colorThemeMode = false;
        private WebsocketChatProgram wsChat;
        
        // Behavior
        private void Start()
        {
            textColor = dark_3;
            ProgramMessageText.text = "";
            ipField.onValueChanged.AddListener(delegate { RemoveSpaceInInputField(); });
            portField.onValueChanged.AddListener(delegate { RemoveSpaceInInputField(); });
            
        }
        
        private void Update()
        {
            if (_colorThemeMode == false)
            {
                LightMode();
            }
            else
            {
                DarkMode();
            }
        }

        // UI Management Method
        public void RemoveSpaceInInputField()
        {
            if (ipField.text.IsNullOrEmpty())
            {
                ipField.text = ipField.text.Replace(" ", "");
                portField.text = portField.text.Replace(" ", "");
            }
        }

        public void SendMessageToShow(string message)
        {
            ProgramMessageText.text = $"{message}";
        }

        // Theme Management Method
        public void ThemeModeClick()
        {
            if (!_colorThemeMode) //If mode == false (light)
            {
                _colorThemeMode = true;
            }
            else
            {
                _colorThemeMode = false;
            }
            
            SetTextStyle();
        }

        public void SetTextStyle()
        {
            if (_colorThemeMode == false)
            {
                textColor = dark_3;
                usernameField.placeholder.color = lightGrey;
                ipField.placeholder.color = lightGrey;
                portField.placeholder.color = lightGrey;
                chatMessageField.placeholder.color = lightGrey;
            }
            else
            {
                textColor = lightWhite;
                usernameField.placeholder.color = dark_1;
                ipField.placeholder.color = dark_1;
                portField.placeholder.color = dark_1;
                chatMessageField.placeholder.color = dark_1;
            }

            usernameField.textComponent.color = textColor;
            ipField.textComponent.color = textColor;
            portField.textComponent.color = textColor;
            chatMessageField.textComponent.color = textColor;
            ProgramMessageText.color = textColor;
        }
        
        private void LightMode()
        {
            //Light
            ThemeBackground.color = lightWhite;
            ThemeModeButtonBackground.color = dark_3;
            ModeIcon.color = lightWhite;
            ModeIcon.sprite = lightModeIcon;
            ConnectButtonBackground.color = Color.white;
            CloseButtonBackground.color = Color.white;
            SendButtonBackground.color = Color.white;
            ChatAlignLeftText.color = dark_2;
            ChatAlignRightText.color = dark_2;
            
            //Input Field
            usernameField.image.color = lightWhite;
            ipField.image.color = lightWhite;
            portField.image.color = lightWhite;
            chatMessageField.image.color = lightWhite;
        }

        private void DarkMode()
        {
            //Dark
            ThemeBackground.color = dark_2;
            ThemeModeButtonBackground.color = lightWhite;
            ModeIcon.color = dark_2;
            ModeIcon.sprite = darkModeIcon;
            ConnectButtonBackground.color = dark_3;
            CloseButtonBackground.color = dark_3;
            SendButtonBackground.color = dark_3;
            ChatAlignLeftText.color = lightWhite;
            ChatAlignRightText.color = lightWhite;
            
            //Input Field
            usernameField.image.color = dark_3;
            ipField.image.color = dark_3;
            portField.image.color = dark_3;
            chatMessageField.image.color = dark_3;
        }
    }
}