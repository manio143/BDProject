open Web
open Suave

let nullable i = new System.Nullable<int>(i)

[<EntryPoint>]
let main argv = 
    DataAccess.resetDatabase()
    DataAccess.insertMessage {Id = 0; Msg = "Kernel start"; Time = 0.0001m; Level = nullable 4; Source = null}
    DataAccess.insertMessage {Id = 0; Msg = "Hello World!"; Time = 0.0002m; Level = nullable 4; Source = "myApp"}
    printfn "Test: %A" (DataAccess.getAllMessages())
    startWebServer defaultConfig Web.mainApp
    0 // return an integer exit code

