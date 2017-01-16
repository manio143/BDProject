open Web
open Suave

[<EntryPoint>]
let main argv = 
    DataAccess.test()
    startWebServer defaultConfig Web.mainApp
    0 // return an integer exit code

