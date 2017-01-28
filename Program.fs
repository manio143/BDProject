open Web
open Suave

[<EntryPoint>]
let main argv = 
    // DataAccess.resetDatabase()
    // DataAccess.insertMessage {Id = 0; Msg = "Kernel start"; Time = 0.0001m; Level = nullable 4; Source = null}
    // DataAccess.insertMessage {Id = 0; Msg = "Hello World!"; Time = 0.0002m; Level = nullable 4; Source = "myApp"}
    // printfn "Test: %A" (DataAccess.getAllMessages())
    // while true do
    //     match System.Console.ReadLine() |> Parser.parseLine with
    //     | Some x -> x |> DataAccess.insertMessage
    //     | None -> ()
    // done
    startWebServer {defaultConfig with maxContentLength = 100000000} Web.mainApp
    0 // return an integer exit code

