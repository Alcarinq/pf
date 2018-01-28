namespace Rent
open System.Data
open System.Windows.Forms
open System.Drawing

open Database

module Rent = 
    let mutable itemId = 0
    let mutable itemEndPrice = 0
    let buttonRent =
        let temp = new Button()
        do temp.Text <- "OK"
        do temp.Location <- new Point(200,200)
        do temp.Size <- new Size(80,25)
        temp

    let labelName =
        let temp = new Label()
        do temp.Size <- new System.Drawing.Size(100,100)
        do temp.Text <- "Nazwa: "
        do temp.AutoSize <- true
        do temp.Location <- new Point(25,55)
        temp

    let inputName =
        let temp = new TextBox()
        do temp.Location <- new Point(110,55)
        do temp.Size <- new Size(150,25)
        do temp.Multiline <- false
        temp

    let labelDays =
        let temp = new Label()
        do temp.Size <- new System.Drawing.Size(100,100)
        do temp.Text <- "Liczba dni: "
        do temp.AutoSize <- true
        do temp.Location <- new Point(25,95)
        temp

    let listDays =
        let temp = new ComboBox()
        do temp.Location <- new Point(110,95)
        do temp.Size <- new Size(150,25)
        do temp.DropDownStyle <- ComboBoxStyle.DropDownList
        do temp.BeginUpdate()
        let seq = seq { for i in 1 .. 10 -> i }
        for s in seq do
            temp.Items.Add(s) |>ignore
        do temp.EndUpdate()
        do temp.SelectedIndex <- 0
        temp

    let labelPrice =
        let temp = new Label()
        do temp.Size <- new System.Drawing.Size(100,100)
        do temp.AutoSize <- true
        do temp.Text <- "Koszt wypożyczenia: "
        do temp.Location <- new Point(25,135)
        temp

    listDays.SelectedValueChanged.Add(fun _ ->
        Database.selectGetPrice itemId |>ignore
        itemEndPrice <- (Database.itemPrice * (listDays.SelectedItem.ToString()|>int))
        labelPrice.Text <- sprintf @"Koszt wypożyczenia: %d" itemEndPrice

        printf "%s" (listDays.SelectedItem.ToString())
        printf "%d" Database.itemPrice
    )

    let RentForm =
        let temp = new Form()
        do temp.ClientSize <- new Size(350, 250)
        do temp.Text <- "Magazyn sprzętu narciarskiego"
        do temp.StartPosition <- FormStartPosition.CenterScreen
        do temp.Controls.Add(buttonRent)
        do temp.Controls.Add(labelName)
        do temp.Controls.Add(inputName)
        do temp.Controls.Add(labelDays)
        do temp.Controls.Add(listDays)
        do temp.Controls.Add(labelPrice)
        temp