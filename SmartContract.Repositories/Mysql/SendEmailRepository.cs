using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SmartContract.models.Domains;
using SmartContract.models.Entities;
using SmartContract.models.Repositories;
using SmartContract.Repositories.Mysql.Base;

namespace SmartContract.Repositories.Mysql
{
    public class SendEmailRepository : MultiThreadUpdateEntityRepository<EmailQueue>, ISendEmailRepository
    {
        public SendEmailRepository(string connectionString) : base(connectionString)
        {
        }

        public SendEmailRepository(IDbConnection dbConnection) : base(dbConnection)
        {
        }

        public override Task<ReturnObject> SafeUpdate(EmailQueue row)
        {
            return base.SafeUpdate(row, new List<string>());
        }
    }
}