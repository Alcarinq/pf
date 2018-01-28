namespace Main
open System
open System.Data
open System.Windows.Forms
open System.Drawing
open System.IO
open Microsoft.FSharp.Reflection
open System.Runtime.InteropServices

open Database
open Login

module Main = 
    [<DllImport("kernel32.dll")>] extern bool FreeConsole()

    let recordsToDataTable (items:'T list) =
      let dataTable = new DataTable() 
      let fields = FSharpType.GetRecordFields(typeof<'T>)
      for fld in fields do
        dataTable.Columns.Add(fld.Name, fld.PropertyType) |> ignore
      for itm in items do
        let row = dataTable.NewRow()
        for fld in fields do
          row.[fld.Name] <- fld.GetValue(itm)
        dataTable.Rows.Add(row)
      dataTable

    let buttonSizeX = 100
    let buttonSizeY = 45 

    let label =
        let temp = new Label()
        do temp.Size <- new System.Drawing.Size(100,100)
        do temp.Text <- "Magazyn sprzętu narciarskiego"
        do temp.AutoSize <- true
        do temp.Location <- new Point(25,25)
        temp

    let buttonShow =
        let temp = new Button()
        do temp.Text <- "Pokaż"
        do temp.Location <- new Point(25,50)
        do temp.Size <- new Size(buttonSizeX,buttonSizeY)
        temp
    
    let buttonSave =
        let temp = new Button()
        do temp.Text <- "Zapisz wiersz"
        do temp.Location <- new Point(120,50)
        do temp.Size <- new Size(buttonSizeX,buttonSizeY)
        temp

    let buttonRemove =
        let temp = new Button()
        do temp.Text <- "Usuń wiersz"
        do temp.Location <- new Point(220,50)
        do temp.Size <- new Size(buttonSizeX,buttonSizeY)
        temp

    let buttonReload =
        let temp = new Button()
        do temp.Text <- "Przeładuj"
        do temp.Location <- new Point(20,50)
        do temp.Size <- new Size(buttonSizeX,buttonSizeY)
        temp

    let buttonRent =
        let temp = new Button()
        do temp.Text <- "Wypożycz"
        do temp.Location <- new Point(120,50)
        do temp.Size <- new Size(buttonSizeX,buttonSizeY)
        temp

    let buttonRemoveRent =
        let temp = new Button()
        do temp.Text <- "Usuń wypożyczenie"
        do temp.Location <- new Point(320,50)
        do temp.Size <- new Size(buttonSizeX,buttonSizeY)
        temp

    let mainForm =
        let temp = new Form()
        do temp.ClientSize <- new Size(800, 600)
        do temp.Controls.Add(label)
        do temp.Controls.Add(buttonShow)
        do temp.Text <- "Magazyn sprzętu narciarskiego"
        do temp.StartPosition <- FormStartPosition.CenterScreen
        temp

    let dataGrid =
        let temp = new DataGridView()
        do temp.Location <- new Point(20,100)
        do temp.Size <- new Size(760,480)
        do temp.AutoSizeColumnsMode <- DataGridViewAutoSizeColumnsMode.AllCells
        do temp.SelectionMode <- DataGridViewSelectionMode.FullRowSelect
        do temp.MultiSelect <- false
        temp

    dataGrid.DataError.Add(fun event ->
        MessageBox.Show("Proszę użyć poprawnego typu dla wartości w wybranej kolumnie.","Błąd") |> ignore 
    )

    let reloadData () =
        dataGrid.DataSource <- null
        dataGrid.Rows.Clear |> ignore
        Database.select () |> ignore
        dataGrid.DataSource <- recordsToDataTable(Database.data)
        if Database.userType = "admin" then
            mainForm.Controls.Add(buttonSave)
            mainForm.Controls.Add(buttonRemove)
            mainForm.Controls.Add(buttonRemoveRent)
        if Database.userType = "user" then
            mainForm.Controls.Add(buttonRent)
            dataGrid.Columns.["Rent"].Visible <- false
            dataGrid.Columns.["Name"].ReadOnly <- true
            dataGrid.Columns.["Price"].ReadOnly <- true
        dataGrid.Columns.["Id"].ReadOnly <- true
        dataGrid.Columns.["Rent"].ReadOnly <- true            

    buttonShow.Click.Add(fun _ ->
        mainForm.Controls.Remove(buttonShow)
        mainForm.Controls.Add(dataGrid)
        mainForm.Controls.Add(buttonReload)
        reloadData ()
        )

    buttonReload.Click.Add(fun _ ->
        reloadData ()
        )

    buttonSave.Click.Add(fun _ ->
        if dataGrid.SelectedRows.[0].Cells.[0].Value <> null then
            let query =
                if (String.IsNullOrEmpty(dataGrid.SelectedRows.[0].Cells.[0].Value.ToString())) then
                    Database.Tquery(qType="insertItemQuery", name=(dataGrid.SelectedRows.[0].Cells.[1].Value.ToString()), price=(dataGrid.SelectedRows.[0].Cells.[2].Value.ToString()|>int) )
                else
                    Database.Tquery(qType="updateItemQuery", 
                                    id=(dataGrid.SelectedRows.[0].Cells.[0].Value.ToString()|>int),
                                    name=(dataGrid.SelectedRows.[0].Cells.[1].Value.ToString()), 
                                    price=(dataGrid.SelectedRows.[0].Cells.[2].Value.ToString()|>int),
                                    rent=Convert.ToInt32((dataGrid.SelectedRows.[0].Cells.[3].Value)) )
                    
                    
            Database.updateQuery query |> ignore 
            reloadData ()
        )

    buttonRemove.Click.Add(fun _ ->
        if dataGrid.SelectedRows.[0].Cells.[0].Value <> null then
            let query = Database.Tquery(qType="deleteQuery", id=(dataGrid.SelectedRows.[0].Cells.[0].Value.ToString() |>int) )
            Database.updateQuery query |> ignore 
            reloadData ()
        )

    buttonRemoveRent.Click.Add(fun _ ->
        if dataGrid.SelectedRows.[0].Cells.[0].Value <> null then
            let query = Database.Tquery(qType="removeRentQuery", id=(dataGrid.SelectedRows.[0].Cells.[0].Value.ToString() |>int) )
            Database.updateQuery query |> ignore 
            reloadData ()
        )

    buttonRent.Click.Add(fun _ ->
        if dataGrid.SelectedRows.Count > 0 && dataGrid.SelectedRows.[0].Cells.[0].Value <> null then
            let query = Database.Tquery(qType="rentItem", id=(dataGrid.SelectedRows.[0].Cells.[0].Value.ToString() |>int) )
            Database.updateQuery query |> ignore 
            reloadData ()
        )

    let keyPressedLogin (e : KeyEventArgs) = 
        match e with
        | e when e.KeyCode = Keys.Enter -> Login.buttonLogin.PerformClick()
        | _  -> ()

    Login.inputLogin.KeyDown.AddHandler(KeyEventHandler (fun _ e -> keyPressedLogin e ))
    Login.inputPassword.KeyDown.AddHandler(KeyEventHandler (fun _ e -> keyPressedLogin e ))
    Login.buttonLogin.Click.Add(fun _ ->
        Database.checkLogin Login.inputLogin.Text Login.inputPassword.Text
        printf "Logged as: %s\n" (Database.userType)
        if Database.userType = "admin" || Database.userType = "user" then
            Login.loginForm.Hide()
            mainForm.Text <- sprintf "%s. Zalogowano jako: %s" (mainForm.Text.ToString()) (Database.userType)
            mainForm.Show()
            
        else
            MessageBox.Show("Błędny login/hasło lub użytkownik nie istnieje w systemie.","Błąd") |> ignore
        )    

    [<EntryPoint>]
    [<STAThread>]
    let main argv =  
        //FreeConsole() |>ignore
        Application.EnableVisualStyles()       
        Application.Run(Login.loginForm)
        0