using System.Collections.Generic;
using SmartContract.Commons.Constants;
using SmartContract.models.Entities;

namespace SmartContract.models.Domains
{
    public class EmailConfig
    {
        public const string VAKAPAY_URL = "google.com.vn";

        public const string LOGO_IMG_URL = "https://i.imgur.com/ooQLCzZ.png";
        public const string CHECK_IMG_URL = "https://i.imgur.com/EEDBk5M.png";
        public const string MAIL_IMG_URL = "https://i.imgur.com/8idVPQD.png";
        public const string HR_IMG_URL = "https://i.imgur.com/SDIW5OD.png";
        public const string DEVICE_IMG_URL = "https://i.imgur.com/2p0MC3f.png";
        public const long BTC_CONFIRMATIONS = 3;
        public const long ETH_CONFIRMATIONS = 50;
        public const long VAKA_CONFIRMATIONS = 15;

        // template
        public const string TEMPLATE_NEW_DEVICE = "newDevice";
        public const string TEMPLATE_VERIFY = "verify";
        public const string TEMPLATE_SENT_OR_RECEIVED = "sent";

        // for sent template
        public const string SIGN_IN_URL = "google.com.vn";
        public const string SENT_OR_RECEIVED_SENT = "sent";
        public const string SENT_OR_RECEIVED_RECEIVED = "received";

        //email Subject
        //new device subject
        public const string SUBJECT_NEW_DEVICE = "New device";

        //sent subject
        public const string SUBJECT_SENT_OR_RECEIVED = "Balance notification!";

        public const string SUBJECT_REPORT = "Your Report";

        //verify subject
        public const string SUBJECT_VERIFY = "Verify account";

        public static readonly Dictionary<EmailTemplate, string> TEMPLATE_FILES =
            new Dictionary<EmailTemplate, string>()
            {
                {EmailTemplate.NewDevice, "newDevice.htm"},
                {EmailTemplate.Sent, "sent.htm"},
                {EmailTemplate.Received, "received.htm"},
                {EmailTemplate.ReceivedInternal, "receivedInternal.htm"},
                {EmailTemplate.Verify, "verify.htm"},
                {EmailTemplate.Report, "report.htm"}
            };

        public static string GetNumberOfNeededConfirmation(string networkName)
        {
            long confirmation = 0;
            switch (networkName)
            {
                case CryptoCurrency.VAKA:
                    confirmation = VAKA_CONFIRMATIONS;
                    break;
                case CryptoCurrency.ETH:
                    confirmation = ETH_CONFIRMATIONS;
                    break;
                case CryptoCurrency.BTC:
                    confirmation = BTC_CONFIRMATIONS;
                    break;
            }

            return confirmation.ToString();
        }
    }
}