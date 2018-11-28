using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SmartContract.Commons.Constants;
using SmartContract.Commons.Helpers;
using SmartContract.models.Domains;
using SmartContract.models.Entities;
using SmartContract.models.Repositories;
using SmartContract.Repositories.Mysql.Base;

namespace SmartContract.Repositories.Mysql
{
    public class UserRepository : MySqlBaseRepository<User>, IUserRepository
    {
        private IUserRepository _userRepositoryImplementation;


        public UserRepository(string connectionString) : base(connectionString)
        {
        }

        public UserRepository(IDbConnection dbConnection) : base(dbConnection)
        {
        }


        public string QuerySearch(Dictionary<string, string> models)
        {
            var sQuery = "SELECT * FROM " + TableName + " WHERE 1 = 1";
            foreach (var model in models)
            {
                sQuery += string.Format(" AND {0}='{1}'", model.Key, model.Value);
            }

            return sQuery;
        }

        public User FindWhere(string sql)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var result = Connection.QueryFirstOrDefault<User>(sql);

                return result;
            }
            catch (Exception e)
            {
                Logger.Error("UserRepository =>> FindWhere fail: " + e.Message);
                return null;
            }
        }

        public User FindByIdUser(string id)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var sQuery = "SELECT * FROM " + TableName + " WHERE mem_id = @ID";

                var result = Connection.QuerySingleOrDefault<User>(sQuery, new { ID = id });

                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public User FindByAddress(string address)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var sQuery = "SELECT * FROM " + TableName + " WHERE mem_address = @add";

                var result = Connection.QuerySingleOrDefault<User>(sQuery, new { add = address });

                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public string FindEmailBySendTransaction(BlockchainTransaction transaction)
        {
            try
            {
                var user = FindByIdUser(transaction.UserId);

                return user.mem_id;
            }
            catch (Exception e)
            {
                Logger.Error("UserRepository =>> FindEmailByAddressOfWallet fail: " + e.Message);
                return null;
            }
        }


        public User FindByEmailAddress(string emailAddress)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var sQuery = $"SELECT * FROM {TableName} WHERE mem_id = @Email";

                var result = Connection.QuerySingleOrDefault<User>(sQuery, new { Email = emailAddress });

                return result;
            }
            catch (Exception e)
            {
                Logger.Error("UserRepository =>> FindByEmailAddress fail: " + e.Message);
                throw;
            }
        }

        public List<User> FindAllUser()
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var sQuery = $"SELECT * FROM {TableName} ";

                var result = Connection.Query<User>(sQuery);

                return (List<User>)result;
            }
            catch (Exception e)
            {
                Logger.Error("UserRepository =>> FindByEmailAddress fail: " + e.Message);
                throw;
            }
        }

        public User FindUserAddressNull()
        {
            string sqlText =
                " select * from member where  (mem_address IS NULL or mem_address = '') and IsProcessing = 0";

            try
            {
                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }

                if (Connection.Ping())
                {
                    var user = Connection.QueryFirstOrDefault<User>(sqlText);

                    return user;
                }
                else
                {
                    Logger.Error("Connection to db not open");
                    throw new Exception("Connection to db not open");
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error when find wallet address is null" + e.Message);
                throw e;
            }
        }


        public async Task<ReturnObject> LockForProcessUser(User row)
        {
            //Console.WriteLine("LockForProcess");
            int cache = row.Version;

            var setQuery = new Dictionary<string, string>
            {
                {nameof(row.Version), (row.Version + 1).ToString()}, {nameof(row.IsProcessing), "1"}
            };

            var whereValue = new Dictionary<string, string>
            {
                {nameof(row.mem_id), row.mem_id},
                {nameof(row.Version), row.Version.ToString()},
                {
                    nameof(row.IsProcessing), "0"
                }
            };

            if (cache != row.Version)
            {
                Console.WriteLine("fucking error");
            }

            return ExecuteSql(SqlHelper.Query_Update(TableName, setQuery, whereValue));
        }

        public async Task<ReturnObject> ReleaseLock(User row)
        {
            //Console.WriteLine("ReleaseLock");
            var setQuery = new Dictionary<string, string>
            {
                {nameof(row.Version), (row.Version + 1).ToString()}, {nameof(row.IsProcessing), "0"}
            };
            var whereValue = new Dictionary<string, string>
            {
                {nameof(row.mem_id), row.mem_id},
                {nameof(row.Version), row.Version.ToString()},
                {nameof(row.IsProcessing), "1"}
            };

            return ExecuteSql(SqlHelper.Query_Update(TableName, setQuery, whereValue));
        }

        public ReturnObject UpdateUser(User objectUpdate)
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

        public ReturnObject AddBalance(string uid, decimal addBalance)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var sQuery = $"UPDATE {TableName} SET mem_dn_balance=mem_dn_balance+@addBalance WHERE mem_id = @mem_id";
                var result = 0;

                result = Connection.Execute(sQuery, new { mem_id = uid, addBalance = addBalance });


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
    }
}