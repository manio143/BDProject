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
        | Regex @"<([0-9])>\[\s*([0-9]*\.[0-9]{6})\]\s((.*: )|(\[.*\] ))(.*)" [level; time; source; msg] ->
            let level = int level
            let time = decimal time
            let source = source.Substring(0, source.Length - 2);
            Some ({Id = 0; Msg = msg; Level = nullable level; Source = source; Time = time})
        | _-> None

    let parseText (text:string) =
        text.Split([|"\n"; "\r\n"|], System.StringSplitOptions.RemoveEmptyEntries) |> Array.map parseLine