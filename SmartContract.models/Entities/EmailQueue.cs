using System.ComponentModel.DataAnnotations.Schema;
using SmartContract.Commons.Constants;
using SmartContract.models.Domains;

namespace SmartContract.models.Entities
{
    public enum EmailTemplate
    {
        NewDevice,
        Sent,
        Received,
        Verify,
        ReceivedInternal,
        Report
    }

    [Table("EmailQueue")]
    public class EmailQueue : MultiThreadUpdateModel
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public EmailTemplate Template { get; set; }

        //new device template
        public string DeviceLocation { get; set; }
        public string DeviceIP { get; set; }
        public string DeviceBrowser { get; set; }
        public string DeviceAuthorizeUrl { get; set; }

        // Sent/Received template
        public string SignInUrl { get; set; }
        public decimal Amount { get; set; }
        public string NetworkName { get; set; }
        public bool IsInnerTransaction { get; set; }
        public string TransactionId { get; set; }

        //Verify email template
        public string VerifyUrl { get; set; }

        public string GetAmount()
        {
            return CryptoCurrency.GetAmount(NetworkName, Amount);
        }
        //Send File
        public string SendFileList { get; set; }
    }
}