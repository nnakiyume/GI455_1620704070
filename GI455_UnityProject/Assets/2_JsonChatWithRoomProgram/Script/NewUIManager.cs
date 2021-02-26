using System;
using System.Collections;
using System.Collections.Generic;
using JsonChatProgram;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class NewUIManager : MonoBehaviour
{
    public GameObject ConnectionPanel;
    public GameObject MessagerPanel;
    public GameObject LobbyPanel;
    public GameObject CreateRoomPanel;
    public GameObject Popup_1;

    public InputField InputUserField;
    public InputField InputIPAddress;
    public InputField InputPort;
    public InputField InputMessageField;
    public InputField InputRoomNameField;

    public Dropdown RoomListDropdown;

    public Text TextUILeft;
    public Text TextUIRight;
    public Text RoomListDropdown_Label;
    public Text TextPopupMessage;
    public Text TextTitleRoom;
    
    public WebsocketChatServer ws;
    public int currentDropdownValue;
    public string currentDropdownMessage;


    // Method
    public void UpdateRoomDropdown()
    {
        PopulateDropdown(ws.roomList);
    }
    
    public void PopulateDropdown (List<Room> optionsList) {
        
        List<string> roomListOptions = new List<string> ();
        if (ws.roomList != null)
        {
            foreach (Room room in optionsList) {
                roomListOptions.Add(room.RoomName);
            }
        }
        RoomListDropdown.ClearOptions ();
        RoomListDropdown.AddOptions(roomListOptions);
        
        /*currentDropdownValue = RoomListDropdown.value;
        currentDropdownMessage = RoomListDropdown.options[currentDropdownValue].text;
        RoomListDropdown_Label.text = currentDropdownMessage;*/
    }
    
    // UI Button
    public void CreateClick()
    {
        if (InputRoomNameField.text != "" && InputRoomNameField.text != null)
        {
            ws.RequestCreateRoom();
            CreteRoomOpenClose();
        }
    }

    public void CreteRoomOpenClose()
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

    public string SelectRoom()
    {
        var roomName = "";
        //roomName = RoomListDropdown.options[currentDropdownValue].text;
        roomName = RoomListDropdown_Label.text;
        Debug.Log($"Select Dropdown : {roomName}");
        return roomName;
    }

    public void PopupMessage(string message)
    {
        Popup_1.SetActive(true);
        TextPopupMessage.text = message;
    }

    public void ClosePopup()
    {
        Popup_1.SetActive(false);
        TextPopupMessage.text = "";
    }
}
