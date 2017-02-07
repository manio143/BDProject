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
        let index (error:'a option) = DotLiquid.page "parse/index.html" error
        
        open System.IO
        
        let post =
            let ``process`` httpRequest =
                if httpRequest.files |> List.isEmpty then index (Some "Pusty plik")
                else
                use reader = File.OpenText httpRequest.files.Head.tempFilePath
                let rec readAndParse list (reader:StreamReader) i f first =
                    match reader.EndOfStream with
                    | true -> if f < i/5 then Success list else Failure first
                    | false -> 
                        let msg = reader.ReadLine() |> Parser.parseLine
                        if msg.IsNone then readAndParse list reader (i+1) (f+1) (min first i)
                        else readAndParse (msg::list) reader (i+1) f first
                match readAndParse [] reader 1 0 System.Int32.MaxValue with
                | Success list -> 
                    DataAccess.resetDatabase()
                    List.rev list |> List.iter (function (Some msg) -> DataAccess.insertMessage msg | None -> ())
                    Redirection.redirect "/dashboard"
                | Failure line ->
                    index (Some (sprintf "Parsing error at line %d" line))
            request ``process``
                
    module Dashboard =
        open Models

        type BrowseModel<'a> = {columns:string[]; data: 'a seq}

        let index = request (fun _ -> if messageCount() > 0m then DotLiquid.page "dashboard/index.html" (DataAccess.getStatistics()) else Redirection.redirect "/parse")

        let browseG<'a> getData = request (fun req ->
                                DotLiquid.page "dashboard/browse.html" 
                                  { columns = [|"Time"; "Source"; "Message"; "Priority"|]; data = getData()})
        let sanitize = Seq.map (fun m -> 
                                {m with 
                                    Source = if isNull m.Source then "" else m.Source
                                    Priority = if not m.Priority.HasValue then System.Nullable<int>(-1) else m.Priority})
        let browse = browseG(fun () -> DataAccess.getAllMessages() |> sanitize)
        let browseLevel l = browseG(fun () -> DataAccess.getMessagesWithPriority(l) |> sanitize)
        let browseSource s = browseG(fun () -> DataAccess.getMessagesFromSource(s) |> sanitize)

        let sources = request ( fun req -> 
                                    DotLiquid.page "dashboard/browse.html"
                                        { columns = [|"Source"; "Count"|]; data = DataAccess.getAllSources() |> Seq.map (fun s->(s:>DotLiquid.ILiquidizable).ToLiquid() :?> SourceR)})

        let filter = choose [
                        path "/dashboard" >=> index
                        path "/dashboard/browse" >=> browse
                        path "/dashboard/browse/sources" >=> sources
                        pathScan "/dashboard/browse/level/%d" browseLevel
                        pathScan "/dashboard/browse/source/%s" browseSource
                        ]

    let mainApp = 
        choose [
                path "/" >=> Home.index
                path "/parse" >=> choose [ GET >=> Parse.index None; POST >=> Parse.post ]
                Dashboard.filter
                Files.browseHome
               ]
    