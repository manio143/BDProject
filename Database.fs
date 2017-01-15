module Database

    open FSharp.Data.Sql    

    let [<Literal>] private resolutionPath = __SOURCE_DIRECTORY__ + "/bin/Debug/"

    type mySql = SqlDataProvider<
                        ConnectionString = "Server=localhost;Database=inf;Uid=root;Pwd=root;",
                        DatabaseVendor = Common.DatabaseProviderTypes.MYSQL,
                        ResolutionPath = resolutionPath,
                        IndividualsAmount = 1000,
                        UseOptionTypes = true >

    //type oracleSql = SqlDataProvider<
    //                    ConnectionString = "DataSource=myOracleDb;User Id=md370784;Pwd=Meninblack1;",
    //                    DatabaseVendor = Common.DatabaseProviderTypes.ORACLE,
    //                    ResolutionPath = resolutionPath,
    //                    IndividualsAmount = 1000,
    //                    UseOptionTypes = true >
                        
    type SqlDatabase = MySQL | Oracle

    let openDb db = 
        match db with
        | MySQL -> mySql.GetDataContext()
        | Oracle -> raise (new System.NotImplementedException())//oracleSql.GetDataContext()

