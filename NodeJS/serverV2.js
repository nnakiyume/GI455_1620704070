//const cant assign new value
//var and let can assign new value

const websocket = require('ws');
const sqlite = require('sqlite3').verbose();

//Callback Init and Run server Function
var callbackInitServer = ()=>{
    console.log("============ ...Server is running... ============");
}

let db = new sqlite.Database('./db/chatDB.db', sqlite.OPEN_CREATE | sqlite.OPEN_READWRITE, (err)=>{
    if(err) throw err;

    console.log("[Log] Database connected.");
});

// Web Socket Server
const wss = new websocket.Server({port:8888}, callbackInitServer);

// On Client : For Store
let roomList = [];
let lobbyList = [];
let userList = []; // Contain UserID, WS

ghost = {
    UserID: "Ghost",
    WS: null
}

LobbyGuest = {
    roomName: "Lobby (for Guest)",
    userList: []
}

LobbyUser = {
    roomName: "Lobby (for User)",
    userList: []
}

GhostRoom = {
    roomName: "Room (for ghost)",
    userList: []
}

// lobbyList[0] = for guest
// lobbyList[1] = for User

lobbyList.push(LobbyGuest);
lobbyList.push(LobbyUser);
roomList.push(GhostRoom);
lobbyList[0].userList.push(ghost);
roomList[0].userList.push(ghost);

console.log("[Log] " + GhostRoom)
/*
{
   "roomList": {
        "roomName": "Lobby",
        "userList": {
            "UserID": "",
             WS": "ws"
        }
    }
}
*/

/*
{
   "lobbyList": {
        "roomName": "Lobby",
        "userList": {
            "UserID": "",
             WS": "ws"
        }
    }
}
*/


// WebSocket Event
wss.on("connection", (ws)=> {
    //Automatic Connection to server as Guest then 
    var newUser = {
        UserID: "",
        WS: ws
    }
    lobbyList[0].userList.push(newUser); //Go to Lobby waiting and Login or Register
    console.log("[Log] ws connect to server.")
    
    ws.on("message", (data)=>{
        //console.log("[Log] Received data \"" + data + "\"");
        var toJsonData = JSON.parse(data); //Convert to JSON
        //console.log("[Log] " + data + " was convert to => " + toJsonData);
        DetectEvent(toJsonData,ws);
    });

    ws.on("close", ()=>{
        //Remove userList at current Location
        let indexOfUser = IndexOfUserList(ws,"lobby");
        let indexOfRoom = RoomIndexOfUserList(ws,"lobby");
        
        lobbyList[indexOfRoom].userList = ArrayRemove(lobbyList[indexOfRoom].userList,lobbyList[indexOfRoom].userList[indexOfUser]);
    });
});

function DetectEvent(json,ws){
    switch (json.EventName) {
        //Login
        case "RequestRegister":
            Register(json,ws);
            break;
        case "RequestLogin":
            Login(json,ws);
            break;
        case "RequestLogout":
            Logout(json,ws);
            break;
        //Room
        case "RequestAnyExistRoom":
            RequestRoom(json,ws);
            break;
            
        case "CreateRoom":
            CreateRoom(json,ws);
            break;
            
        case "JoinRoom":
            JoinRoom(json,ws);
            break;
            
        case "LeaveRoom":
            LeaveRoom(json,ws);
            break;
            
        case "SendMessage":
            if(json.Output == '0'){ //0 send as server
                RoomBroadcast(json)
            }
            else{ // send as user
                RoomMessageBroadcast(json);
            }
            
            break;
    }
}

