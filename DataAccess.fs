module DataAccess

    open Database

    let cnn = ``open`` MySQL

    type TestValue = {Id: int}

    let test () = printfn "%A" (query<TestValue> cnn "SELECT id FROM inf.test;")
//TODO: create functions for the buisness logic of the app
