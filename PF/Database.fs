namespace Database
open System.Data.SQLite
open System

module Database =
    type Test = {Id: int; Name: string; Price: int; Rent: bool}

    let mutable data = []
    let mutable userType = "empty"
    let fillData (_id: int) (name: string) (price: int) (rent: int) list1=
        List.append list1 [{Test.Id=_id; Test.Name=name; Test.Price=price; Test.Rent=if rent = 1 then true else false}]

    type Result = Success | Failure  
    let databaseName = "../database.db"
    let connectionString = sprintf "Data Source=%s;Version=3" databaseName

    let tableStructure = @"CREATE TABLE IF NOT EXISTS STUFF (
                            _id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT,
                            Price INTEGER,
                            Rent INTEGER)"

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

    let selectQuery () =
        if userType = "admin" then
            sprintf @"SELECT * FROM STUFF"
        else
            sprintf @"SELECT * FROM STUFF where Rent = 0"

    let selectLoginQuery login password = 
        sprintf @"SELECT * FROM USERS WHERE login = '%s' AND password = '%s'" login password

    let insertItemQuery name price = 
        sprintf @"INSERT INTO STUFF (Name, Price) values ('%s', '%d')" name price 

    let updateItemQuery id name price rent = 
        sprintf @"UPDATE STUFF SET Name = '%s', Price = %d, Rent = %d WHERE _id = %d" name price rent id

    let deleteQuery id = 
        sprintf @"DELETE FROM STUFF WHERE _id = %d" id 
 
    let removeRentQuery id = 
        sprintf @"UPDATE STUFF SET Rent = 0 WHERE _id = %d" id 

    let rentItem id = 
        sprintf @"UPDATE STUFF SET Rent = 1 WHERE _id = %d" id 
          
    type Tquery (qType:string, ?id:int, ?name:string, ?price:int, ?rent:int) =
      let id = defaultArg id 0
      let name = defaultArg name ""
      let price = defaultArg price 0
      let rent = defaultArg rent 0

      member this.ToExpression() =  
        match qType with
          | "insertItemQuery" -> insertItemQuery name price
          | "updateItemQuery" -> updateItemQuery id name price rent
          | "deleteQuery" -> deleteQuery id
          | "removeRentQuery" -> removeRentQuery id
          | "rentItem" -> rentItem id
          | _ -> "" 
                          
    let select () =
        data <- List.empty
        if createTable() = Success 
            then
                try
                    let cn = new SQLiteConnection(connectionString)
                    cn.Open()

                    let sql = selectQuery ()
                    let cmd = new SQLiteCommand(sql, cn)
                    let result = cmd.ExecuteReader()
                    
                    while result.Read() do
                        data <- fillData (result.["_id"].ToString() |>int) (result.["name"].ToString()) (result.["price"].ToString() |>int) (result.["rent"].ToString() |>int) data
                        printfn "ID: %i, Name: %s, Price: %i, Rent: %i" (result.["_id"].ToString() |>int) (result.["name"].ToString()) (result.["price"].ToString() |>int) (result.["rent"].ToString() |>int)

                    cn.Close()
                    
                    if data.Length = 0 then Failure
                    else Success
                with 
                    | _ -> Failure

            else Failure 
    
    let checkLogin login password =
        userType <- "empty"
        try
            let cn = new SQLiteConnection(connectionString)
            cn.Open()

            let sql = selectLoginQuery login password
            let cmd = new SQLiteCommand(sql, cn)
            let result = cmd.ExecuteReader()          

            while result.Read() do
                userType <- result.["group"].ToString()

            cn.Close()
        with 
            | _ -> ()

    let updateQuery (tQuery:Tquery) = 
        if createTable() = Success 
            then
                try
                    let cn = new SQLiteConnection(connectionString)
                    cn.Open()

                    let sql = tQuery.ToExpression()

                    let cmd = new SQLiteCommand(sql, cn)
                    let result = cmd.ExecuteNonQuery()
        
                    cn.Close()
                    
                    if result = -1 then Failure
                    else Success
                with 
                    | _ -> Failure

            else Failure 