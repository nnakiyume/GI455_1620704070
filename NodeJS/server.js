var websocket = require('ws');

//Callback Init and Run server Function
var callbackInitServer = ()=>{
    console.log("============ ....Server is running... ============");
}

// Web Socket Server
var wss = new websocket.Server({port:8888}, callbackInitServer);

// On Client
var wsList = [];
var Lobby = [];
var roomList = [];
/* roomList format
{
    roomName: "string",
    wsList: [] //Array
}

*/
/* SocketData Json format
{
    eventName: "string",
    roomName: "string",  //Data
}
*/
/* MessageData Json format
{
    username: "string",
    message: "string",
}
*/

wss.on("connection", (ws)=>{
    {
        Lobby = {
            roomName: "Lobby",
            wsList:[]
        }
        Lobby.wsList.push(ws);

        //Entry lobby zone
        ws.on("message", (data)=>{

            console.log("[log] Received data \"" + data + "\"");
            var toJsonData = JSON.parse(data); //Convert to JSON
            console.log("[log] JSON Data was convert to => " + toJsonData);
            
            //Creation Room
            if(toJsonData.EventName == "CreateRoom")
            {
                console.log("[log] Received request \"Create Room\" from client")
                
                //Check Room
                var isFoundRoom = false;

                for(var i = 0; i < roomList.length; i++)
                {
                    if(roomList[i].roomName == toJsonData.Data)
                    {
                        isFoundRoom = true;
                        break;
                    }
                }

                //If room is exist
                if(isFoundRoom)
                {
                    console.log("[Server] There are already the same room name on the server.")
                    
                    //Set Message Data that will send back to client
                    var resultData = 
                    {
                        EventName: toJsonData.EventName,
                        Data: toJsonData.Data,
                        Output: "fail"
                    }

                    // Convert Json to string
                    var toJsonStrData = JSON.stringify(resultData);
                    
                    // Send JSON String
                    ws.send(toJsonStrData);
                }
                else
                {
                    //Creat new room on server
                    var newRoom = 
                    {
                        roomName : toJsonData.Data,
                        wsList:[]
                    }

                    roomList.push(newRoom);

                    console.log("[log] Create new room named \"" + roomList[0].roomName + "\".");
                    
                    //Set Message Data that will send back to client
                    var resultData = 
                    {
                        EventName: toJsonData.EventName,
                        Data: toJsonData.Data,
                        Output: "success"
                    };
                    
                    // Convert Json to string
                    var toJsonStrData = JSON.stringify(resultData);
                
                    ws.send(toJsonStrData);
                }
            }
            //Join Room
            else if(toJsonData.EventName == "JoinRoom")
            {
                console.log("[log] Received request \"Join Room\" from client")
                //Check Room & Client
                var isFoundRoom = false;
                var isFoundWSOtherRoom = false;
                var foundOnRoomIndex = 0;

                for(var i = 0; i < roomList.length; i++)
                {
                    if(roomList[i].roomName == toJsonData.Data)
                    {
                        console.log("[log] Found " + toJsonData.Data + " room to join");
                        foundOnRoomIndex = i;
                        isFoundRoom = true;
                        break;
                    }
                }

                //Check Client in room
                for(var i = 0; i < roomList.length; i++){
                    for(var j = 0; j < roomList[i].wsList.length; j++){
                        if(roomList[i].wsList == ws){
                            isFoundWSOtherRoom = true;
                            break;
                        }
                    }
                }

                //If room is exist and not in other room.
                if(isFoundRoom == true && isFoundWSOtherRoom == false)
                {
                    //Remove from Lobby
                    ArrayRemove(Lobby.wsList, ws);
                    
                    //Join Room
                    console.log("[log] Client join to room \"" + roomList[foundOnRoomIndex].roomName + "\"");
                    roomList[foundOnRoomIndex].wsList.push(ws); //Join client to wsList in room
                    RoomMessageBroadcast(toJsonData.Data,toJsonData," enter room.");

                    //Set Message Data that will send back to client
                    var resultData = 
                    {
                        EventName: toJsonData.EventName,
                        Data: roomList[foundOnRoomIndex].roomName,
                        Output: "success"
                    }
                    // Convert Json to string
                    var toJsonStrData = JSON.stringify(resultData);
                
                    ws.send(toJsonStrData);
                }
                else if(isFoundRoom == false && isFoundWSOtherRoom == true){
                    // Force client leave room
                    ForceLeave(toJsonData,ws);
                    
                    var resultData = 
                    {
                        EventName: toJsonData.EventName,
                        Data: "",
                        Output: "fail_1" // No room and In Other room.
                    }
                    // Convert Json to string
                    console.log("[log] The room does not exist on server. and client still in other room.")
                    var toJsonStrData = JSON.stringify(resultData);
                    ws.send(toJsonStrData);
                }
                else if(isFoundRoom == false && isFoundWSOtherRoom == false){
                    // Force client leave room
                    ForceLeave(toJsonData,ws);
                    
                    var resultData = 
                    {
                        EventName: toJsonData.EventName,
                        Data: "",
                        Output: "fail_2" // No room
                    }
                    // Convert Json to string
                    console.log("[log] The room does not exist on server.")
                    var toJsonStrData = JSON.stringify(resultData);
                    ws.send(toJsonStrData);
                }
                else if(isFoundRoom == true && isFoundWSOtherRoom == false){
                    // Force client leave room
                    ForceLeave(toJsonData,ws);
                    
                    var resultData = 
                    {
                        EventName: toJsonData.EventName,
                        Data: "",
                        Output: "fail_3" // Has room but still in other room.
                    }
                    // Convert Json to string
                    console.log("[log] Client still in other room.")
                    var toJsonStrData = JSON.stringify(resultData);
                    ws.send(toJsonStrData);

                }
                
            }
            //Leave Room
            else if(toJsonData.EventName == "LeaveRoom")
            {
                ForceLeave(toJsonData,ws);
            }
            //Request Exist Room
            else if(toJsonData.EventName == "RequestAnyExistRoom")
            {
                console.log("[log] Request find any exist room from client.")
                
                if(roomList.length > 0)
                {
                    console.log("[log] There are some room on server. Server will send Room name to client.");
                    var dataRoomList = "";
                    for(var n = 0 ; n < roomList.length; n++){
                        if(roomList[n] != null){
                            dataRoomList += "#" + roomList[n].roomName;
                        } 
                    }
                    var resultDataRoom = {
                        EventName: toJsonData.EventName,
                        Data: dataRoomList,
                        Output: "success"
                    }
                    // Convert Json to string
                    var toJsonStrData = JSON.stringify(resultDataRoom);
                    ws.send(toJsonStrData);
                    console.log("[log] Send " + toJsonStrData + " to client.");
                }
                else{
                    var resultData = 
                    {
                        EventName: toJsonData.EventName,
                        Data: "",
                        Output: "fail"
                    }
                    // Convert Json to string
                    console.log("[log] There are no room on server.");
                    var toJsonStrData = JSON.stringify(resultData);
                    ws.send(toJsonStrData);
                }
            }

            else if(toJsonData.Username != "" || toJsonData.Username != null)
            {
                RoomBroadcast(toJsonData.CurrentRoom,toJsonData)
            }
        });
    }

    //Connect
    //console.log("Client connected.");  

    //Close
    ws.on("close", ()=>{
        for(var i = 0; i < Lobby.wsList.length; i++){
            if(Lobby.wsList[i] == ws){
                ArrayRemove(Lobby.wsList, ws);
                console.log("[log] Client disconnected.");
            }
            else{
                FindAndRemoveClientAInRoom(ws);
            }
        }
    });
});

