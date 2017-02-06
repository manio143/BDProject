open Web
open Suave
open DotLiquid

[<EntryPoint>]
let main argv = 
    do Template.RegisterFilter(Filters.Filter().GetType())

    startWebServer defaultConfig Web.mainApp
    0 // return an integer exit code