// Event function
function Register(jsonData,ws){
    if(jsonData != null){
        let isFound = false;
        
        let inputUsername = jsonData.Data1;
        let table = "UserData";
        let col_1 = "UserID"; //UserID
        let con = col_1 + "='" + inputUsername + "'"; //WHERE UserID='UserID(input)' 

        let s = SelectAllWhere(table,con);
        console.log("[Log] SQL to execute are : " + s);
        db.all(s, (err, rows) => {
            if (err) {
                console.log("[Log] SQL " + err.message + "--> at " + rows);
            }
            else{
                if(rows[0] != null){ // found data on server
                    if(rows[0].UserID == inputUsername){ // matched data
                        console.log("[Log] Found at " + rows[0] + ":." + rows[0].UserID + ", " + rows[0].Password + ", " + rows[0].Name);
                        isFound = true;
                    }
                    else{
                        isFound = false
                    }
                    console.log("[Log] Register find data result :. " + isFound);
                }
                
                if(isFound == false){ // Insert Into Database
                    
                    console.log("[Log] Do not found " + jsonData.Data1 + " on server. -> Insert Into Database Start...")
                    
                    let table = "UserData";
                    let col_1 = "UserID"; //UserID 
                    let col_2 = "Password"; //Password
                    let col_3 = "Name"; //Name
                    let val_1 = "\'" + jsonData.Data1 + "\'"; //UserID Data
                    let val_2 = "\'" + jsonData.Data2 + "\'"; //Password Data
                    let val_3 = "\'" + jsonData.Data3 + "\'"; //Name Data
                    let combineColumn = col_1 + "," + col_2 + "," + col_3;
                    let combineValue = val_1 + "," + val_2 + "," + val_3;
                    
                    console.log("[Log] SQL Column -> " + col_1 + " " + col_2 + " " + col_3 + " Value -> " + val_1 + " " + val_2 + " " + val_3);
                    console.log("[Log] SQL Column Combine to -> \"[" + combineColumn + "]\"");
                    console.log("[Log] SQL Value Combine to -> \"[" + combineValue + "]\"");
                    let s = InsertInto(table,combineColumn,combineValue);
                    console.log("[Log] SQL to execute are : " + s);
                    ExecuteSQL(s);
                    
                    //Set Message Data that will send back to client
                    var resultData = {
                        EventName: jsonData.EventName,
                        Data: jsonData.Data1,
                        Output: "success"
                    }

                    // Convert Json to string
                    let toJsonStrData = JSON.stringify(resultData);

                    // Send JSON String
                    ws.send(toJsonStrData);
                    console.log("[Server] " + col_1 + " \'" + jsonData.Data1 + "\'" + " has register success.")
                    
                }
                else{ //Found on database
                    console.log("[Server] There are already the same UserID on the server.")
                    //Set Message Data that will send back to client
                    var resultData = {
                        EventName: jsonData.EventName,
                        Data: jsonData.Data1,
                        Output: "fail"
                    }

                    // Convert Json to string
                    let toJsonStrData = JSON.stringify(resultData);

                    // Send JSON String
                    ws.send(toJsonStrData);
                }
            }
        });
    }
}

