using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;
using System.Linq;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace JsonChatWithDatabase
{
    public class ChatWebsocketServerManager : MonoBehaviour
    {
        

        public struct SocketEventStruct
        {
            public string EventName;
            public string Data;
            public string Data1; // UserData --- Username
            public string Data2; // Password --- Meassage
            public string Data3; // Name --- TargetRoom
            public string Data4; // CurrentRoom (for SQL)
            public string Output;
            
        }
        
        [Header("Core Class")]
        public WebSocket Websocket;
        public NewUIManager UIM;
        public RoomManageer RM;
        public UserData userData;
        
        private string tempData;
        private string ipaddress;
        private string port;
        
        public bool isConnectToServer;
        private bool isLogin;
        
        // Unity Method
        void Start()
        {
            isConnectToServer = false;
            isLogin = false;
            ConnectToServer();
            UIM.SetUIState(NewUIManager.UIAppState.Login);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (UIM.Popup_1.activeSelf)
                {
                    UIM.ClearPopupMessage();
                }
                else if (UIM.LoginPanel.activeSelf)
                {
                    RequestLogin();
                }
                else if (UIM.LobbyPanel.activeSelf && !UIM.CreateRoomPanel.activeSelf)
                {
                    RequestJoinRoom();
                }
                else if(UIM.RoomPanel.activeSelf)
                {
                    RequestSendMessage();
                }
                else if (UIM.LobbyPanel.activeSelf && UIM.CreateRoomPanel.activeSelf)
                {
                    RequestCreateRoom();
                }
                
            }
            
            if (!tempData.IsNullOrEmpty())
            {
                var jsed = JsonUtility.FromJson<SocketEventStruct>(tempData); // jsed = Json Socket Event Data

                if (jsed.EventName != null)
                {
                    switch (jsed.EventName)
                    {
                        case "RequestAnyExistRoom":
                            if (jsed.Output == "fail")
                            {
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output} there are no room on server.");
                            }
                            else if(jsed.Output == "success")
                            {
                                Debug.Log("RequestAnyExistRoom request success. -> will update room list.");
                                RM.UpdateRoomList(jsed); 
                                tempData = null;
                            }
                            break;
                        
                        case "CreateRoom":
                            if (jsed.Output == "fail")
                            {
                                UIM.PopupMessage($"\"Create Room\" fail.\nThere are already <color=#d1432a>{jsed.Data}</color> please try another name.");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}");
                            }
                            else if(jsed.Output == "success")
                            {
                                Room newRoom = new Room(jsed.Data);
                                RM.roomList.Add(newRoom);
                                UIM.UpdateRoomDropdown();
                                Debug.Log($"Room was created. Room name is {jsed.Data}.");
                            }
                            break;
                        
                        case "JoinRoom":
                            if (jsed.Output == "fail_1")
                            {
                                UIM.PopupMessage("\"Join Room\" fail. The room does not exits. and you still in other room. \n<size=12>(the system will automatic leave for you.)</size>");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}");
                            }
                            else if (jsed.Output == "fail_2")
                            {
                                UIM.PopupMessage("\"Join Room\" fail. The room does not exits.");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}");
                            }
                            else if (jsed.Output == "fail_3")
                            {
                                UIM.PopupMessage("\"Join Room\" fail. Found the room but you still in other room.");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}");
                            }
                            else
                            {
                                RM.SetCurrentRoom(jsed.Data);
                                userData.CurrentRoom = RM.CurrentRoom;
                                RM.JoinRoom();
                                Debug.Log($"<color=green><b>\"{jsed.Data1} or {jsed.Data3}\"</b></color> Join to {jsed.Data} room.");
                            }

                            break;
                        
                        case "LeaveRoom":
                            if (jsed.Output == "fail")
                            {
                                UIM.PopupMessage("\"Leave Room\" fail");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}");
                            }
                            else
                            {
                                RM.SetCurrentRoom(null);
                                UIM.TextTitleRoom.text = "";
                                UIM.SetUIState(NewUIManager.UIAppState.Lobby);
                                Debug.Log($"{userData.Name} leave {jsed.Data} room.");
                            }
                            break;
                        
                        case "SendMessage":
                            if (jsed.Output == "fail")
                            {
                                UIM.PopupMessage("\"Send Message\" fail");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}");
                            }
                            else if(jsed.Output == "success")
                            {
                                if (jsed.Data1 != null)
                                {
                                    if (jsed.Data4 != RM.CurrentRoom)
                                    {
                                        Debug.Log("Message come wrong room!!");
                                    }
                                    else
                                    {
                                        if (jsed.Data1 == userData.UserID)
                                        {
                                            UIM.TextUILeft.text += "\n";
                                            UIM.TextUIRight.text += "<b><color=#ffd900>" + jsed.Data3 + "</color></b> : " + jsed.Data + "\n";
                                        }
                                        else
                                        {
                                            UIM.TextUIRight.text += "\n";
                                            UIM.TextUILeft.text += "<b><color=#00aeff>" + jsed.Data3 + "</color></b> : " + jsed.Data + "\n";
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.Log("jsed.Data_1 is null");
                                }
                            }
                            break;
                    
                        case "RequestRegister":
                            if (jsed.Output == "fail")
                            {
                                UIM.PopupMessage($"\"Request Register\" fail.\nThere are already <color=#d1432a>{jsed.Data}</color> please try another name.");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}.");
                            }
                            else
                            {
                                UIM.PopupMessage($"\"Request Register\" success.\nYour UserID is <color=#2ad16d>{jsed.Data}</color>.");
                                tempData = null;
                            }
                            break;
                        
                        case "RequestLogin":
                            if (jsed.Output == "fail_1") // Dont found UserID on Database
                            {
                                isLogin = false;
                                UIM.PopupMessage($"\"Login\" fail.\nDo not found <color=#d1432a>{jsed.Data}</color> on database.");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}. Dont found UserID on Database.");
                            }
                            else if (jsed.Output == "fail_2") // Password doesn't match on Database
                            {
                                isLogin = false;
                                UIM.PopupMessage($"\"Login\" fail.\n<color=#d1432a>{jsed.Data}</color> is not your password.");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}. Dont found UserID on Database.");
                            }
                            else if(jsed.Output == "success")
                            {
                                isLogin = true;
                                UIM.PopupMessage("\"Login\" success.");
                                
                                userData.UserID = jsed.Data1;
                                userData.Password = jsed.Data2;
                                userData.Name = jsed.Data3;
                                RM.SetCurrentRoom(null);
                                userData.CurrentRoom = RM.CurrentRoom;
                                
                                UIM.SetUIState(NewUIManager.UIAppState.Lobby);
                                
                                tempData = null;
                            }
                            break;
                        
                        case "RequestLogout":
                            if (jsed.Output == "fail")
                            {
                                UIM.PopupMessage($"\"Logout\" fail.\n<color=#d1432a>{jsed.Data}</color>");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}.");
                            }
                            else if (jsed.Output == "success")
                            {
                                isLogin = false;
                                UIM.SetUIState(NewUIManager.UIAppState.Login);
                                UIM.PopupMessage($"\"Logout\" Success.\n<color=#2ad170>{jsed.Data}</color>");
                                Debug.Log($"Request processed result : {jsed.EventName} is {jsed.Output}.");
                            }
                            break;
                    }
                }
            }

            tempData = null;
        }

        // Websocket method
        public void ConnectToServer()
        {
            if (isConnectToServer == false)
            {
                ipaddress = "127.0.0.1";
                port = "8888";

                string url = $"ws://{ipaddress}:{port}/";

                Websocket = new WebSocket(url);
                Websocket.OnMessage += OnMessage;

                Websocket.Connect();

                UIM.SetUIState(NewUIManager.UIAppState.Login);

                isConnectToServer = true;
            }
            else
            {
                Disconnect();
            }
        }

        // Send Request Method
        public void RequestLogin()
        {
            if (!isLogin) // If not isLogin
            {
                SocketEventStruct newEvent = new SocketEventStruct();

                newEvent.EventName = "RequestLogin";
                newEvent.Data = "";
                newEvent.Data1 = UIM.InputUserIDField.text;
                newEvent.Data2 = UIM.InputUserPasswordField.text;
                
                string toJsonStr = JsonUtility.ToJson(newEvent);
                
                Websocket.Send(toJsonStr);
                Debug.Log($"Send Request Login to server. :. {UIM.InputUserIDField.text}, {UIM.InputUserPasswordField}");
                UIM.InputUserIDField.text = "";
                UIM.InputUserPasswordField.text = "";
            }
            else
            {
                Debug.Log("Can not send \"RequestLogin\" Becuase already login.");
            }
        }

        public void RequestRegister()
        {
            if (!isLogin) // If not isLogin
            {
                SocketEventStruct newEvent = new SocketEventStruct();

                newEvent.EventName = "RequestRegister";
                newEvent.Data = "";
                newEvent.Data1 = UIM.InputRegisterUserIDField.text;
                newEvent.Data2 = UIM.InputRegisterPasswordField.text;
                newEvent.Data3 = UIM.InputRegisterNameField.text;
                
                string toJsonStr = JsonUtility.ToJson(newEvent);
                
                Websocket.Send(toJsonStr);
                UIM.InputUserIDField.text = "";
                UIM.InputUserPasswordField.text = "";
            }
            else
            {
                Debug.Log("Can not send \"RequestLogin\" Becuase already login.");
            }
        }

        public void RequestSendMessage()
        {
            if (isLogin == false) // If not isLogin
                return;

            SocketEventStruct newEvent = new SocketEventStruct();
            newEvent.EventName = "SendMessage";
            newEvent.Data = UIM.InputMessageField.text;
            newEvent.Data1 = userData.UserID;
            newEvent.Data3 = userData.Name;
            newEvent.Data4 = RM.CurrentRoom;
            newEvent.Output = "1";

            string toJsonStr = JsonUtility.ToJson(newEvent);
            
            Websocket.Send(toJsonStr);
            
            UIM.InputMessageField.Select();
            UIM.InputMessageField.text = "";
        }
        
        public void RequestCreateRoom()
        {
            Debug.Log("Create Room Request send.");
            var roomName = UIM.InputRoomNameField.text;
            
            if (Websocket.ReadyState == WebSocketState.Open)
            {
                SocketEventStruct newEvent = new SocketEventStruct();
                newEvent.EventName = "CreateRoom";
                newEvent.Data = roomName;
                string jsonStr = JsonUtility.ToJson(newEvent);
                Websocket.Send(jsonStr);
            }
        }

        public void RequestJoinRoom()
        {
            Debug.Log("Join Room Request Clicked");
            var roomName = UIM.DropDownSelectRoom();
            
            if (Websocket.ReadyState == WebSocketState.Open)
            {
                SocketEventStruct newEvent = new SocketEventStruct();
                newEvent.EventName = "JoinRoom";
                newEvent.Data = roomName;
                newEvent.Data1 = userData.UserID;
                newEvent.Data3 = userData.Name;

                string jsonStr = JsonUtility.ToJson(newEvent);
                Websocket.Send(jsonStr);
            }
        }

        public void RequestAnyExistRoom()
        {
            if (Websocket.ReadyState == WebSocketState.Open)
            {
                SocketEventStruct newEvent = new SocketEventStruct();
                newEvent.EventName = "RequestAnyExistRoom";
                
                string jsonStr = JsonUtility.ToJson(newEvent);
                Websocket.Send(jsonStr);
                
                Debug.Log("Send Request Exist Room");
            }
        }

        public void RequestLeaveRoom()
        {
            var roomName = RM.CurrentRoom;

            if (Websocket.ReadyState == WebSocketState.Open)
            {
                SocketEventStruct newEvent = new SocketEventStruct();
                newEvent.EventName = "LeaveRoom";
                newEvent.Data = roomName;
                newEvent.Data1 = userData.UserID;

                string jsonStr = JsonUtility.ToJson(newEvent);
                Websocket.Send(jsonStr);
            }
        }
        
        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            tempData = messageEventArgs.Data;
        }

        public void Disconnect()
        {
            if (Websocket != null)
            {
                //UIM.SetUIState(NewUIManager.UIAppState.Login);
                //RM.SetCurrentRoom("");
                //isLogin = false;
                
                SocketEventStruct newEvent = new SocketEventStruct();
                newEvent.EventName = "RequestLogout";
                newEvent.Data1 = userData.UserID;
                
                string jsonStr = JsonUtility.ToJson(newEvent);
                Websocket.Send(jsonStr);
            }
        }

        public void QuitApp()
        {
            isConnectToServer = false;
            Websocket.Close();
            Application.Quit();
        }
        
        private void OnDestroy()
        {
            isConnectToServer = false;
            Websocket.Close();
        }
    }
}