function ArrayRemove(anyArray, value){
    return anyArray.filter((element)=>{
        return element != value;
    });
}

// Broadcasting
function RoomBroadcast(targetRoom, toJsonData)
{
    var resultData = 
    {
        Username: toJsonData.Username,
        Message: toJsonData.Message,
        CurrentRoom: targetRoom
    }
     // Convert Json to string
    var toJsonStrData = JSON.stringify(resultData);
    //ws.send(toJsonStrData);

    for(var i = 0; i < roomList.length; i++)
    {
        if(roomList[i].roomName == targetRoom)
        {
            for(var j = 0; j < roomList[i].wsList.length; j++)
            {
                roomList[i].wsList[j].send(toJsonStrData);
            }
            console.log("[Broadcast] " + toJsonStrData + " to everyone in \"" + roomList[i].roomName + "\" room");
            break;
        }
    }
}

function RoomMessageBroadcast(targetRoom, toJsonData, Message)
{
    var resultData = 
    {
        Username: toJsonData.Output,
        Message: toJsonData.Output + Message,
        CurrentRoom: targetRoom
    }
     // Convert Json to string
    var toJsonStrData = JSON.stringify(resultData);
    //ws.send(toJsonStrData);

    for(var i = 0; i < roomList.length; i++)
    {
        if(roomList[i].roomName == targetRoom)
        {
            for(var j = 0; j < roomList[i].wsList.length; j++)
            {
                roomList[i].wsList[j].send(toJsonStrData);
            }
            console.log("[Broadcast] " + toJsonStrData + " to everyone in \"" + roomList[i].roomName + "\" room");
            break;
        }
    }
}

// function Broadcast(data){
//     for(var i = 0; i < wsList.length; i++){
//         wsList[i].send(data)
//     }
// }

function FindAndRemoveClientAInRoom(ws){
    for(var i = 0; i < roomList.length; i++){
        for(var j = 0; i < roomList[i].wsList.length; i++){
             if(roomList[i].wsList[j] == ws){
                ArrayRemove(roomList[i].wsList, ws);
             }
        }
    }
}


function ForceLeave(toJsonData,ws){
    // Force client leave room
    for(var i = 0; i < roomList.length; i++)
    {
        for(var j = 0; j < roomList[i].wsList.length; j++)
        {
        if(roomList[i].wsList[j] == ws)
            {
                //Remove from Room
                ArrayRemove(roomList[i].wsList, ws);
                console.log("[log] Client leave" + toJsonData.Data);
                RoomMessageBroadcast(toJsonData.Data,toJsonData,"leave room.");

                //Enter to Lobby
                Lobby.wsList.push(ws);
                break;
            }
        }
    }
}