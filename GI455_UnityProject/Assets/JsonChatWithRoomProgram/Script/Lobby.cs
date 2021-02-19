/*using System.Collections.Generic;
using UnityEngine;

namespace JsonChatProgram
{
    public class Lobby : MonoBehaviour
    {
        public enum State
    {
        Lobby,
        CreateRoom,
        JoinRoom,
        InRoom
    }

    private State state;
    private List<Room> roomList = new List<Room>();

    private string roomName;

    public void Start()
    {
        LobbyManager.instance.OnCreateRoom += OnCreateRoom;
        LobbyManager.instance.OnJoinRoom += OnJoinRoom;
        LobbyManager.instance.OnLeaveRoom += OnLeaveRoom;

        //Create room for example
        LobbyManager.instance.CreateRoom("Public Room");

        state = State.Lobby;
    }

    public void OnGUI()
    {
        if (LobbyManager.instance == null)
            return;

        if(state == State.Lobby)
        {
            if(GUILayout.Button("CreateRoom"))
            {
                state = State.CreateRoom;
            }

            if(GUILayout.Button("JoinRoom"))
            {
                state = State.JoinRoom;
            }

            if(GUILayout.Button("GetRoomList"))
            {
                roomList = LobbyManager.instance.GetRoomList();
            }

            if(GUILayout.Button("ClearRoomList"))
            {
                roomList.Clear();
            }

            GUILayout.TextArea("==================");

            for(int i = 0; i < roomList.Count; i++)
            {
                GUILayout.TextArea(roomList[i].RoomName);
            }
        }
        else if(state == State.CreateRoom)
        {
            roomName = GUILayout.TextField(roomName);

            if(GUILayout.Button("CreateRoom"))
            {
                LobbyManager.instance.CreateRoom(roomName);
            }
        }
        else if(state == State.JoinRoom)
        {
            roomName = GUILayout.TextField(roomName);

            if(GUILayout.Button("JoinRoom"))
            {
                LobbyManager.instance.JoinRoom(roomName);
            }
        }
        else if(state == State.InRoom)
        {
            GUILayout.TextArea("Room : [" + LobbyManager.instance.CurrentRoom.RoomName + "]");

            if(GUILayout.Button("LeaveRoom"))
            {
                LobbyManager.instance.LeaveRoom();
            }
        }
    }

    public void OnCreateRoom(LobbyManager.MessageCallback result)
    {
        if(result.status)
        {
            state = State.InRoom;
        }
        else
        {
            Debug.Log(result.message);
        }
    }

    public void OnJoinRoom(LobbyManager.MessageCallback result)
    {
        if(result.status)
        {
            state = State.InRoom;
        }
        else
        {
            Debug.Log(result.message);
        }
    }

    public void OnLeaveRoom(LobbyManager.MessageCallback result)
    {
        if(result.status)
        {
            state = State.Lobby;
        }
        else
        {
            Debug.Log(result.message);
        }
    }
    }
}*/