module Parser

    open System.Text.RegularExpressions

    open Models

    let nullable i = new System.Nullable<int>(i)

    let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
        else None

    let parseLine line =
        match line with
        | Regex @"<([0-9])>\[\s*([0-9]*\.[0-9]{6})\]\s(.*?: |\[.*\] )(.*)" [level; time; source; msg] ->
            let level = int level
            let time = decimal time
            let source = 
                if source.StartsWith("[") then source.Substring(1, source.Length - 2)
                else source.Substring(0, source.Length - 2)
            Some ({Id = 0; Msg = msg; Level = nullable level; Source = source; Time = time})
        | Regex @"\[\s*([0-9]*\.[0-9]{6})\]\s(.*?: |\[.*\] )(.*)" [time; source; msg] ->
            let time = decimal time
            let source = 
                if source.StartsWith("[") then source.Substring(1, source.Length - 2)
                else source.Substring(0, source.Length - 2)
            Some ({Id = 0; Msg = msg; Level = System.Nullable(); Source = source; Time = time})
        | Regex @"<([0-9])>\[\s*([0-9]*\.[0-9]{6})\]\s(.*)" [level; time; msg] ->
            let level = int level
            let time = decimal time
            Some ({Id = 0; Msg = msg; Level = nullable level; Source = null; Time = time})
        | Regex @"\[\s*([0-9]*\.[0-9]{6})\]\s(.*)" [time; msg] ->
            let time = decimal time
            Some ({Id = 0; Msg = msg; Level = System.Nullable(); Source = null; Time = time})
        | _-> None

    let parseText (text:string) =
        text.Split([|"\n"; "\r\n"|], System.StringSplitOptions.RemoveEmptyEntries) |> Array.map parseLine