module DataAccess

    open Database
    open System

    let cnn = ``open`` MySQL

    let inline private queryP<'a, 'b> = query<'a, 'b> cnn
    let inline private query<'a> cmd = queryP<'a, obj> cmd None
    let inline private executeP cmd param = execute cnn cmd param
    let inline private execute cmd = executeP cmd None

    open Models
    open Parameters

    let mutable updateState = {MsgId = 0; SourceId = 0; MsgSourceId = 0;}

    let insertMessage (msg:Message) =
        updateState.MsgId <- updateState.MsgId + 1
        executeP "INSERT INTO Messages VALUES (@Id, @Time, @Msg, @Level);" {msg with Id = updateState.MsgId} |> ignore

        match msg.Source with
        | null -> ()
        | src -> 
            let mutable id = queryP<int, SourceParam> "SELECT id FROM Sources WHERE source = @Src;" {Id = 0;Src = src} |> Seq.first
            if Option.isNone id then
                updateState.SourceId <- updateState.SourceId + 1
                id <- Some (updateState.SourceId)
                executeP "INSERT INTO Sources VALUES (@Id, @Src);" {Id = id.Value; Src = src} |> ignore
            
            updateState.MsgSourceId <- updateState.MsgSourceId + 1
            executeP "INSERT INTO MessageSources VALUES (@Id, @MsgId, @SourceId)" 
                {Id = updateState.MsgSourceId; MsgId = updateState.MsgId; SourceId = updateState.SourceId} |> ignore

    let resetDatabase () = 
        execute "DELETE FROM MessageSources;" |> ignore
        execute "DELETE FROM Sources;" |> ignore
        execute "DELETE FROM Messages;" |> ignore
        updateState <- {MsgId = 0; SourceId = 0; MsgSourceId = 0;}

    let private composeQuery condition = 
        "SELECT Messages.id, time, msg, level, source
         FROM Messages
         LEFT JOIN MessageSources ON (Messages.id = MessageSources.msg_id)
         LEFT JOIN Sources ON (Sources.id = MessageSources.source_id)
         " + condition + "
         ORDER BY Messages.id;"

    let getAllMessages () = 
        let queryString = composeQuery ""
        query<Message> queryString

    let getMessagesWithLevel l =
        let queryString = composeQuery "WHERE level = @Level"
        queryP<Message, LevelParam> queryString {Level = l} 

    let getMessagesWithLevelGreaterThan l =
        let queryString = composeQuery "WHERE level > @Level"
        queryP<Message, LevelParam> queryString {Level = l}

    let getAllSources () =
        let queryString = "SELECT source FROM Sources;"
        query<string> queryString |> List.ofSeq

    let getMessagesFromSource src =
        let queryString = composeQuery "WHERE source = @Src"
        queryP<Message, SourceParam> queryString {Src = src; Id = 0}