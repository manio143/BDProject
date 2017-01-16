module Database

    open Dapper

    open System.Data.Common

    type DatabaseType = MySQL | Oracle

    module MySQL =

        open MySql.Data.MySqlClient

        let private connectionStr = System.Configuration.ConfigurationManager.ConnectionStrings.["mySqlCnnStr"].ConnectionString
        let createConnection () = new MySqlConnection(connectionStr)

    module Oracle =

        open Oracle.ManagedDataAccess.Client

        let private connectionStr = System.Configuration.ConfigurationManager.ConnectionStrings.["oracleCnnStr"].ConnectionString
        let createConnection () = new OracleConnection(connectionStr)

    let ``open`` db =
        let cnn = 
            match db with
            | MySQL -> MySQL.createConnection() :> DbConnection
            | Oracle -> Oracle.createConnection() :> DbConnection
        cnn.Open()
        cnn

    let query<'a> cnn (cmd:string) = 
        Dapper.SqlMapper.Query<'a>(cnn, cmd)

    let execute cnn (cmd:string) =
        Dapper.SqlMapper.Execute(cnn, cmd)