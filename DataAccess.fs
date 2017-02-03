module DataAccess

    open Database
    open System

    let cnn = ``open`` chosenDb

    let inline private queryP<'a, 'b> = query<'a, 'b> cnn
    let inline private query<'a> cmd = queryP<'a, obj> cmd None
    let inline private executeP cmd param = execute cnn cmd param
    let inline private execute cmd = executeP cmd None

    open Models
    open Parameters
    open Queries

    let mutable updateState = {MsgId = 0; SourceId = 0; MsgSourceId = 0;}

    let insertMessage (msg:Message) =
        updateState.MsgId <- updateState.MsgId + 1
        executeP INSERT_MESSAGE (asMessageParam {msg with Id = updateState.MsgId}) |> ignore

        match msg.Source with
        | null -> ()
        | src -> 
            let mutable id = queryP<int, SourceParam> SELECT_SOURCE_ID {Id = 0;Src = src} |> Seq.first
            if Option.isNone id then
                updateState.SourceId <- updateState.SourceId + 1
                id <- Some (updateState.SourceId)
                executeP INSERT_SOURCE {Id = id.Value; Src = src} |> ignore
            
            updateState.MsgSourceId <- updateState.MsgSourceId + 1
            executeP INSERT_MESSAGE_SOURCE 
                {Id = updateState.MsgSourceId; MsgId = updateState.MsgId; SourceId = updateState.SourceId} |> ignore

    let resetDatabase () = 
        execute DELETE_MESSAGE_SOURCES |> ignore
        execute DELETE_SOURCES |> ignore
        execute DELETE_MESSAGES |> ignore
        updateState <- {MsgId = 0; SourceId = 0; MsgSourceId = 0;}

    let getAllMessages () = 
        query<Message> (SELECT_MESSAGES + ORDERED)

    let getMessagesWithLevel l =
        queryP<Message, LevelParam> (SELECT_MESSAGES_WHERE_LEVEL "=") {PLevel = l} 

    let getMessagesWithLevelGreaterThan l =
        queryP<Message, LevelParam> (SELECT_MESSAGES_WHERE_LEVEL ">") {PLevel = l}

    let getAllSources () =
        query<string> SELECT_SOURCES

    let getMessagesFromSource src =
        queryP<Message, SourceParam> SELECT_MESSAGES_WHERE_SOURCE {Src = src; Id = 0}

    let getKernellVersion () =
        let msg = query<Message> SELECT_KERNEL_VERSION |> Seq.head
        match msg.Msg with
        | Parser.Regex "Linux version ([0-9\.-]+) .*" [ver] ->
            {Version = ver}
        | _ -> {Version = "N/A"}

    let getStatistics () =
        let msgs = query<decimal> SELECT_NUMBER_OF_MESSAGES |> Seq.head
        let prwc = query<PriorityWithCount> SELECT_NUMBER_OF_MESSAGES_BY_PRIORITY
        let sources = query<decimal> SELECT_NUMBER_OF_SOURCES |> Seq.head
        let swmm = query<string> SELECT_SOURCES_ORDERED_BY_MESSAGE_COUNT |> Seq.head
        let swme =
            let s = query<string> SELECT_SOURCES_WITH_ERRORS_ORDERED_BY_MESSAGE_COUNT
            if Seq.isEmpty s then null else s |> Seq.head
        {MsgsByPriority = List.ofSeq prwc; Sources = sources; SourceWithMostMsgs = swmm; SourceWithMostErrors = swme; KernelVersion = getKernellVersion(); Msgs = msgs}
