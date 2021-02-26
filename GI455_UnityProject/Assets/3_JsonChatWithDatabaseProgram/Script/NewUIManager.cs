using System;
using System.Collections;
using System.Collections.Generic;
using JsonChatProgram;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace JsonChatWithDatabase
{
    public class NewUIManager : MonoBehaviour
    {
        public enum UIAppState
        {
            wait,
            Login,
            Lobby,
            Room
        }

        [Header("Core Class")]
        public ChatWebsocketServerManager CWSM;
        public RoomManageer RM;
        
        [Header("Main Panel")]
        public GameObject LoginPanel;
        public GameObject LobbyPanel;
        public GameObject RoomPanel;
        
        [Header("Sub Panel")]
        public GameObject CreateRoomPanel;
        public GameObject RegisterPanel;
        public GameObject Popup_1;

        [Header("Input Field")]
        public InputField InputUserIDField;
        public InputField InputUserPasswordField;
        public InputField InputMessageField;
        public InputField InputRoomNameField;
        
        [Header("Register Input Field")]
        public InputField InputRegisterNameField;
        public InputField InputRegisterUserIDField;
        public InputField InputRegisterPasswordField;
        public InputField InputRegisterRePasswordField;

        [Header("Dropdown")]
        public Dropdown RoomListDropdown;

        [Header("UI Text")]
        public Text TextUILeft;
        public Text TextUIRight;
        public Text RoomListDropdown_Label;
        public Text TextPopupMessage;
        public Text TextTitleRoom;
        public Text ServerStatusText;

        [SerializeField] public UIAppState currentState { get; private set; }
        [SerializeField] public UIAppState tempCurrentState;
        private bool tempServerStatus;

        private void Start()
        {
            tempCurrentState = currentState;
            tempServerStatus = CWSM.isConnectToServer;
        }

        private void Update()
        {
            switch (currentState)
            {
                case UIAppState.Login:
                    LoginPanelSetVisibility(true);
                    tempCurrentState = currentState;
                    break;
                case UIAppState.Lobby:
                    LobbyPanelSetVisibility(true);
                    tempCurrentState = currentState;
                    break;
                case UIAppState.Room:
                    RoomPanelSetVisibility(true);
                    tempCurrentState = currentState;
                    break;
            }

            if (CWSM.isConnectToServer != tempServerStatus)
            {
                if (CWSM.isConnectToServer == false)
                {
                    ServerStatusText.text = "<color=red>Disconnected.</color>";
                }
                else
                {
                    ServerStatusText.text = "<color=green>Connected.</color>";
                }
            }
        }

        public void SetUIState(UIAppState state)
        {
            this.currentState = state;
        }
        
        // Main Panel
        public void LoginPanelSetVisibility(bool set)
        {
            if (set) // true
            {
                //Show Login Panel
                LoginPanel.SetActive(set);
                LobbyPanel.SetActive(false);
                RoomPanel.SetActive(false);
            }
            else
            {
                LoginPanel.SetActive(false);
            }
        }
        
        public void LobbyPanelSetVisibility(bool set)
        {
            if (set) // true
            {
                //Show Lobby Panel
                LoginPanel.SetActive(false);
                LobbyPanel.SetActive(set);
                RoomPanel.SetActive(false);

                if (RM.roomList == null)
                {
                    CWSM.RequestAnyExistRoom();
                }
                
                //UpdateRoomDropdown();
            }
            else
            {
                LobbyPanel.SetActive(false);
            }
        }
        
        public void RoomPanelSetVisibility(bool set)
        {
            if (set) // true
            {
                //Show Room Panel
                LoginPanel.SetActive(false);
                LobbyPanel.SetActive(false);
                RoomPanel.SetActive(set);
                TextTitleRoom.text = RM.CurrentRoom;
            }
            else
            {
                TextTitleRoom.text = "";
                RoomPanel.SetActive(false);
            }
        }
        
        // Sub Panel
        public void CreteRoomPanelVisibility()
        {
            if (CreateRoomPanel.activeSelf)
            {
                InputRoomNameField.text = null;
                CreateRoomPanel.SetActive(false);
            }
            else
            {
                CreateRoomPanel.SetActive(true);
            }
        }

        public void RegisterPanelVisibility()
        {
            if (RegisterPanel.activeSelf)
            {
                InputRegisterNameField.text = null;
                InputRegisterUserIDField.text = null;
                InputRegisterPasswordField.text = null;
                RegisterPanel.SetActive(false);
            }
            else
            {
                RegisterPanel.SetActive(true);
            }
        }
        
        // Popup
        public void PopupMessage(string message)
        {
            Popup_1.SetActive(true);
            TextPopupMessage.text = message;
        }

        public void ClearPopupMessage()
        {
            Popup_1.SetActive(false);
            TextPopupMessage.text = "";
        }
        
        // Dropdown
        public void UpdateRoomDropdown()
        {
            PopulateDropdown(RM.roomList);
        }

        public void PopulateDropdown(List<Room> optionsList)
        {

            List<string> roomListOptions = new List<string>();
            if (RM.roomList != null)
            {
                foreach (Room room in optionsList)
                {
                    roomListOptions.Add(room.RoomName);
                }
            }

            RoomListDropdown.ClearOptions();
            RoomListDropdown.AddOptions(roomListOptions);
        }
        
        public string DropDownSelectRoom()
        {
            var roomName = "";
            roomName = RoomListDropdown_Label.text;
            Debug.Log($"Select Dropdown : {roomName}");
            return roomName;
        }

        // UI Button
        public void CreateClick()
        {
            var a = InputRoomNameField.text.IsNullOrEmpty();
            if (!a)
            {
                CWSM.RequestCreateRoom();
                CreteRoomPanelVisibility();
            }
            else
            {
                PopupMessage("Please enter \'Room Name Field\'");
            }
        }

        public void LoginClick()
        {
            var a = InputUserIDField.text.IsNullOrEmpty();
            var b = InputUserPasswordField.text.IsNullOrEmpty();
            
            if (!a && !b)
            {
                CWSM.RequestLogin();
            }
            else
            {
                var message = "\nPlease enter information Field\nAs\n<color=red>";
                if (a)
                {
                    message += "User ID";
                }

                if (b)
                {
                    message += "Password";
                }

                message += "</color>";
                
                PopupMessage(message);

                message = "";
            }
        }
        
        public void LeaveRoomClick()
        {
            if (RM.CurrentRoom != null)
            {
                CWSM.RequestLeaveRoom();
            }
        }

        public void RegisterClick()
        {
            var a = InputRegisterNameField.text.IsNullOrEmpty();
            var b = InputRegisterUserIDField.text.IsNullOrEmpty();
            var c = InputRegisterPasswordField.text.IsNullOrEmpty();
            var d = InputRegisterRePasswordField.text.IsNullOrEmpty();
            var pass = InputRegisterPasswordField.text;
            var re_pass = InputRegisterRePasswordField.text;
            Debug.Log($"{pass}, {re_pass}");
            
            if (!a && !b && !c && !d && pass == re_pass)
            {
                CWSM.RequestRegister();
            }
            else
            {
                var message = "\nPlease fill the information input field before submit.\nAs\n<color=red>";
                
                if (a)
                {
                    message += "Name\n";
                }

                if (b)
                {
                    message += "UserID\n";
                }
                
                if (c)
                {
                    message += "Password\n";
                }
                
                if (d)
                {
                    message += "Re-Password\n";
                }

                if (re_pass != pass)
                {
                    message += "\'Re-Password\' is not the same as <b>\'Password\'</b>\n";
                }

                message += "</color>";
                
                PopupMessage(message);

                message = "";
            }

        }
    }
}