function Login(jsonData,ws) {

    let inputUsername = jsonData.Data1;
    let inputPassword = jsonData.Data2;

    let table = "UserData";
    let col_1 = "UserID"; //UserID
    let con = col_1 + "='" + inputUsername + "'"; //WHERE UserID='UserID(input)' 

    let s = SelectAllWhere(table, con);
    console.log("[Log] SQL to execute are : " + s);
    db.all(s, (err, rows) => {
        if (err) {
            console.log("[Log] SQL " + err.message + "--> at " + rows);
        } else {
            if (rows[0] != null) { //Found UserID on database.
                if (rows[0].UserID == inputUsername) { //Check UserID
                    
                    function CheckValidLetter(string){
                        if(string.includes("\'") || string.includes("\"") || string.includes(" ")){
                            //Contains some characters that are not valid.  ( ' or " or empty)
                            return true;
                        }
                        else{
                            return false;
                        }
                    }
                    
                    if (CheckValidLetter(inputPassword)) { //Check Password is on Correct ?
                        //The password contains some characters that are not valid.  ( ' or " or empty )
                        
                        
                    } else if (inputPassword == rows[0].Password && CheckValidLetter(inputPassword) != true) { //Succeess
                        //Set Message Data that will send back to client
                        var resultData = {
                            EventName: jsonData.EventName,
                            Data: "",
                            Data1: rows[0].UserID,
                            Data2: rows[0].Password,
                            Data3: rows[0].Name,
                            Output: "success"
                        }

                        let indexOfRoom = RoomIndexOfUserList(ws,"lobby");
                        let indexOfUser = IndexOfUserList(ws,"lobby");
                        
                        lobbyList[indexOfRoom].userList[indexOfUser].UserID = rows[0].UserID;

                        let toLocateWs = GetWSInRoomByUserID(jsonData.Data1,ws,"lobby");
                        ReLocateRoom("lobby->lobby",1,toLocateWs);
                        
                        // Convert Json to string
                        let toJsonStrData = JSON.stringify(resultData);

                        // Send JSON String
                        ws.send(toJsonStrData);
                        console.log("[Log] " + jsonData.Data1 + " login success.")
                        
                    } else if (inputPassword != rows[0].Password && CheckValidLetter(inputPassword) != true) { //Wrong password --Note: fail_2
                        //Set Message Data that will send back to client
                        var resultData = {
                            EventName: jsonData.EventName,
                            Data: inputPassword,
                            Output: "fail_2",
                        }

                        // Convert Json to string
                        let toJsonStrData = JSON.stringify(resultData);

                        // Send JSON String
                        ws.send(toJsonStrData);
                        console.log("[Log] " + jsonData.Data1 + " login fail_2.")
                    }
                } else { //Dont found UserID on database. --Note: fail_1
                    //Set Message Data that will send back to client
                    var resultData = {
                        EventName: jsonData.EventName,
                        Data: inputUsername,
                        Output: "fail_1",
                    }

                    // Convert Json to string
                    let toJsonStrData = JSON.stringify(resultData);

                    // Send JSON String
                    ws.send(toJsonStrData);
                    console.log("[Log] " + jsonData.Data1 + " login fail_1.")
                }
            }
        }
    });
}

function Logout(jsonData,ws){
    //Remove userList at current Location
    let indexOfUser = FindUserByUserID(jsonData.Data1,"lobby");
    let indexOfRoom = RoomIndexOfUserList(ws,"lobby");
    
    let temp = lobbyList[indexOfRoom].userList;
    lobbyList[indexOfRoom].userList = ArrayRemove(lobbyList[indexOfRoom].userList,lobbyList[indexOfRoom].userList[indexOfUser]);

    if(temp != lobbyList[indexOfRoom].userList){
        var resultData = {
            EventName: jsonData.EventName,
            Data1: "",
            Output: "success"
        }

        // Convert Json to string
        let toJsonStrData = JSON.stringify(resultData);

        // Send JSON String
        ws.send(toJsonStrData);
        console.log("[Server] ws disconnected.");
    }
    else{
        var resultData = {
            EventName: jsonData.EventName,
            Data1: "",
            Output: "fail"
        }

        // Convert Json to string
        let toJsonStrData = JSON.stringify(resultData);

        // Send JSON String
        ws.send(toJsonStrData);
        console.log("[Log] Some thing went wrong with logout function.");
    }
    
}

function CreateRoom(jsonData,ws){
    //Creation Room
    console.log("[Log] Received request \"Create Room\" from client")
    //Check Room
    
    var isFoundRoom = CheckExistRoomByName(jsonData.Data);

    //If room is exist
    if(isFoundRoom == true)
    {
        //Set Message Data that will send back to client
        var resultData =
            {
                EventName: jsonData.EventName,
                Data: jsonData.Data,
                Output: "fail"
            }

        // Convert Json to string
        let toJsonStrData = JSON.stringify(resultData);

        // Send JSON String
        ws.send(toJsonStrData);
        console.log("[Server] There are already the same room name on the server.")
    }
    else
    {
        //Creat new room on server
        var newRoom = {
            roomName: jsonData.Data,
            userList: []
        }
        roomList.push(newRoom);
        
        let indexOfRoom = FindRoomIndexByRoomName(newRoom.roomName);
        var str = JSON.stringify(roomList[indexOfRoom]);
        console.log("[Log] Room [" + str + "] was create as " + newRoom.roomName);
        console.log("[Log] Push \'New\' room to roomList as \'" + newRoom.roomName + "\' room.");
        console.log("[Log] Create \'New\' room complete => \"" + str + "\".");

        //Set Message Data that will send back to client
        var resultData = {
            EventName: jsonData.EventName,
            Data: jsonData.Data,
            Output: "success"
        }

        // Convert Json to string
        let toJsonStrData = JSON.stringify(resultData);

        ws.send(toJsonStrData);
    }
}

