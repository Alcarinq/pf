namespace Database
open System.Data.SQLite

module Database =
    type Test = {Id: int; Name: string; Price: int}

    let mutable data = []
    let fillData (_id: int) (name: string) (price: int) list1=
        List.append list1 [{Test.Id=_id; Test.Name=name; Test.Price=price}]

    type Result = Success | Failure  
    let databaseName = "../database.db"
    let connectionString = sprintf "Data Source=%s;Version=3" databaseName

    let tableStructure = @"CREATE TABLE IF NOT EXISTS STUFF (
                            _id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT,
                            Price INTEGER)"

    let private createTable() = 
        try
            let cn = new SQLiteConnection(connectionString)
            cn.Open()

            let createTableScript = tableStructure
            let cmd = new SQLiteCommand(createTableScript, cn)
            let result = cmd.ExecuteNonQuery()
            cn.Close()
            
            Success
        with
            | _ -> Failure

    let selectQuery = 
        sprintf @"SELECT * FROM STUFF"

    let selectLoginQuery login password = 
        sprintf @"SELECT * FROM USERS WHERE login = '%s' AND password = '%s'" login password

    let insertQuery name price = 
        sprintf @"INSERT INTO STUFF (Name, Price) values ('%s', '%d')" name price 

    let deleteQuery id = 
        sprintf @"DELETE FROM STUFF WHERE _id = %d" id 

                          
    let select () =
        data <- List.empty
        if createTable() = Success 
            then
                try
                    let cn = new SQLiteConnection(connectionString)
                    cn.Open()

                    let sql = selectQuery
                    let cmd = new SQLiteCommand(sql, cn)
                    let result = cmd.ExecuteReader()
        
                    
                    while result.Read() do
                        data <- fillData (result.["_id"].ToString() |>int) (result.["name"].ToString()) (result.["price"].ToString() |>int) data
                        printfn "ID: %i, Name: %s, Price: %i" (result.["_id"].ToString() |>int) (result.["name"].ToString()) (result.["price"].ToString() |>int)

                    cn.Close()
                    
                    if data.Length = 0 then Failure
                    else Success
                with 
                    | _ -> Failure

            else Failure 

    let insert name price = 
        if createTable() = Success 
            then
                try
                    let cn = new SQLiteConnection(connectionString)
                    cn.Open()

                    let sql = insertQuery name price
                    let cmd = new SQLiteCommand(sql, cn)
                    let result = cmd.ExecuteNonQuery()
        
                    cn.Close()
                    
                    if result = -1 then Failure
                    else Success
                with 
                    | _ -> Failure

            else Failure 

    let delete (id:int) =
        if createTable() = Success 
            then
                try
                    let cn = new SQLiteConnection(connectionString)
                    cn.Open()

                    let sql = deleteQuery id
                    let cmd = new SQLiteCommand(sql, cn)
                    let result = cmd.ExecuteNonQuery()
        
                    cn.Close()
                    
                    if result = -1 then Failure
                        else Success
                 with 
                    | _ -> Failure  
            else Failure 
              
    let checkLogin login password =
        try
            let cn = new SQLiteConnection(connectionString)
            cn.Open()

            let sql = selectLoginQuery login password
            let cmd = new SQLiteCommand(sql, cn)
            let result = cmd.ExecuteReader()
        
            cn.Close()

            let userType =
                if result.HasRows then
                    while result.Read() do
                        if result.["group"].ToString() = "admin" then "admin" else "user" 
                else ()
            ""
        with 
            | _ -> ""