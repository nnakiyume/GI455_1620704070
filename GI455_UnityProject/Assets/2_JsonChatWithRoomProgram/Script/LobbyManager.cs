/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

namespace JsonChatProgram
{
    public class LobbyManager : MonoBehaviour
    {
        // Properties
        public struct MessageCallback
        {
            public bool status;
            public string message;

            public MessageCallback(bool status, string message)
            {
                this.status = status;
                this.message = message;
            }
        }

        public delegate void LobbyDelegateHandle(MessageCallback result); //Event 

        public event LobbyDelegateHandle OnCreateRoom;
        public event LobbyDelegateHandle OnJoinRoom;
        public event LobbyDelegateHandle OnLeaveRoom;

        public static LobbyManager instance;
        private List<Room> roomList = new List<Room>();
        public Room CurrentRoom { get; private set; }

        // Behavior Method
        private void Awake()
        {
            instance = this;
        }

        public void CreateRoom(string roomName)
        {
            if (isExistRoom(roomName))
            {
                if (OnCreateRoom != null)
                    OnCreateRoom(new MessageCallback(false, "Room name is exist."));

                return;
            }

            Room newRoom = new Room(roomName);
            roomList.Add(newRoom);

            CurrentRoom = newRoom;

            if (OnCreateRoom != null)
                OnCreateRoom(new MessageCallback(true, "Create room success."));
        }

        public void JoinRoom(string roomName)
        {
            if (!isExistRoom(roomName))
            {
                if (OnJoinRoom != null)
                    OnJoinRoom(new MessageCallback(false, "Room name is not exist."));

                return;
            }

            CurrentRoom = GetRoomByName(roomName);

            if (OnJoinRoom != null)
                OnJoinRoom(new MessageCallback(true, "Join room success"));
        }

        public void LeaveRoom()
        {
            if (CurrentRoom == null)
            {
                if (OnLeaveRoom != null)
                    OnLeaveRoom(new MessageCallback(false, "You doesn't join in room"));
            }

            CurrentRoom = null;

            if (OnLeaveRoom != null)
                OnLeaveRoom(new MessageCallback(true, "Leave room success"));
        }

        public List<Room> GetRoomList()
        {
            List<Room> roomList = new List<Room>();
            roomList.AddRange(roomList);
            return roomList;
        }

        private bool isExistRoom(string roomName)
        {
            Room room = GetRoomByName(roomName);
            return room != null;
        }

        private Room GetRoomByName(string roomName)
        {
            for (int i = 0; i < roomList.Count; i++)
            {
                if (roomList[i].RoomName == roomName)
                {
                    return roomList[i];
                }
            }

            return null;
        }
    }
}
*/
