using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;
using System.Linq;
using UnityEngine.UI;

namespace _MidTerm
{
    public class WSPreMid : MonoBehaviour
    {
        struct EventGetStudentData
        {
            public string eventName;
            public string studentID;

            public EventGetStudentData (string eventName, string studentID)
            {
                this.eventName = eventName;
                this.studentID = studentID;
            }
        }

        struct StudentData
        {
            /*
                {
                "eventName":"GetStudentData",
                "status":true,
                "message":"success",
                "studentName":"MR. NUTTHANON PATSUWAN",
                "studentEmail":"nutthanon.pats@bumail.net"
                }
             */
            public string eventName;
            public bool status;
            public string message;
            public string studentName;
            public string studentEmail;
        }
        
        //Field
        public WebSocket ws;

        [SerializeField] private InputField studentIDField;
        
        [SerializeField] private Text EventNameText;
        [SerializeField] private Text InfoText;

        private string tempData;

        private void Start()
        {
            tempData = "";
            ConnectToServer();
        }

        private void Update()
        {
            if (tempData != "")
            {
                Debug.Log($"Debug temp : {tempData}");
                var json = JsonUtility.FromJson<StudentData>(tempData);

                var format = $"<size=28><b>{json.studentName}</b></size>\n<size=20><i>({json.studentEmail})</i></size>";
                
                InfoText.text = format;

                tempData = "";
            }
        }

        public void ConnectToServer()
        {
            var url = "ws://gi455-305013.an.r.appspot.com:80/";
            ws = new WebSocket(url);
            
            ws.OnMessage += OnMessage;
            
            ws.Connect();
            
            Debug.Log($"Connect to {url}");
        }
        
        public void GetStudentDataClick()
        {
            Debug.Log("ClickGet!");

            if (ws.ReadyState == WebSocketState.Open)
            {
                EventGetStudentData newEvent = new EventGetStudentData();
                newEvent.eventName = "GetStudentData";
                
                if (studentIDField.text != null || studentIDField.text != "")
                {
                    newEvent.studentID = studentIDField.text;
                }
                else
                {
                    newEvent.studentID = "1620704070";
                }
            
                string toJsonStr = JsonUtility.ToJson(newEvent);
                EventNameText.text = toJsonStr;
                ws.Send(toJsonStr);
                Debug.Log($"Request : {toJsonStr} to server.");
            }
        }

        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            tempData = messageEventArgs.Data;
        }
        
        private void OnDestroy()
        {
            Debug.Log("Disconnect");
            ws.Close();
        }
    }
}

