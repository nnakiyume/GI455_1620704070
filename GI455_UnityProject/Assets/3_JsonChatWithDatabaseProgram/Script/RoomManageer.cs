using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace JsonChatWithDatabase
{
    public class RoomManageer : MonoBehaviour
    {
        //Properties
        public ChatWebsocketServerManager CWSM;
        public NewUIManager UIM;
        
        public List<Room> roomList = new List<Room>();
        public string CurrentRoom { get; private set; }

        // Method
        public void SetCurrentRoom(string name)
        {
            CurrentRoom = name;
        }
        
        public void JoinRoom()
        {
            UIM.TextTitleRoom.text = CurrentRoom;
            UIM.TextUILeft.text = "";
            UIM.TextUIRight.text = "";
            
            UIM.SetUIState(NewUIManager.UIAppState.Room);
        }

        public void UpdateRoomList(ChatWebsocketServerManager.SocketEventStruct eventData)
        {
            roomList.Clear(); //Clear
            
            char spliter = '#';
            string[] roomNames = eventData.Data.Split(spliter);
            
            foreach (string room in roomNames)
            {
                if(room == "") continue;
                Room newRoom = new Room(room);
                roomList.Add(newRoom);;
                Debug.Log($"Add {newRoom.RoomName} from server.");
            }
            
            UIM.UpdateRoomDropdown();
        }
        
    }
}