using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;
using System.Linq;
using UnityEngine.UI;

namespace JsonChatProgram
{
    public class WebsocketChatServer : MonoBehaviour
    {
        struct MessageData
        {
            public string Username;
            public string Message;
            public string CurrentRoom;

            public MessageData(string username, string message,string currentRoom)
            {
                Username = username;
                Message = message;
                CurrentRoom = currentRoom;
            }
        }

        public struct SocketEvent
        {
            public string EventName;
            public string Data;
            public string Output;

            public SocketEvent(string eventName, string data, string output)
            {
                EventName = eventName;
                Data = data;
                Output = output;
            }
        }
        
        public List<Room> roomList = new List<Room>();
        
        public string CurrentRoom { get; private set; }

        public WebSocket ws;
        private string tempData;
        private string ipaddress;
        private string port;
        private string Username;
        private bool isConnectToServer;
        public NewUIManager uim;

        // Unity Behavior
        void Start()
        {
            isConnectToServer = false;
            uim.ConnectionPanel.SetActive(true);
            uim.LobbyPanel.SetActive(false);
            uim.MessagerPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (!isConnectToServer)
                {
                    Connect();
                }
                else if(uim.MessagerPanel.activeSelf)
                {
                    SendMessageToServer();
                }
                else if (uim.LobbyPanel.activeSelf)
                {
                    RequestJoinRoom();
                }
                else if (uim.LobbyPanel.activeSelf && uim.CreateRoomPanel.activeSelf)
                {
                    RequestCreateRoom();
                }
                
            }
            
            if (!tempData.IsNullOrEmpty())
            {
                var JSED = JsonUtility.FromJson<SocketEvent>(tempData); // JSED = Json SocketEventData
                var JMD = JsonUtility.FromJson<MessageData>(tempData); // JMD = Json MessageData
                
                if (JSED.EventName != null)
                {
                    //Debug.Log($"--------JSED data format => {JSED.EventName} : {JSED.Data} : {JSED.Output}");
                    switch (JSED.EventName)
                    {
                        case "RequestAnyExistRoom":
                            if (JSED.Output == "success")
                            {
                                UpdateRoomList(JSED);
                                tempData = null;
                            }
                            else
                            {
                                Debug.Log($"Request processed result : {JSED.Output} there are no room on server.");
                                //RequestCreateRoom();
                            }
                            break;
                        case "CreateRoom":
                            if (JSED.Output == "fail")
                            {
                                uim.PopupMessage($"\"Create Room\" fail.\nThere are already <color=#d1432a>{JSED.Data}</color> please try another name.");
                                Debug.Log($"Request processed result : {JSED.Output}.");
                            }
                            else
                            {
                                Room newRoom = new Room(JSED.Data);
                                roomList.Add(newRoom);
                                uim.UpdateRoomDropdown();
                                Debug.Log($"Room was created. Room name is {JSED.Data}.");
                            }
                            break;
                        case "JoinRoom":
                            if (JSED.Output == "fail_1")
                            {
                                uim.PopupMessage("\"Join Room\" fail. The room does not exits. and you still in other room. \n<size=12>(the system will automatic leave for you.)</size>");
                                Debug.Log($"Request processed result : {JSED.Output}.");
                            }
                            else if (JSED.Output == "fail_2")
                            {
                                uim.PopupMessage("\"Join Room\" fail. The room does not exits.");
                                Debug.Log($"Request processed result : {JSED.Output}.");
                            }
                            else if (JSED.Output == "fail_3")
                            {
                                uim.PopupMessage("\"Join Room\" fail. Found the room but you still in other room.");
                                Debug.Log($"Request processed result : {JSED.Output}.");
                            }
                            else
                            {
                                this.CurrentRoom = JSED.Data;
                                JoinRoom();
                                Debug.Log($"Join to {JSED.Data} room.");
                            }
                            break;
                        case "LeaveRoom":
                            if (JSED.Output == "fail")
                            {
                                uim.PopupMessage("\"Leave Room\" fail");
                                Debug.Log($"Request processed result : {JSED.Output}");
                            }
                            else
                            {
                                this.CurrentRoom = "";
                                uim.TextTitleRoom.text = "";
                                Debug.Log($" from {JSED.Data}");
                            }
                            break;
                    }
                }

                if (JMD.Username != null)
                {
                    Debug.Log($"--------JMD data format => {JMD.Username} : {JMD.Message} : {JMD.CurrentRoom}");
                    
                    if (JMD.Username == Username)
                    {
                        uim.TextUILeft.text += "\n";
                        uim.TextUIRight.text += "<b><color=#ffd900>" + JMD.Username + "</color></b> : " + JMD.Message + "\n";
                    }
                    else
                    {
                        uim.TextUIRight.text += "\n";
                        uim.TextUILeft.text += "<b><color=#00aeff>" + JMD.Username + "</color></b> : " + JMD.Message + "\n";
                    }
                }

            }
            
