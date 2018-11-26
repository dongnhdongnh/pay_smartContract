using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SmartContract.Commons.Constants;
using SmartContract.Commons.Helpers;
using SmartContract.models.Domains;

namespace SmartContract.Repositories.Mysql.Base
{
    public abstract class MultiThreadUpdateEntityRepository<TEntity> : MySqlBaseRepository<TEntity>
        where TEntity : MultiThreadUpdateModel
    {
        public MultiThreadUpdateEntityRepository(string connectionString) : base(connectionString)
        {
        }

        public MultiThreadUpdateEntityRepository(IDbConnection dbConnection) : base(dbConnection)
        {
        }


        public TEntity FindRowPending()
        {
            try
            {
                return FindRowByStatus(Status.STATUS_PENDING);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public List<TEntity> FindRowsPending()
        {
            try
            {
                return FindRowsByStatus(Status.STATUS_PENDING);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public List<TEntity> FindRowsInProcess()
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var sqlString = $"Select * from {TableName} where IsProcessing = 1";
                var result = Connection.Query<TEntity>(sqlString).ToList();
                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public TEntity FindRowError()
        {
            try
            {
                return FindRowByStatus(Status.STATUS_ERROR);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public TEntity FindRowByStatus(string status)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var sqlString = $"Select * from {TableName} where Status = @status and IsProcessing = 0";
                var result =
                    Connection.QueryFirstOrDefault<TEntity>(sqlString, new {status = status});
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public List<TEntity> FindRowsByStatus(string status)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();
                //Console.WriteLine("FIND TRANSACTION BY STATUS");
                var sqlString = $"Select * from {TableName} where Status = @status and IsProcessing = 0";
                var result = Connection.Query<TEntity>(sqlString, new {status = status})
                    .ToList();
                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<ReturnObject> LockForProcess(TEntity row)
        {
            //Console.WriteLine("LockForProcess");
            int cache = row.Version;

            var setQuery = new Dictionary<string, string>
            {
                {nameof(row.Version), (row.Version + 1).ToString()}, {nameof(row.IsProcessing), "1"}
            };

            var whereValue = new Dictionary<string, string>
            {
                {nameof(row.Id), row.Id},
                {nameof(row.Version), row.Version.ToString()},
                {nameof(row.IsProcessing), "0"}
            };

            if (cache != row.Version)
            {
                Console.WriteLine("fucking error");
            }

            return ExecuteSql(SqlHelper.Query_Update(TableName, setQuery, whereValue));
        }

        public async Task<ReturnObject> ReleaseLock(TEntity row)
        {
            //Console.WriteLine("ReleaseLock");
            var setQuery = new Dictionary<string, string>
            {
                {nameof(row.Version), (row.Version + 1).ToString()}, {nameof(row.IsProcessing), "0"}
            };
            var whereValue = new Dictionary<string, string>
            {
                {nameof(row.Id), row.Id},
                {nameof(row.Version), row.Version.ToString()},
                {nameof(row.IsProcessing), "1"}
            };

            return ExecuteSql(SqlHelper.Query_Update(TableName, setQuery, whereValue));
        }

        public abstract Task<ReturnObject> SafeUpdate(TEntity row);

        public async Task<ReturnObject> SafeUpdate(TEntity row, IEnumerable<string> updatePropStrings)
        {
            //Console.WriteLine("SafeUpdate");
            try
            {
                var cache = row.Version;
                var setQuery = new Dictionary<string, string>
                {
                    {nameof(row.Version), (row.Version + 1).ToString()},
                    {nameof(row.IsProcessing), "0"},
                    {nameof(row.Status), row.Status},
                    {nameof(row.UpdatedAt), CommonHelper.GetUnixTimestamp().ToString()}
                };

                foreach (var prop in updatePropStrings)
                {
                    var value = typeof(TEntity).GetProperty(prop).GetValue(row);

                    if (value != null)
                    {
                        setQuery.Add(prop, value.ToString());
                    }
                }

                var whereValue = new Dictionary<string, string>
                {
                    {nameof(row.Id), row.Id},
                    {nameof(row.Version), row.Version.ToString()},
                    {nameof(row.IsProcessing), "1"}
                };

                if (cache != row.Version)
                {
                    Console.WriteLine("fucking error");
                }

                return ExecuteSql(SqlHelper.Query_Update(TableName, setQuery, whereValue));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}