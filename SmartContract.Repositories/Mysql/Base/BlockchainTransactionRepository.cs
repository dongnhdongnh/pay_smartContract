using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SmartContract.Commons.Constants;
using SmartContract.Commons.Helpers;
using SmartContract.models.Domains;
using SmartContract.models.Repositories.Base;

namespace SmartContract.Repositories.Mysql.Base
{
    public abstract class BlockchainTransactionRepository<TTransaction> :
        MultiThreadUpdateEntityRepository<TTransaction>,
        IRepositoryBlockchainTransaction<TTransaction> where TTransaction : BlockchainTransaction
    {
        public BlockchainTransactionRepository(string connectionString) : base(connectionString)
        {
        }

        public BlockchainTransactionRepository(IDbConnection dbConnection) : base(dbConnection)
        {
        }


       
        public List<TTransaction> FindTransactionsNotCompleteOnNet()
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();
                
                var sqlString =
                    $"Select * from {TableName} where BlockNumber = @BlockNumber and IsProcessing = 0 and Status=@Status";
                var result = Connection
                    .Query<TTransaction>(sqlString, new { BlockNumber = 0, Status = Status.STATUS_COMPLETED })
                    .ToList();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<BlockchainTransaction> FindTransactionHistory(int offset = -1, int limit = -1,
            string[] orderBy = null)
        {
            try
            {
                var setQuery = new Dictionary<string, string>();
                setQuery.Add(nameof(BlockchainTransaction.Status), Status.STATUS_COMPLETED);
                return FindBySql(SqlHelper.Query_Search(TableName, setQuery, limit, offset, orderBy))
                    .ToList<BlockchainTransaction>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
                //	throw;
            }

            //	throw new NotImplementedException();
        }

        public List<BlockchainTransaction> FindTransactionsByUserId(string userId)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("FindTransactionsByUserId: UserId cannot be null or empty");
                }

                var sqlString = $"Select * from {TableName} where UserId = @UserId";
                var result = Connection.Query<TTransaction>(sqlString, new { UserId = userId })
                    .ToList<BlockchainTransaction>();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public List<BlockchainTransaction> FindTransactionsFromAddress(string fromAddress)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                if (string.IsNullOrEmpty(fromAddress))
                {
                    return new List<BlockchainTransaction>();
                }

                var sqlString = $"Select * from {TableName} where FromAddress = @Address";
                var result = Connection.Query<TTransaction>(sqlString, new { Address = fromAddress })
                    .ToList<BlockchainTransaction>();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public List<BlockchainTransaction> FindTransactionsToAddress(string toAddress)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                if (string.IsNullOrEmpty(toAddress))
                {
                    return new List<BlockchainTransaction>();
                }

                var sqlString = $"Select * from {TableName} where ToAddress = @Address";
                var result = Connection.Query<TTransaction>(sqlString, new { Address = toAddress })
                    .ToList<BlockchainTransaction>();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public List<BlockchainTransaction> FindTransactionsInner(string innerAddress)
        {
            var className = GetClassName();

            if (className.Contains("Deposit"))
            {
                return FindTransactionsToAddress(innerAddress);
            }

            if (className.Contains("Withdraw"))
            {
                return FindTransactionsFromAddress(innerAddress);
            }

            throw new Exception(
                className + ": Transaction repository class name not contain \"Deposit\" or \"Withdraw\" keyword");
        }


        public List<BlockchainTransaction> FindTransactionHistoryAll(out int numberData, string userId, string currency,
            string tableNameWithdraw, string tableNameDeposit, string tableInternalWithdraw, int offset, int limit,
            string[] orderByValue, string whereValue, long dayValue)

        {
            numberData = -1;
            try
            {
                string searchString = "";
                if (whereValue != null && whereValue != String.Empty && whereValue != "")
                {
                    searchString += " and ABS(Amount)= " + whereValue + " ";
                }
                if (dayValue >= 0)
                {
                    searchString += " and CreatedAt>= " + dayValue + " ";
                }

                tableInternalWithdraw = tableInternalWithdraw.Replace("`", string.Empty);
                var selectInternal =
                    $"SELECT {tableInternalWithdraw}.Id,SenderUserId as UserId,SenderUserId as FromAddress,Email as ToAddress,{tableInternalWithdraw}.CreatedAt,{tableInternalWithdraw}.Status,{tableInternalWithdraw}.Description,-1 as Hash,PricePerCoin,-Amount as Amount " +
                    $"FROM {tableInternalWithdraw} left join User on ReceiverUserId=User.Id" +
                    $" where SenderUserId = '{userId}'and Currency = '{currency}' " + searchString +
                    $"UNION ALL " +
                    $"SELECT {tableInternalWithdraw}.Id,ReceiverUserId as UserId,Email as FromAddress,ReceiverUserId as ToAddress,{tableInternalWithdraw}.CreatedAt,{tableInternalWithdraw}.Status,{tableInternalWithdraw}.Description,-1 as Hash,PricePerCoin,Amount as Amount " +
                    $"FROM {tableInternalWithdraw} left join User on SenderUserId=User.Id" +
                    $" where ReceiverUserId = '{userId}'and Currency = '{currency}'" + searchString;
                var selectThing = "Id,UserId,FromAddress,ToAddress,CreatedAt,Status,Description,Hash,PricePerCoin";
                var output =
                    $"Select * from ( SELECT {selectThing},-Amount AS Amount FROM {tableNameWithdraw} WHERE UserId='{userId}'" +
                    searchString +
                    $" UNION ALL " +
                    $" SELECT {selectThing},Amount FROM {tableNameDeposit} WHERE UserId='{userId}' {searchString} UNION ALL {selectInternal}) as t_uni ";
                var outputCount =
                    $"Select count(*) from ( SELECT {selectThing},-Amount AS Amount FROM {tableNameWithdraw} WHERE UserId='{userId}'" +
                    searchString +
                    $" UNION ALL " +
                    $" SELECT {selectThing},Amount FROM {tableNameDeposit} WHERE UserId='{userId}' {searchString} UNION ALL {selectInternal}) as t_uni ";
                numberData = ExcuteCount(outputCount);
                StringBuilder orderStr = new StringBuilder("");
                int count = 0;
                if (orderByValue != null)
                {
                    count = 0;
                    foreach (var prop in orderByValue)
                    {
                        //if (prop.Value != null)
                        {

                            if (count > 0)
                                orderStr.Append(",");
                            if (prop[0].Equals('-'))
                            {
                                orderStr.AppendFormat(" {0} DESC ", prop.Remove(0, 1));
                            }
                            else
                            {
                                orderStr.AppendFormat(" {0}", prop);
                            }
                            count++;
                        }
                    }

                    output += " ORDER BY " + orderStr.ToString();
                }

                if (limit > 0)
                {
                    output += " LIMIT " + limit;
                }

                if (offset > 0)
                {
                    output += " OFFSET " + offset;
                }

                return FindBySql(output).ToList<BlockchainTransaction>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
                //	throw;
            }
        }

        public string GetTableName()
        {
            return TableName;
            // throw new NotImplementedException();
        }

        public override Task<ReturnObject> SafeUpdate(TTransaction row)
        {
            return base.SafeUpdate(row, new[] { nameof(row.Hash) });
        }
    }
}