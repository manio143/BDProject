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

    let mutable updateState = {MessageId = 0; SourceId = 0; MessageSourceId = 0;}

    let insertMessage (msg:Message) =
        updateState.MessageId <- updateState.MessageId + 1
        executeP INSERT_MESSAGE (asMessageParam {msg with Id = updateState.MessageId}) |> ignore

        match msg.Source with
        | null -> ()
        | src -> 
            let mutable id = queryP<int, SourceParam> SELECT_SOURCE_ID {Id = 0;Src = src} |> Seq.first
            if Option.isNone id then
                updateState.SourceId <- updateState.SourceId + 1
                id <- Some (updateState.SourceId)
                executeP INSERT_SOURCE {Id = id.Value; Src = src} |> ignore
            
            updateState.MessageSourceId <- updateState.MessageSourceId + 1
            executeP INSERT_MESSAGE_SOURCE 
                {Id = updateState.MessageSourceId; MessageId = updateState.MessageId; SourceId = updateState.SourceId} |> ignore

    let resetDatabase () = 
        execute DELETE_MESSAGE_SOURCES |> ignore
        execute DELETE_SOURCES |> ignore
        execute DELETE_MESSAGES |> ignore
        updateState <- {MessageId = 0; SourceId = 0; MessageSourceId = 0;}

    let getAllMessages () = 
        query<Message> (SELECT_MESSAGES + ORDERED)

    let getMessagesWithPriority l =
        queryP<Message, PriorityParam> (SELECT_MESSAGES_WHERE_LEVEL "=") {PPriority = l} 

    let getMessagesWithPriorityGreaterThan l =
        queryP<Message, PriorityParam> (SELECT_MESSAGES_WHERE_LEVEL ">") {PPriority = l}

    let getAllSources () =
        query<Source> SELECT_SOURCES

    let getMessagesFromSource src =
        queryP<Message, SourceParam> SELECT_MESSAGES_WHERE_SOURCE {Src = src; Id = 0}

    let getKernellVersion () =
        let msg = query<Message> SELECT_KERNEL_VERSION |> Seq.head
        match msg.Message with
        | Parser.Regex "Linux version ([0-9\.-]+) .*" [ver] ->
            {Version = ver}
        | _ -> {Version = "N/A"}

    let messageCount() = query<decimal> SELECT_NUMBER_OF_MESSAGES |> Seq.head
    let getStatistics () =
        let msgs = messageCount()
        let prwc = query<PriorityWithCount> SELECT_NUMBER_OF_MESSAGES_BY_PRIORITY
        let sources = query<decimal> SELECT_NUMBER_OF_SOURCES |> Seq.head
        let swmm = query<string> SELECT_SOURCES_ORDERED_BY_MESSAGE_COUNT |> Seq.head
        let swme =
            let s = query<string> SELECT_SOURCES_WITH_ERRORS_ORDERED_BY_MESSAGE_COUNT
            if Seq.isEmpty s then null else s |> Seq.head
        {MessagesByPriority = List.ofSeq prwc; Sources = sources; SourceWithMostMessages = swmm; SourceWithMostErrors = swme; KernelVersion = getKernellVersion(); Messages = msgs}