function JoinRoom(jsonData,ws){
    console.log("[Log] Received request \"Join Room\" from client")
    console.log("[Log] Data as : " + JSON.stringify(jsonData));
    //Check Room & Client
    var isFoundRoom = CheckExistRoomByName(jsonData.Data);
    let isFoundOverWS;
    let FoundWSCount = 0;
    
    //Check Client in room
    for(var i = 0; i < roomList.length; i++){
        for(var j = 0; j < roomList[i].userList.length; j++){
            if(roomList[i].userList[j] == ws){
                FoundWSCount += 1;
            }
        }
    }
    
    if(FoundWSCount > 1){
        isFoundOverWS = true;
    }
    else {
        isFoundOverWS = false;
    }

    console.log("[Log] Join Room Condition Log => " + "isFound : " + isFoundRoom + " WSCount : " + FoundWSCount + " isFounOverWS : " + isFoundOverWS);
    
    if(isFoundRoom == true && isFoundOverWS == false) //If room is exist and not in other room.
    {
        //Join Room
        let table = "UserData";
        let col_1 = "UserID"; //UserID
        let con = col_1 + "='" + jsonData.Data1 + "'"; //WHERE UserID='UserID(input)' 

        let s = SelectAllWhere(table, con);
        console.log("[Log] SQL to execute are : " + s);
        db.all(s, (err, rows) => {
            if (err) {
                console.log("[Log] SQL " + err.message + " --> at " + rows);
            }
            else{
                console.log("[Log] Select " + jsonData.Data + " of " + jsonData.Data1);

                //Re Locate
                let indexOfUser = IndexOfUserList(ws,"lobby");
                let indexOfRoom = RoomIndexOfUserList(ws,"lobby");
                let newRoomIndex = FindRoomIndexByRoomName(jsonData.Data);
                
                let toLocateWs = GetWSInRoomByUserID(jsonData.Data1,ws,"lobby");
                ReLocateRoom("lobby->room",newRoomIndex,toLocateWs);

                //Set Message Data that will send back to client
                var resultData = {
                    EventName: jsonData.EventName,
                    Data: roomList[newRoomIndex].roomName,
                    Data1: rows[0].UserID, // UserID
                    Data3: rows[0].Name, // Name
                    Output: "success"
                }
                
                console.log("[Log] Client join to room \"" + roomList[newRoomIndex].roomName + "\"");

                // Convert Json to string
                let toJsonStrData = JSON.stringify(resultData);

                ws.send(toJsonStrData);
            }
        });
    }
    else if(isFoundRoom == false && isFoundOverWS == true){
        // Force client to Lobby Room
        LeaveRoom(jsonData,ws);

        var resultData =
            {
                EventName: jsonData.EventName,
                Data: "",
                Output: "fail_1" // No room and In Other room.
            }
        // Convert Json to string
        console.log("[Log] The room does not exist on server. and client still in other room.")
        let toJsonStrData = JSON.stringify(resultData);
        ws.send(toJsonStrData);
    }
    else if(isFoundRoom == false && isFoundOverWS == false){
        // Force client to Lobby Room
        
        var resultData = {
            EventName: jsonData.EventName,
            Data: "",
            Output: "fail_2" // No room
        }
        // Convert Json to string
        console.log("[Log] The room does not exist on server.")
        let toJsonStrData = JSON.stringify(resultData);
        ws.send(toJsonStrData);
    }
}

