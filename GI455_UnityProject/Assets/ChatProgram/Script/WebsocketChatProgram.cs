using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System.Linq;
using UnityEngine.UI;
using WebSocketSharp.Net;

namespace ChatProgram
{
    public class WebsocketChatProgram : MonoBehaviour
    {
        
        // Properties
        public UIManager ui;
        
        private WebSocket webSocket;
        [Header("WebSocket Properties")]
        public string Username;
        //public int AvatarSprite_id;
        [SerializeField] private string ipAddress;
        [SerializeField] private string port;
        [SerializeField] private string url;
        [SerializeField] private string message;
        private bool serverConnectStatus;
        //[SerializeField] private string[] chatDataLog;

        //public Sprite[] AvatarSprites = new Sprite[2];
        public GameObject ChatPrefab;
        public GameObject ChatEmptyPrefab;
        public GameObject ChatContainerLeft;
        public GameObject ChatContainerRight;
        

        // Behavior
        void Start()
        {
            serverConnectStatus = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (!serverConnectStatus)
                {
                    if (ui.ipField.text != null || ui.portField.text != null)
                    {
                        ConnectToServer();
                    }
                }
                else
                {
                    SendChatMessage();
                }
            }

            if (!serverConnectStatus)
            {
                ui.ConnectServerPanel.SetActive(true);
                ui.ChatPanel.SetActive(false);
            }
            else
            {
                ui.ConnectServerPanel.SetActive(false);
                ui.ChatPanel.SetActive(true);
            }
            
            if (!message.IsNullOrEmpty()) //IF MESSAGE NOT NULL OR EMPTY
            {
                UpdateChatUI();
            }
        }

        public void UpdateChatUI()
        {
            var prefix = Username + " :";
            
            // Search String Conditional
            if (message.Contains(prefix))
            {
                Debug.Log($"Conditional result : found \"{prefix}\" in {message}");
                ChatAlignRight();
            }
            else
            {
                Debug.Log($"Conditional result : do not found \"{prefix}\" in {message}");
                ChatAlignLeft();
            }

            if (message != null || message != "")
            {
                message = null;
            }
        }

        public void ChatAlignLeft() // Other is sender
        {
            ui.ChatAlignRightText.text += "\n";
            ui.ChatAlignLeftText.text += message + "\n";
        }
        
        public void ChatAlignRight() // User is sender
        {
            ui.ChatAlignLeftText.text += "\n";
            ui.ChatAlignRightText.text += message + "\n";
        }

        //-- Server progress
        public void SendChatMessage()
        {
            var text = ui.chatMessageField.text; ;
            if (webSocket.ReadyState == WebSocketState.Open)
            {
                webSocket.Send($"{Username} : {text}");
            }

            ui.chatMessageField.text = null;
            ui.chatMessageField.Select();
        }
        
        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            message = messageEventArgs.Data;
            Debug.Log($"Receive message from server => {message}");
        }

        /*public void OnRecieveMessage()
        {
            whileRecieveMessage = true;
        }*/
        
        /*public void CreateChatContext()
        {
            if (chatDataLog.Contains($"{Username} : ")) // If sender is user
            {
                InstantiateItemToParent(ChatPrefab,ChatContainerRight);
                InstantiateItemToParent(ChatEmptyPrefab, ChatContainerLeft);
            }
            else // If sender is othere
            {
                InstantiateItemToParent(ChatPrefab,ChatContainerLeft);
                InstantiateItemToParent(ChatEmptyPrefab, ChatContainerRight);
            }
        }*/

        /*public void InstantiateItemToParent(GameObject item, GameObject parent)
        {
            
            GameObject output = Instantiate(item,new Vector3 (0,0,0), Quaternion.identity) as GameObject;
            output.transform.SetParent(parent.transform);
        }*/
        
        public void ConnectToServer()
        {
            if (!serverConnectStatus)
            {

                if (ui.ipField.text == null || ui.portField.text == null || ui.ipField.text == "" || ui.portField.text == "")
                {
                    ui.SendMessageToShow("Please enter <b><color=#DC143C>\"Server IP Address or Port\"</color></b>.");
                }
                else
                {
                    //Set Parameter from Input Field
                    if (Username == null)
                    {
                        Username = "Guest";
                    }
                    else
                    {
                        Username = ui.usernameField.text;
                    }
                    
                    ui.RemoveSpaceInInputField();
                    ipAddress = ui.ipField.text;
                    port = ui.portField.text;
                    url = $"ws://{ipAddress}:{port}/";
                    
                    // Server Connecting
                    webSocket = new WebSocket(url); // Set Connection
                    webSocket.Connect(); // Connect
                    webSocket.OnMessage += OnMessage; // Set OnMessage Event
                    webSocket.Send($"{Username} conected.");
                    ui.SendMessageToShow($"<b>{Username} <color=#3CB371>connected</color></b> to server via <color=#4682B4>{url}</color> ."); // Update UI
                    serverConnectStatus = true;
                }
                
            }
        }
        
        private void OnDestroy()
        {
            if (webSocket != null)
            {
                webSocket.Send($"{Username} disconected.");
                webSocket.Close();
            }
        }
    }
}

