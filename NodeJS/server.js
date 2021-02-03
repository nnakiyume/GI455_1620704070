var websocket = require('ws');

//Callback Init and Run server Function
var callbackInitServer = ()=>{
    console.log("Server is running...");
}

// Web Socket Server
var wss = new websocket.Server({port:8888}, callbackInitServer);

// On Client
var wsList = [];

wss.on("connection", (ws)=>{
    //Connect
    wsList.push(ws);
    console.log("Client connected.");

    //Send
    ws.on("message", (data)=>{
        console.log("[log] Received from client => " + data);
        Boardcast(data);
    });

    //Close
    ws.on("close", ()=>{
        wsList = ArrayRemove(wsList, ws);
        console.log("Client disconnected.");
    });
});

function ArrayRemove(anyArray, value){
    return anyArray.filter((element)=>{
        return element != value;
    });
}

// Boardcasting
function Boardcast(data){
    for(var i = 0; i < wsList.length; i++){
        wsList[i].send(data)
    }
}
