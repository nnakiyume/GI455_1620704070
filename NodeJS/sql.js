const sqlite = require('sqlite3').verbose();

let db = new sqlite.Database('./db/chatDB.db', sqlite.OPEN_CREATE | sqlite.OPEN_READWRITE, (err)=>{
    if(err) throw err;

    console.log("[Log] Database connected.")

    var table = "UserData";
    var conditional = "UserID='admin'"
    var sql = "SELECT * FROM " + table + " WHERE " + conditional + ";";
    
    var result = db.all(sql, (err,rows)=>{
        if(err){
            //return false;
            console.log("[Log] SQL " + err + " at " + rows);
        }
        else{
            if(rows.length > 0){
                var data = rows[0].UserID;
                console.log(data);
            }
            //return true;
        }
        
    });
});




