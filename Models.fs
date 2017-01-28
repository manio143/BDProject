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

    type PriorityWithCount = {Priority: int; Count: int64;}

    type Statistics = {
                        Msgs: int;
                        MsgsByPriority: PriorityWithCount list;
                        Sources: int;
                        SourceWithMostMsgs: string;
                        SourceWithMostErrors: string;
                        KernelVersion: LinuxKernel;
                      }

    module Parameters =
        type SourceParam = {Id: int; Src: string}
        type MessageSourceParam = {Id: int; MsgId: int; SourceId: int}
        type LevelParam = {Level: int}