function LeaveRoom(jsonData,ws){
    //Re Locate
    let indexOfUser = IndexOfUserList(ws,"room");
    let indexOfRoom = RoomIndexOfUserList(ws,"room");
    console.log("[Log] --<" + roomList[indexOfRoom] + ">--");
    let toLeaveWS = GetWSInRoomByUserID(jsonData.Data1,ws,"room")
    ReLocateRoom("room->lobby",1,toLeaveWS);

    var resultDataRoom = {
        EventName: jsonData.EventName,
        Data: FindRoomNameByIndex(indexOfRoom,"room"),
        Output: "success"
    }
    
    // Convert Json to string
    let toJsonStrData = JSON.stringify(resultDataRoom);
    ws.send(toJsonStrData);
    console.log("[Log] Send " + toJsonStrData + " to client.");
    
    /*let table = "UserData";
    let col_1 = "UserID"; //UserID
    let value = null;
    let con = col_1 + "='" + jsonData.Data1 + "'"; //WHERE UserID='UserID(input)' 

    let s = UpdateInfo(table,value,con);
    console.log("[Log] SQL to execute are : " + s);
    db.all(s, (err, rows) => {
        if (err) {
            console.log("[Log] SQL " + err.message + "--> at " + rows);
        }
        else{
            console.log("[Log] Update current room of " + jsonData.Data1 + " to " + rows[0].CurrentRoom);
        }
    });*/
}

function RequestRoom(jsonData,ws){
    console.log("[Log] Request find any exist room from client.")

    if(roomList.length > 0)
    {
        console.log("[Log] There are some room on server. Server will send Room name to client.");
        let dataRoomList = "";
        for(var i = 0 ; i < roomList.length; i++){
            if(roomList[i] != null){
                dataRoomList += "#" + roomList[i].roomName;
            }
        }
        
        var resultDataRoom = {
            EventName: jsonData.EventName,
            Data: dataRoomList,
            Output: "success"
        }
        // Convert Json to string
        let toJsonStrData = JSON.stringify(resultDataRoom);
        ws.send(toJsonStrData);
        console.log("[Log] Send " + toJsonStrData + " to client.");
    }
    else{
        var resultData =
            {
                EventName: jsonData.EventName,
                Data: "",
                Output: "fail"
            }
        // Convert Json to string
        console.log("[Log] There are no room on server.");
        let toJsonStrData = JSON.stringify(resultData);
        ws.send(toJsonStrData);
    }
}
// Other Function
/*
//OG Broadcast function
function Broadcast(data){
    for(var i = 0; i < userList.length; i++){
        userList[i].send(data)
    }
}
*/

function RoomBroadcast(jsonData) {
    var resultData =
        {
            EventName: "SendMessage",
            Data: jsonData.Data1 + " " + jsonData.Data, // Message
            Data1: "[Server]", // UserID
            Data4: jsonData.Data4, // RoomName
            Output: "success"
        };
    // Convert Json to string
    let toJsonStrData = JSON.stringify(resultData);
    let targetRoom = jsonData.Data4;

    //Loop Send
    for (var i = 0; i < roomList.length; i++) {
        if (roomList[i].roomName == jsonData.Data4) {
            for (var j = 0; j < roomList[i].userList.length; j++) {
                roomList[i].userList[j].WS.send(toJsonStrData);
            }
            console.log("[Broadcast] " + toJsonStrData + " to everyone in \"" + roomList[i].roomName + "\" room");
            break;
        }
    }
}

function RoomMessageBroadcast(jsonData) {
    var resultData = {
        EventName: jsonData.EventName,
        Data: jsonData.Data, // Message
        Data1: jsonData.Data1, // UserID
        Data3: jsonData.Data3, // Name
        Data4: jsonData.Data4, // RoomName
        Output: "success"
    }
    // Convert Json to string
    var toJsonStrData = JSON.stringify(resultData);

    //Loop Send
    for (var i = 0; i < roomList.length; i++) {
        if (roomList[i].roomName == jsonData.Data4) {
            for (var j = 0; j < roomList[i].userList.length; j++) {
                roomList[i].userList[j].WS.send(toJsonStrData);
            }
            console.log("[Broadcast] " + toJsonStrData + " to everyone in \"" + roomList[i].roomName + "\" room");
            break;
        }
    }
}

