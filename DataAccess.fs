module DataAccess

    open Database
    open System

    let chosenDb = Oracle

    let cnn = ``open`` chosenDb

    let inline private queryP<'a, 'b> = query<'a, 'b> cnn
    let inline private query<'a> cmd = queryP<'a, obj> cmd None
    let inline private executeP cmd param = execute cnn cmd param
    let inline private execute cmd = executeP cmd None

    open Models
    open Parameters

    let mutable updateState = {MsgId = 0; SourceId = 0; MsgSourceId = 0;}

    let rparam (sql:string) = match chosenDb with | Oracle -> sql.Replace('@', ':') | _ -> sql

    let insertMessage (msg:Message) =
        updateState.MsgId <- updateState.MsgId + 1
        executeP (rparam "INSERT INTO Messages VALUES (@Id, @Time, @Msg, @Level)") {msg with Id = updateState.MsgId} |> ignore

        match msg.Source with
        | null -> ()
        | src -> 
            let mutable id = queryP<int, SourceParam> (rparam "SELECT id FROM Sources WHERE source = @Src") {Id = 0;Src = src} |> Seq.first
            if Option.isNone id then
                updateState.SourceId <- updateState.SourceId + 1
                id <- Some (updateState.SourceId)
                executeP (rparam "INSERT INTO Sources VALUES (@Id, @Src)") {Id = id.Value; Src = src} |> ignore
            
            updateState.MsgSourceId <- updateState.MsgSourceId + 1
            executeP (rparam "INSERT INTO MessageSources VALUES (@Id, @MsgId, @SourceId)") 
                {Id = updateState.MsgSourceId; MsgId = updateState.MsgId; SourceId = updateState.SourceId} |> ignore

    let resetDatabase () = 
        execute "DELETE FROM MessageSources" |> ignore
        execute "DELETE FROM Sources" |> ignore
        execute "DELETE FROM Messages" |> ignore
        updateState <- {MsgId = 0; SourceId = 0; MsgSourceId = 0;}

    let private composeQuery condition = 
        "SELECT Messages.id, time, msg, level, source
         FROM Messages
         LEFT JOIN MessageSources ON (Messages.id = MessageSources.msg_id)
         LEFT JOIN Sources ON (Sources.id = MessageSources.source_id)
         " + condition + "
         ORDER BY Messages.id"

    let getAllMessages () = 
        let queryString = composeQuery ""
        query<Message> queryString

    let getMessagesWithLevel l =
        let queryString = composeQuery (rparam "WHERE level = @Level")
        queryP<Message, LevelParam> queryString {Level = l} 

    let getMessagesWithLevelGreaterThan l =
        let queryString = composeQuery (rparam "WHERE level > @Level")
        queryP<Message, LevelParam> queryString {Level = l}

    let getAllSources () =
        let queryString = "SELECT source FROM Sources"
        query<string> queryString

    let getMessagesFromSource src =
        let queryString = composeQuery (rparam "WHERE source = @Src")
        queryP<Message, SourceParam> queryString {Src = src; Id = 0}

    let getKernellVersion () =
        let queryString = composeQuery "WHERE msg like \"Linux version%\""
        let msg = query<Message> queryString |> Seq.head
        match msg.Msg with
        | Parser.Regex "Linux version ([0-9\.-]+) .*" [ver] ->
            {Version = ver}
        | _ -> {Version = "N/A"}

    let getStatistics () =
        let msgs = query<int> "SELECT COUNT(*) FROM Messages" |> Seq.head
        let prwc = query<PriorityWithCount> "SELECT level as Priority, COUNT(*) as Count FROM Messages GROUP BY level"
        let sources = query<int> "SELECT COUNT(*) FROM Sources" |> Seq.head
        let swmm = query<string> "SELECT source FROM Sources 
                                  JOIN MessageSources ON (Sources.id = MessageSources.source_id)
                                  GROUP BY source
                                  ORDER BY COUNT(*) DESC" |> Seq.head
        let swme =
            let s = query<string> "SELECT source FROM Sources
                                  JOIN MessageSources ON (Sources.id = MessageSources.source_id)
                                  LEFT JOIN Messages ON (Messages.id = MessageSources.msg_id)
                                  WHERE Messages.level > 4
                                  GROUP BY source
                                  ORDER BY COUNT(*) DESC"
            if Seq.isEmpty s then null else s |> Seq.head
        {MsgsByPriority = List.ofSeq prwc; Sources = sources; SourceWithMostMsgs = swmm; SourceWithMostErrors = swme; KernelVersion = getKernellVersion(); Msgs = msgs}
