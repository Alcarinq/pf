namespace Database
open System.Data.SQLite

module Database =
    type Test = {Id: int; Name: string; Price: int}

    let mutable myTest = []
    let fillMyTest (_id: int) (name: string) (price: int) list1=
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
                          
    let select =
        try
            let cn = new SQLiteConnection(connectionString)
            cn.Open()

            let sql = "select * from STUFF"
            let cmd = new SQLiteCommand(sql, cn)
            let result = cmd.ExecuteReader()
                        
            while result.Read() do
                myTest <- fillMyTest (result.["_id"].ToString() |>int) (result.["name"].ToString()) (result.["price"].ToString() |>int) myTest
                printfn "ID: %i, Name: %s, Price: %i" (result.["_id"].ToString() |>int) (result.["name"].ToString()) (result.["price"].ToString() |>int)
    
            cn.Close()

            Success
        with
            | _ -> Failure

    let insertQuery name price = 
        sprintf @"INSERT INTO STUFF (Name, Price) values ('%s', '%d')" name price 

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