/*function splitData(string){
    let res = string.split("#");
    return res;
}*/

function ArrayRemove(anyArray, value){
    return anyArray.filter((element)=>{
        return element != value;
    });
}

function FindUserByUserID(userIdToFind,as){
    if(as == "lobby"){
        for(var i = 0; i < lobbyList.length; i++){
            for(var j = 0; j < lobbyList[i].userList.length; j++){
                if(lobbyList[i].userList[j].UserID == userIdToFind){
                    return j;
                }
            }
        }
    }
    else if(as == "room"){
        for(var i = 0; i < roomList.length; i++){
            for(var j = 0; j < roomList[i].userList.length; j++){
                if(roomList[i].userList[j].UserID == userIdToFind){
                    return j;
                }
            }
        }
    }
}

function IndexOfUserList(ws,as){ //Find on evenry room
    if(as == "lobby"){
        for(var i = 0; i < lobbyList.length; i++){
            for(var j = 0; j < lobbyList[i].userList.length; j++){
                if(lobbyList[i].userList[j].WS == ws){
                    return j;
                }
            }
        }
    }
    else if(as == "room"){
        for(var k = 0; k < roomList.length; k++){
            for(var l = 0; l < roomList[k].userList.length; l++){
                if(roomList[k].userList[l].WS == ws){
                    return l;
                }
            }
        }
    }
    
}

function RoomIndexOfUserList(ws,as){
    if(as == "lobby"){
        for(var i = 0; i < lobbyList.length; i++){
            for(var j = 0; j < lobbyList[i].userList.length; j++){
                if(lobbyList[i].userList[j].WS == ws){
                    console.log("---Room Index Of User List : " + i);
                    return i;
                }
            }
        }
    }
    else if(as == "room"){
        for(var k = 0; k < roomList.length; k++){
            for(var l = 0; l < roomList[k].userList.length; l++){
                if(roomList[k].userList[l].WS == ws){
                    console.log("---Room Index Of User List : " + k);
                    return k;
                }
            }
        }
    }
}

function FindRoomIndexByRoomName(nameToFind,as){
    if(as == "lobby"){
        for(var i = 0; i < lobbyList.length; i++){
            if(lobbyList[i].roomName == nameToFind){
                return i;
            }
        }
    }
    else{
        for(var i = 0; i < roomList.length; i++){
            if(roomList[i].roomName == nameToFind){
                return i;
            }
        }
    }
    
}

function FindRoomNameByIndex(indexToFind,as){
    if(as == "lobby"){
        for(var i = 0; i < lobbyList.length; i++){
            if(i == indexToFind){
                return lobbyList[i].roomName;
            }
            else{
                return null;
            }
        }
    }
    else{
        for(var i = 0; i < roomList.length; i++){
            if(i == indexToFind){
                return roomList[i].roomName;
            }
            else{
                return null;
            }
        }
    }
    
}

function CheckExistRoomByName(nameToFind){
    for(var i = 0; i < roomList.length; i++){
        if(roomList[i].roomName == nameToFind){
            return true;
        }
    }
    return false;
}

function GetWSInRoomByUserID(userIDToFind,eventWS,as){
    if(as == "room"){
        for(var i = 0; i < roomList.length; i++){
            for(var j = 0; j < roomList[i].userList.length; j++){
                if(roomList[i].userList[j].UserID == userIDToFind){
                    if(roomList[i].userList[j].WS == eventWS){
                        return roomList[i].userList[j].WS;
                    }
                }
            }
        }
    }
    else if(as == "lobby"){
        for(var i = 0; i < lobbyList.length; i++){
            for(var j = 0; j < lobbyList[i].userList.length; j++){
                if(lobbyList[i].userList[j].UserID == userIDToFind){
                    if(lobbyList[i].userList[j].WS == eventWS){
                        return lobbyList[i].userList[j].WS;
                    }
                }
            }
        }
    }
}