            tempData = null;
        }
        
        // Websocket method
        public void Connect()
        {
            if (!uim.InputUserField.text.IsNullOrEmpty()) //Require Username
            {
                if (ipaddress.IsNullOrEmpty() || port.IsNullOrEmpty())
                {
                    ipaddress = "127.0.0.1";
                    port = "8888";
                }
                else
                {
                    ipaddress = uim.InputIPAddress.text;
                    port = uim.InputPort.text;
                }
            
                string url = $"ws://{ipaddress}:{port}/";
                this.Username = uim.InputUserField.text;
            
                ws = new WebSocket(url);
                ws.OnMessage += OnMessage;
            
                ws.Connect();
            
                RequestAnyExistRoom();
            
                uim.ConnectionPanel.SetActive(false);
                uim.LobbyPanel.SetActive(true);
                uim.MessagerPanel.SetActive(false);
                
                uim.UpdateRoomDropdown();
                
                isConnectToServer = true;
            }
        }

        public void LeaveRoom()
        {
            if (CurrentRoom != null)
            {
                uim.ConnectionPanel.SetActive(false);
                uim.LobbyPanel.SetActive(true);
                uim.MessagerPanel.SetActive(false);
                
                CurrentRoom = "";
                
                RequestLeaveRoom();
            }
            
        }
        
        public void Disconnect()
        {
            if (ws != null)
            {
                uim.ConnectionPanel.SetActive(true);
                uim.LobbyPanel.SetActive(false);
                uim.MessagerPanel.SetActive(false);

                Username = "";
                CurrentRoom = "";
                
                ws.Close();
            }
        }

        public void QuitApp()
        {
            Application.Quit();
        }

        public void JoinRoom()
        {
            uim.TextTitleRoom.text = CurrentRoom;
            uim.TextUILeft.text = "";
            uim.TextUIRight.text = "";
            
            uim.LobbyPanel.SetActive(false);
            uim.CreateRoomPanel.SetActive(false);
            uim.MessagerPanel.SetActive(true);
        }

        private void OnDestroy()
        {
            ws.Close();
        }

        public void RequestCreateRoom()
        {
            /*if (roomList.Count < 1) //For first time
            {
                uim.InputRoomNameField.text = "Main Room";
            }
            */
            var roomName = uim.InputRoomNameField.text;
            
            if (ws.ReadyState == WebSocketState.Open)
            {
                SocketEvent socketEvent = new SocketEvent("CreateRoom", roomName,"");
                string jsonStr = JsonUtility.ToJson(socketEvent);
                ws.Send(jsonStr);
            }
        }

        public void RequestJoinRoom()
        {
            Debug.Log("Join Room Request Clicked");
            
            var roomName = uim.SelectRoom();
            if (ws.ReadyState == WebSocketState.Open)
            {
                SocketEvent socketEvent = new SocketEvent("JoinRoom", roomName,Username);
                string jsonStr = JsonUtility.ToJson(socketEvent);
                ws.Send(jsonStr);
            }
        }

        public void RequestAnyExistRoom()
        {
            if (ws.ReadyState == WebSocketState.Open)
            {
                SocketEvent socketEvent = new SocketEvent("RequestAnyExistRoom", "","");
                string jsonStr = JsonUtility.ToJson(socketEvent);
                ws.Send(jsonStr);
                Debug.Log("Send Request Exist Room");
            }
        }

        public void RequestLeaveRoom()
        {
            var roomName = this.CurrentRoom;

            if (ws.ReadyState == WebSocketState.Open)
            {
                SocketEvent socketEvent = new SocketEvent("LeaveRoom", roomName, "");
                string jsonStr = JsonUtility.ToJson(socketEvent);
                ws.Send(jsonStr);
            }
        }
        
        public void SendMessageToServer()
        {
            if (uim.InputUserField.text == "" || ws.ReadyState != WebSocketState.Open)
                return;

            MessageData newMessageData = new MessageData();
            newMessageData.Username = this.Username;
            newMessageData.Message = uim.InputMessageField.text;
            newMessageData.CurrentRoom = this.CurrentRoom;

            string toJsonStr = JsonUtility.ToJson(newMessageData);
            
            ws.Send(toJsonStr);
            uim.InputMessageField.text = "";
        }

        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            tempData = messageEventArgs.Data;
        }

        public void UpdateRoomList(SocketEvent eventData)
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
            
            uim.UpdateRoomDropdown();
        }
    }
}

