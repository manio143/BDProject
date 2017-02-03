module Models

    type UpdateState = {
                        mutable MsgId: int;
                        mutable SourceId: int;
                        mutable MsgSourceId: int;
                       }
    type Message = {
                    Id: int;
                    Time: decimal;
                    Msg: string;
                    Level: System.Nullable<int>;
                    Source: string;
                   }
    type LinuxKernel = {Version: string}

    type PriorityWithCount = {Priority: int; Count: decimal;}

    type Statistics = {
                        Msgs: decimal;
                        MsgsByPriority: PriorityWithCount list;
                        Sources: decimal;
                        SourceWithMostMsgs: string;
                        SourceWithMostErrors: string;
                        KernelVersion: LinuxKernel;
                      }

    module Parameters =
        type SourceParam = {Id: int; Src: string}
        type MessageSourceParam = {Id: int; MsgId: int; SourceId: int}
        type LevelParam = {PLevel: int}
        type MessageParam = {Id: int; PTime: decimal; Msg: string; PLevel: System.Nullable<int>;}

        let asMessageParam (msg:Message) =
            {Id = msg.Id; PTime = msg.Time; Msg = msg.Msg; PLevel = msg.Level;}