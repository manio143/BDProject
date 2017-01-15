open Database

[<EntryPoint>]
let main argv = 
    let ctx = openDb MySQL
    let example =
            query {
                for i in ctx.Inf.Test do
                    select (i.Id)
                  }
    example |> Seq.iter (fun x -> printfn "%d" x)
    0 // return an integer exit code