//Leave Room and Join Room 
function ReLocateRoom(LocateTo,NewRoomIndex,theWS){
    if(LocateTo == "lobby->room"){
        let indexOfRoom = RoomIndexOfUserList(theWS,"lobby");
        let userIndex = IndexOfUserList(theWS,"lobby");
        console.log("[ReLocate Log] START indexOfRoom \'" + indexOfRoom + "\' and userIndex \'" + userIndex + "\'");
        console.log("----------------------------------------------------------");
        var str = JSON.stringify(lobbyList[indexOfRoom].userList[userIndex]);
        console.log("[0] Before Client in " + lobbyList[indexOfRoom].roomName + " as " + str + " at index " + userIndex + ".");
        console.log("[1] Start re-locate \'" + LocateTo + "\' user");

        var newUser = {
            UserID: lobbyList[indexOfRoom].userList[userIndex].UserID,
            WS: theWS
        }
        
        roomList[NewRoomIndex].userList.push(newUser);
        console.log("[2] Push \'Defined User\' to \'" + roomList[NewRoomIndex].roomName + "\' room.");

        lobbyList[indexOfRoom].userList = ArrayRemove(lobbyList[indexOfRoom].userList, lobbyList[indexOfRoom].userList[userIndex]);
        console.log("[3] Remove \'Defined User\' from  \'" + lobbyList[indexOfRoom].roomName + "\' room.");
        
        userIndex = IndexOfUserList(theWS,"room");
        console.log("Re-locate " + roomList[NewRoomIndex].userList[userIndex] + " finsh.");
        
        indexOfRoom = RoomIndexOfUserList(theWS,"room");
        var str2 = JSON.stringify(roomList[indexOfRoom].userList[userIndex]);
        console.log("[4] After Client in " + roomList[indexOfRoom].roomName + " as " + str2 + " at index " + userIndex + ".");
        console.log("----------------------------------------------------------");
        console.log("[ReLocate Log] END indexOfRoom \'" + indexOfRoom + "\' and userIndex \'" + userIndex + "\'");
    }
    else if(LocateTo == "room->lobby"){
        let indexOfRoom = RoomIndexOfUserList(theWS,"room");
        let userIndex = IndexOfUserList(theWS,"room");
        console.log("[ReLocate Log] START indexOfRoom \'" + indexOfRoom + "\' and userIndex \'" + userIndex + "\'");
        console.log("----------------------------------------------------------");
        var str = JSON.stringify(roomList[indexOfRoom].userList[userIndex]);
        console.log("[0] Before Client in " + roomList[indexOfRoom].roomName + " as " + str + " at index " + userIndex + ".");
        console.log("[1] Start re-locate \'" + LocateTo + "\' user");

        var newUser = {
            UserID: roomList[indexOfRoom].userList[userIndex].UserID,
            WS: theWS
        }

        lobbyList[NewRoomIndex].userList.push(newUser);
        console.log("[2] Push \'Defined User\' to \'" + lobbyList[NewRoomIndex].roomName + "\' room.");

        roomList[indexOfRoom].userList = ArrayRemove(roomList[indexOfRoom].userList, roomList[indexOfRoom].userList[userIndex]);
        console.log("[3] Remove \'Defined User\' from  \'" + roomList[indexOfRoom].roomName + "\' room.");
        
        userIndex = IndexOfUserList(theWS,"lobby");
        console.log("Re-locate " + lobbyList[NewRoomIndex].userList[userIndex] + " finsh.");

        indexOfRoom = RoomIndexOfUserList(theWS,"lobby");
        var str2 = JSON.stringify(lobbyList[indexOfRoom].userList[userIndex]);
        console.log("[4] After Client in " + lobbyList[indexOfRoom].roomName + " as " + str2 + " at index " + userIndex + ".");
        console.log("----------------------------------------------------------");
        console.log("[ReLocate Log] END indexOfRoom \'" + indexOfRoom + "\' and userIndex \'" + userIndex + "\'");
        
    }
    else if(LocateTo == "lobby->lobby"){
        let indexOfRoom = RoomIndexOfUserList(theWS,"lobby");
        let userIndex = IndexOfUserList(theWS,"lobby");
        console.log("[ReLocate Log] START indexOfRoom \'" + indexOfRoom + "\' and userIndex \'" + userIndex + "\'");
        console.log("----------------------------------------------------------");
        var str = JSON.stringify(lobbyList[indexOfRoom].userList[userIndex]);
        console.log("[0] Before Client in " + lobbyList[indexOfRoom].roomName + " as " + str + " at index " + userIndex + ".");
        console.log("[1] Start re-locate \'" + LocateTo + "\' user");

        var newUser = {
            UserID: lobbyList[indexOfRoom].userList[userIndex].UserID,
            WS: theWS
        }

        lobbyList[NewRoomIndex].userList.push(newUser);
        console.log("[2] Push \'Defined User\' to \'" + lobbyList[NewRoomIndex].roomName + "\' room.");

        lobbyList[indexOfRoom].userList = ArrayRemove(lobbyList[indexOfRoom].userList, lobbyList[indexOfRoom].userList[userIndex]);
        console.log("[3] Remove \'Defined User\' from  \'" + lobbyList[indexOfRoom].roomName + "\' room.");

        userIndex = IndexOfUserList(theWS,"lobby");
        console.log("Re-locate " + lobbyList[NewRoomIndex].userList[userIndex] + " finsh.");

        indexOfRoom = RoomIndexOfUserList(theWS,"lobby");
        var str2 = JSON.stringify(lobbyList[indexOfRoom].userList[userIndex]);
        console.log("[4] After Client in " + lobbyList[indexOfRoom].roomName + " as " + str2 + " at index " + userIndex + ".");
        console.log("----------------------------------------------------------");
        console.log("[ReLocate Log] END indexOfRoom \'" + indexOfRoom + "\' and userIndex \'" + userIndex + "\'");
    }
}

