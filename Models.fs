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

    type SourceR = {Source: string; Count: int}
    type Source(source: string, count: int) =
        member x.Source = source
        member x.Count = count
        new(source: string, count: int64) = Source(source, int count)
        new(source: string, count: decimal) = Source(source, int count)
        interface DotLiquid.ILiquidizable with member x.ToLiquid () = {Source = source; Count = count} :> obj

    type PriorityWithCount(priority: int, count: int) =
        member x.Priority = priority
        member x.Count = count
        new(priority: int, count: int64) = PriorityWithCount(priority, int count)
        new(priority: int, count: decimal) = PriorityWithCount(priority, int count)

    type Statistics = {
                        Messages: int;
                        MessagesByPriority: PriorityWithCount list;
                        Sources: int;
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