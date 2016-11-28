using System;
using System.Configuration;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using Dapper;

namespace BDProject
{
	public static class DBHelper
	{
		public static DbConnection Open() {
			return new OracleConnection (ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString);
		}
	}
}