//SQL function

function SelectSpecific(column,table){
    //Syntax : SELECT column1, column2, ... FROM table_name;
    let sql = "SELECT " + column + "FROM " + table + ";";
    return sql;
}

function SelectSpecificWhere(column,table,conditional){
    //Syntax : SELECT column1, column2, ... FROM table_name WHERE condition;
    let sql = "SELECT " + column + "FROM " + table + " WHERE " + conditional + ";";
    return sql;
}

function SelectAll(table){
    //Syntax : SELECT * FROM table_name;
    let sql = "SELECT " + "*" + " FROM " + table + ";";
    return sql;
}

function SelectAllWhere(table,conditional){
    //Syntax : SELECT * FROM table_name WHERE condition;
    let sql = "SELECT " + "*" + " FROM " + table + " WHERE " + conditional + ";";
    return sql;
    
}

function InsertInto(table,column,value){
    //Syntax : INSERT INTO table_name (column1, column2, column3, ...) VALUES (value1, value2, value3, ...);
    let sql = "INSERT " + "INTO " + table + " (" + column + ") VALUES (" + value + ");";
    return sql;
}

function UpdateInfo(table,value,condition) {
    //Syntax : UPDATE table_name SET column1 = value1, column2 = value2, ... WHERE condition;
    let sql = "UPDATE " + table + " SET " + value + " WHERE " + condition + ";";
    return sql;
}

function DeleteInfo(table, condition) {
    //Syntax : DELETE FROM table_name WHERE condition;
    let sql = "DELETE FROM " + table + " WHERE " + condition + ";";
    return sql;
}

function ExecuteSQL(sql){
    db.run(sql, (err) => {
        if (err) {
            console.log("[Log] SQL " + err.message);
        }
        else{
            console.log("[SQL Result] Execute \"" + sql + "\" success.")
        }
    });
    
    //db.close();
}
        