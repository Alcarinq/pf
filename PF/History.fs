namespace History
open System.Data
open System.Windows.Forms
open System.Drawing

open Database

module History = 

    let historyGrid =
        let temp = new DataGridView()
        do temp.Location <- new Point(20,100)
        do temp.AutoSizeColumnsMode <- DataGridViewAutoSizeColumnsMode.Fill
        do temp.SelectionMode <- DataGridViewSelectionMode.FullRowSelect
        do temp.Dock <- DockStyle.Fill
        do temp.ReadOnly <- true
        temp    

    let HistoryForm =
        let temp = new Form()
        do temp.ClientSize <- new Size(600, 400)
        do temp.Text <- "Historia przedmiotu"
        do temp.StartPosition <- FormStartPosition.CenterScreen
        do temp.Controls.Add(historyGrid)
        temp