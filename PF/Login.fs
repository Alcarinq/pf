namespace Login
open System.Data
open System.Windows.Forms
open System.Drawing

open Database

module Login = 
    let buttonLogin =
        let temp = new Button()
        do temp.Text <- "Zaloguj"
        do temp.Location <- new Point(150,100)
        do temp.Size <- new Size(80,25)
        temp
    
    let labelLogin =
        let temp = new Label()
        do temp.Size <- new System.Drawing.Size(100,100)
        do temp.Text <- "Login: "
        do temp.AutoSize <- true
        do temp.Location <- new Point(25,25)
        temp

    let inputLogin =
        let temp = new RichTextBox()
        do temp.Location <- new Point(80,20)
        do temp.Size <- new Size(150,25)
        do temp.Multiline <- false
        temp

    let labelPassword =
        let temp = new Label()
        do temp.Size <- new System.Drawing.Size(100,100)
        do temp.Text <- "Hasło: "
        do temp.AutoSize <- true
        do temp.Location <- new Point(25,55)
        temp

    let inputPassword =
        let temp = new RichTextBox()
        do temp.Location <- new Point(80,55)
        do temp.Size <- new Size(150,25)
        do temp.Multiline <- false
        temp

    let loginForm =
        let temp = new Form()
        do temp.ClientSize <- new Size(300, 150)
        do temp.Text <- "Magazyn sprzętu narciarskiego"
        do temp.StartPosition <- FormStartPosition.CenterScreen
        do temp.Controls.Add(buttonLogin)
        do temp.Controls.Add(labelLogin)
        do temp.Controls.Add(inputLogin)
        do temp.Controls.Add(labelPassword)
        do temp.Controls.Add(inputPassword)
        temp


