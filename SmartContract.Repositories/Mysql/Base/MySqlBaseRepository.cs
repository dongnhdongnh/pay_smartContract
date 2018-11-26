using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using SmartContract.Commons.Constants;
using SmartContract.models.Domains;
using SmartContract.models.Repositories.Base;

namespace SmartContract.Repositories.Mysql.Base
{
     public abstract class MySqlBaseRepository<TModel> : MysqlBaseConnection, IRepositoryBase<TModel>
    {
        public MySqlBaseRepository(string connectionString) : base(connectionString,
            SimpleCRUD.GetTableName(typeof(TModel)))
        {
        }

        public MySqlBaseRepository(IDbConnection dbConnection) : base(dbConnection,
            SimpleCRUD.GetTableName(typeof(TModel)))
        {
        }

        public ReturnObject Update(TModel objectUpdate)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();
                var result = Connection.Update(objectUpdate);
                var status = result > 0 ? Status.STATUS_SUCCESS : Status.STATUS_ERROR;
                Logger.Debug(GetClassName() + " =>> update status: " + status);
                return new ReturnObject
                {
                    Status = status,
                    Message = status == Status.STATUS_ERROR ? "Cannot Update" : "Update Success",
                    Data = ""
                };
            }
            catch (Exception e)
            {
                Logger.Error(GetClassName() + " =>> update fail: " + e.Message);
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = "Cannot update: " + e.Message,
                    Data = ""
                };
            }
        }

        public ReturnObject Delete(string id)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var result = Connection.Delete(FindById(id));
                var status = result > 0 ? Status.STATUS_SUCCESS : Status.STATUS_ERROR;
                Logger.Debug(GetClassName() + " =>> Delete status: " + status);
                return new ReturnObject
                {
                    Status = status,
                    Message = status == Status.STATUS_ERROR ? "Cannot delete" : "Delete Success"
                };
            }
            catch (Exception e)
            {
                Logger.Error(GetClassName() + " =>> Delete fail: " + e.Message);
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = "Cannot delete: " + e.Message,
                    Data = ""
                };
            }
        }

        public ReturnObject Insert(TModel objectInsert)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();
                var result = Connection.InsertTask<string, TModel>(objectInsert);
                var status = !string.IsNullOrEmpty(result) ? Status.STATUS_SUCCESS : Status.STATUS_ERROR;
                Logger.Debug(GetClassName() + " =>> insert status: " + status);
                return new ReturnObject
                {
                    Status = status,
                    Message = status == Status.STATUS_ERROR ? "Cannot insert" : "Insert Success"
                };
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                Logger.Error(GetClassName() + " =>> insert fail: " + e.Message);
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = "Cannot insert: " + e.Message,
                    Data = ""
                };
            }
        }

        public TModel FindById(string id)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var sQuery = "SELECT * FROM " + TableName + " WHERE Id = @ID";

                var result = Connection.QuerySingleOrDefault<TModel>(sQuery, new {ID = id});

                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public List<TModel> FindBySql(string sqlString)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var result = Connection.Query<TModel>(sqlString);


                return result.ToList();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public ReturnObject ExecuteSql(string sqlString, object transaction = null)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();


                var result = 0;
                if (transaction != null)
                {
                    MySqlTransaction mysqlTransaction = (MySqlTransaction)transaction;
                    result = Connection.Execute(sqlString, null, mysqlTransaction);
                }
                else
                {
                    result = Connection.Execute(sqlString);
                }

                var status = result > 0 ? Status.STATUS_SUCCESS : Status.STATUS_ERROR;
                //	Console.WriteLine("Excute thing " + result);
                return new ReturnObject
                {
                    Status = status,
                    Message = status == Status.STATUS_ERROR ? "Cannot Excute" : "Excute Success"
                };
            }
            catch (Exception e)
            {
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = e.Message
                };
            }
        }

        public int ExcuteCount(string sql)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                return Connection.ExecuteScalar<int>(sql);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}