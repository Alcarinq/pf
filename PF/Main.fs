namespace Main
open System
open System.Data
open System.Windows.Forms
open System.Drawing
open System.IO
open Microsoft.FSharp.Reflection
open System.Runtime.InteropServices

open Database

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

    let dataTableToRecords (table:DataGridViewRowCollection) : 'T list =
      //[ for row in table.Rows ->
      //    let values = [| for fld in fields -> row.[fld.Name] |]
      //    FSharpValue.MakeRecord(typeof<'T>, values) :?> 'T ]
      Database.myTest

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
        temp
    
    let buttonSave =
        let temp = new Button()
        do temp.Text <- "Zapisz wiersz"
        do temp.Location <- new Point(105,50)
        do temp.Size <- new Size(100,25)
        temp

    let buttonReload =
        let temp = new Button()
        do temp.Text <- "Przeładuj"
        do temp.Location <- new Point(25,50)
        temp

    let mainForm =
        let temp = new Form()
        do temp.ClientSize <- new Size(800, 600)
        do temp.Controls.Add(label)
        do temp.Controls.Add(buttonShow)
        do temp.Text <- "PF projekt"
        temp

    let dataGrid =
        let temp = new DataGridView()
        //do temp.Dock <- DockStyle.Fill
        do temp.Location <- new Point(20,100)
        do temp.Size <- new Size(760,480)
        do temp.AutoSizeColumnsMode <- DataGridViewAutoSizeColumnsMode.AllCells
        do temp.SelectionMode <- DataGridViewSelectionMode.FullRowSelect
        do temp.MultiSelect <- false
        // do temp.DataError += new DataGridViewDataErrorEventHandler
        // do temp.DataSource <- recordsToDataTable(Database.myTest)
        temp

    dataGrid.DataError.Add(fun event ->
        MessageBox.Show("Proszę użyć poprawnego typu dla wartości w wybranej kolumnie.","Błąd") |> ignore 
    )

    buttonShow.Click.Add(fun _ ->
        mainForm.Controls.Remove(buttonShow)
        mainForm.Controls.Add(dataGrid)
        mainForm.Controls.Add(buttonReload)
        mainForm.Controls.Add(buttonSave)
        dataGrid.Columns.["Id"].ReadOnly <- true )

    buttonReload.Click.Add(fun _ ->
        Database.select |> ignore
        dataGrid.DataSource <- recordsToDataTable(Database.myTest) )

    buttonSave.Click.Add(fun _ ->
        do Database.insert (dataGrid.SelectedRows.[0].Cells.[1].Value.ToString()) (dataGrid.SelectedRows.[0].Cells.[2].Value.ToString()|>int) |>ignore
        do Database.select |> ignore
        dataGrid.DataSource <- recordsToDataTable(Database.myTest)
        )

    [<EntryPoint>]
    [<STAThread>]
    let main argv =  
        //FreeConsole() |>ignore
        Application.EnableVisualStyles()

        //Database.insert "Narty" 50 
        Database.select |> ignore
        dataGrid.DataSource <- recordsToDataTable(Database.myTest)
        
        Application.Run(mainForm)
        0