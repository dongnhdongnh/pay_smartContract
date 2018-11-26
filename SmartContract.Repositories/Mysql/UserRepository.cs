using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SmartContract.models.Domains;
using SmartContract.models.Entities;
using SmartContract.models.Repositories;
using SmartContract.Repositories.Mysql.Base;

namespace SmartContract.Repositories.Mysql
{
    public class UserRepository : MultiThreadUpdateEntityRepository<User>, IUserRepository
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

        public string FindEmailBySendTransaction(BlockchainTransaction transaction)
        {
            try
            {
                var user = FindById(transaction.UserId);

                return user.Id;
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

                var result = Connection.QuerySingleOrDefault<User>(sQuery, new {Email = emailAddress});

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

                return (List<User>) result;
            }
            catch (Exception e)
            {
                Logger.Error("UserRepository =>> FindByEmailAddress fail: " + e.Message);
                throw;
            }
        }
        
        public User FindUserAddressNull()
        {
            string sqlText = " select * from member where  (mem_address IS NULL or mem_address = '') and IsProcessing = 0 and Status = 'pending'";

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
        
        public override Task<ReturnObject> SafeUpdate(User row)
        {
            return base.SafeUpdate(row, new[] {nameof(row.Status)});
        }
    }
}