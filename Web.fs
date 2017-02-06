module Web

    open DataAccess

    open Suave
    open Suave.Successful
    open Suave.Operators
    open Suave.Filters
    open Suave.RequestErrors

    DotLiquid.setTemplatesDir "./templates"

    type Result<'a, 'b> = Success of 'a | Failure of 'b

    module Home =
        let index = DotLiquid.page "home/index.html" []

    module Parse =
        let index error = DotLiquid.page "parse/index.html" error
        
        open System.IO
        
        let post =
            let ``process`` httpRequest =
                use reader = File.OpenText httpRequest.files.Head.tempFilePath
                let rec readAndParse list (reader:StreamReader) i =
                    match reader.EndOfStream with
                    | true -> Success list
                    | false -> 
                        let msg = reader.ReadLine() |> Parser.parseLine
                        if msg.IsNone then Failure i
                        else (*(if msg.Value.Source <> null && msg.Value.Source.Length > 20 then printfn "%s" msg.Value.Source else ());*) readAndParse (msg::list) reader (i+1)
                match readAndParse [] reader 1 with
                | Success list -> 
                    DataAccess.resetDatabase()
                    List.rev list |> List.iter (function (Some msg) -> DataAccess.insertMessage msg | None -> ())
                    Redirection.redirect "/dashboard"
                | Failure line ->
                    index (Some (sprintf "Parsing error at line %d" line))
            request ``process``
                
    module Dashboard =
        do DotLiquid.Template.RegisterFilter(Filters.Filter().GetType())
        type BrowseModel = {columns:string[]; data: Models.Message seq}
        let index = request (fun _ -> if messageCount() > 0m then DotLiquid.page "dashboard/index.html" (DataAccess.getStatistics()) else Redirection.redirect "/parse")

        let browse = request (fun req -> 
                                DotLiquid.page "dashboard/browse.html" 
                                  { columns = [|"Time"; "Source"; "Message"; "Priority"|]; data = DataAccess.getAllMessages()})

        let filter = choose [
                        path "/dashboard" >=> index
                        path "/dashboard/browse" >=> browse
                        ]

    let mainApp = 
        choose [
                path "/" >=> Home.index
                path "/parse" >=> choose [ GET >=> Parse.index None; POST >=> Parse.post ]
                Dashboard.filter
                Files.browseHome
               ]