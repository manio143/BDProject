module Models

    type UpdateState = {
                        mutable MessageId: int;
                        mutable SourceId: int;
                        mutable MessageSourceId: int;
                       }
    type Message = {
                    Id: int;
                    Time: decimal;
                    Message: string;
                    Priority: System.Nullable<int>;
                    Source: string;
                   }
    type LinuxKernel = {Version: string}

    type Source = { Source: string; Count: decimal }

    type PriorityWithCount = {Priority: int; Count: decimal;}

    type Statistics = {
                        Messages: decimal;
                        MessagesByPriority: PriorityWithCount list;
                        Sources: decimal;
                        SourceWithMostMessages: string;
                        SourceWithMostErrors: string;
                        KernelVersion: LinuxKernel;
                      }

    module Parameters =
        type SourceParam = {Id: int; Src: string}
        type MessageSourceParam = {Id: int; MessageId: int; SourceId: int}
        type PriorityParam = {PPriority: int}
        type MessageParam = {Id: int; PTime: decimal; Message: string; PPriority: System.Nullable<int>;}

        let asMessageParam (msg:Message) =
            {Id = msg.Id; PTime = msg.Time; Message = msg.Message; PPriority = msg.Priority;}