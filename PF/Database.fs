namespace Database
open System.Data.SQLite
open System

module Database =
    type Test = {Id: int; Name: string; Price: int; Rent: bool}
    type TestHistory = {Id: int; IdItem: int; IdRentUser: int; UserName: string; RentDays: int; EndPrice: int; Name:string; Price:int; Login:string}

    let mutable data = []
    let mutable userId = 0
    let mutable userType = "empty"
    let mutable itemPrice = 0
    let mutable historyData = []
    let fillData (_id: int) (name: string) (price: int) (rent: int) list1=
        List.append list1 [{Test.Id=_id; Test.Name=name; Test.Price=price; Test.Rent=if rent = 1 then true else false}]
    let fillHistoryData (_id: int)  (_idItem:int) (_idRentUser:int) (userName:string) (rentDays:int) (endPrice:int) (name:string) (price:int) (login:string) list1=
        List.append list1 [{TestHistory.Id=_id; TestHistory.IdItem=_idItem; TestHistory.IdRentUser=_idRentUser; TestHistory.UserName=userName; TestHistory.RentDays=rentDays; TestHistory.EndPrice=endPrice; TestHistory.Name=name; TestHistory.Price=price; TestHistory.Login=login}]

    type Result = Success | Failure  
    let databaseName = "../database.db"
    let connectionString = sprintf "Data Source=%s;Version=3" databaseName

    let tableStuff = @"CREATE TABLE IF NOT EXISTS `STUFF` (
	                    `_id`	INTEGER PRIMARY KEY AUTOINCREMENT,
	                    `Name`	TEXT,
	                    `Price`	INTEGER,
	                    `Rent`	INTEGER DEFAULT 0
                    )"

    let tableUsers = @"CREATE TABLE IF NOT EXISTS `USERS` (
	                    `_id`	INTEGER PRIMARY KEY AUTOINCREMENT,
	                    `Login`	TEXT,
	                    `Password`	TEXT,
	                    `UserGroup`	TEXT
                    )"

    let tableRental = @"CREATE TABLE IF NOT EXISTS `RENTAL` (
	                    `_id`	INTEGER PRIMARY KEY AUTOINCREMENT,
	                    `_idItem`	INTEGER,
	                    `_idRentUser`	INTEGER,
	                    `Username`	TEXT,
	                    `RentDays`	INTEGER,
	                    `EndPrice`	INTEGER,
	                    FOREIGN KEY(`_idRentUser`) REFERENCES `USERS`(`_id`),
	                    FOREIGN KEY(`_idItem`) REFERENCES `STUFF`(`_id`)
                    )"

    let private createTable() = 
        try
            let cn = new SQLiteConnection(connectionString)
            cn.Open()

            let createTableStuff = tableStuff
            let createTableUsers = tableUsers
            let createTableRental = tableRental

            let cmd1 = new SQLiteCommand(createTableStuff, cn)
            let result1 = cmd1.ExecuteNonQuery()
            let cmd2 = new SQLiteCommand(createTableUsers, cn)
            let result2 = cmd2.ExecuteNonQuery()
            let cmd3 = new SQLiteCommand(createTableRental, cn)
            let result3 = cmd3.ExecuteNonQuery()

            let usersQuery = sprintf @"INSERT INTO USERS (Login, Password, UserGroup) values ('admin', 'admin', 'admin'), ('guest1', 'guest1', 'user'), ('guest2', 'guest2', 'user')"
            let cmd4 = new SQLiteCommand(usersQuery, cn)
            let result4 = cmd4.ExecuteNonQuery()

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

    let selectGetPriceQuery id = 
        sprintf @"SELECT Price FROM STUFF where _id = %d" id

    let selectHistoryQuery itemId =
        sprintf @"SELECT r._id, r._idItem, r._idRentUser, r.UserName, r.RentDays, r.EndPrice, s.Name, s.Price, u.Login FROM RENTAL r, STUFF s, USERS u where r._idItem = %d and r._idItem = s._id and r._idRentUser = u._id" itemId
        

    let insertItemQuery name price = 
        sprintf @"INSERT INTO STUFF (Name, Price) values ('%s', '%d')" name price 

    let updateItemQuery id name price rent = 
        sprintf @"UPDATE STUFF SET Name = '%s', Price = %d, Rent = %d WHERE _id = %d" name price rent id

    let deleteQuery id = 
        sprintf @"DELETE FROM STUFF WHERE _id = %d" id 
 
    let removeRentQuery id = 
        sprintf @"UPDATE STUFF SET Rent = 0 WHERE _id = %d" id 

    let rentItemQuery id = 
        sprintf @"UPDATE STUFF SET Rent = 1 WHERE _id = %d" id 

    let insertRentalQuery _idItem _idRentUser userName rentDays endPrice = 
        sprintf @"INSERT INTO RENTAL (_idItem, _idRentUser, Username, RentDays,EndPrice) values ('%d', '%d', '%s', '%d', '%d')" _idItem _idRentUser userName rentDays endPrice

    type Tquery (qType:string, ?id:int, ?name:string, ?price:int, ?rent:int, ?_idItem:int, ?_idRentUser:int, ?userName:string, ?rentDays:int, ?endPrice:int) =
      let id = defaultArg id 0
      let name = defaultArg name ""
      let price = defaultArg price 0
      let rent = defaultArg rent 0
      let _idItem = defaultArg _idItem 0
      let _idRentUser = defaultArg _idRentUser 0
      let userName = defaultArg userName ""
      let rentDays = defaultArg rentDays 0
      let endPrice = defaultArg endPrice 0

      member this.ToExpression() =  
        match qType with
          | "insertItemQuery" -> insertItemQuery name price
          | "updateItemQuery" -> updateItemQuery id name price rent
          | "deleteQuery" -> deleteQuery id
          | "removeRentQuery" -> removeRentQuery id
          | "rentItemQuery" -> rentItemQuery id
          | "insertRentalQuery" -> insertRentalQuery _idItem _idRentUser userName rentDays endPrice
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
    
    let selectGetPrice id =
        if createTable() = Success 
            then
                try
                    let cn = new SQLiteConnection(connectionString)
                    cn.Open()

                    let sql = selectGetPriceQuery id
                    let cmd = new SQLiteCommand(sql, cn)
                    let result = cmd.ExecuteReader()
                    
                    while result.Read() do
                        itemPrice <- (result.["price"].ToString()|>int)
                        printfn "itemPrice: %d" (result.["price"].ToString()|>int)

                    cn.Close()
                    
                    if data.Length = 0 then Failure
                    else Success
                with 
                    | _ -> Failure

            else Failure 

    let selectHistory itemId =
        historyData <- List.empty
        if createTable() = Success 
            then
                try
                    let cn = new SQLiteConnection(connectionString)
                    cn.Open()

                    let sql = selectHistoryQuery itemId
                    let cmd = new SQLiteCommand(sql, cn)
                    let result = cmd.ExecuteReader()
                    
                    while result.Read() do
                        historyData <- fillHistoryData (result.["_id"].ToString() |>int) (result.["_idItem"].ToString()|>int) (result.["_idRentUser"].ToString() |>int) (result.["UserName"].ToString()) (result.["RentDays"].ToString() |>int) (result.["EndPrice"].ToString() |>int) (result.["Name"].ToString()) (result.["Price"].ToString() |>int) (result.["Login"].ToString() ) historyData

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
                userType <- result.["UserGroup"].ToString()
                userId <- result.["_id"].ToString()|>int

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