using System;
using System.Data;
using MySql.Data.MySqlClient;
using NLog;

namespace SmartContract.Repositories.Mysql.Base
{
    public class MysqlBaseConnection : IDisposable
    {
        public static Logger Logger { get; } = LogManager.GetCurrentClassLogger();
        public string TableName { get; set; }

        public MySqlConnection Connection { get; }

        public MysqlBaseConnection(string connectionString, string tableName)
        {
            TableName = tableName;
            Connection = new MySqlConnection(connectionString);
        }

        public MysqlBaseConnection(IDbConnection dbConnection, string tableName)
        {
            TableName = tableName;
            Connection = dbConnection as MySqlConnection;
        }

        protected string GetClassName()
        {
            return GetType().Name;
        }

        public void Dispose()
        {
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
            Connection?.Dispose();
        }
    }
}