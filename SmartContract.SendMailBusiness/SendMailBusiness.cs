using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using SmartContract.Commons.Constants;
using SmartContract.Commons.Helpers;
using SmartContract.models.Domains;
using SmartContract.models.Entities;
using SmartContract.models.Repositories;
using SmartContract.Repositories.Mysql;

namespace SmartContract.SendMailBusiness
{
    public class SendMailBusiness
    {
        private readonly ISmartContractRepositoryFactory _smartcontractRepositoryFactory;

        private readonly IDbConnection _connectionDb;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public SendMailBusiness(ISmartContractRepositoryFactory smartcontractRepositoryFactory, bool isNewConnection = true)
        {
            _smartcontractRepositoryFactory = smartcontractRepositoryFactory;
            _connectionDb = isNewConnection
                ? smartcontractRepositoryFactory.GetDbConnection()
                : smartcontractRepositoryFactory.GetOldConnection();
        }

        public async Task<ReturnObject> CreateEmailQueueAsync(EmailQueue emailQueue)
        {
            try
            {
                using (var sendEmailRepository = _smartcontractRepositoryFactory.GetSendEmailRepository(_connectionDb))
                {
                    // save to DB
                    var result = sendEmailRepository.Insert(emailQueue);

                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = e.ToString()
                };
            }
        }

        public async Task<ReturnObject> SendEmailAsync(string apiUrl, string apiKey, string from,
            string fromName)
        {
            using (var sendEmailRepository = _smartcontractRepositoryFactory.GetSendEmailRepository(_connectionDb))
            {
                var pendingEmail = sendEmailRepository.FindRowPending();

                if (pendingEmail?.Id == null)
                    return new ReturnObject
                    {
                        Status = Status.STATUS_SUCCESS,
                        Message = "Pending email not found"
                    };

                if (_connectionDb.State != ConnectionState.Open)
                    _connectionDb.Open();

                //begin first email
                var transctionScope = _connectionDb.BeginTransaction();
                try
                {
                    var lockResult = await sendEmailRepository.LockForProcess(pendingEmail);
                    if (lockResult.Status == Status.STATUS_ERROR)
                    {
                        transctionScope.Rollback();
                        return new ReturnObject
                        {
                            Status = Status.STATUS_SUCCESS,
                            Message = "Cannot Lock For Process"
                        };
                    }

                    transctionScope.Commit();
                }
                catch (Exception e)
                {
                    transctionScope.Rollback();
                    return new ReturnObject
                    {
                        Status = Status.STATUS_ERROR,
                        Message = e.ToString()
                    };
                }

                //update Version to Model
                pendingEmail.Version += 1;

                var transactionSend = _connectionDb.BeginTransaction();
                try
                {
                    ReturnObject sendResult;
                    if (pendingEmail.SendFileList == null)
                        sendResult = await SendEmail(pendingEmail, apiUrl, apiKey, from, fromName);
                    else
                        sendResult = await SendEmailSendFile(pendingEmail, apiUrl, apiKey, from, fromName);

                    

                    pendingEmail.Status = sendResult.Status;
                    pendingEmail.IsProcessing = 0;

                    var updateResult = await sendEmailRepository.SafeUpdate(pendingEmail);
                    if (updateResult.Status == Status.STATUS_ERROR)
                    {
                        transactionSend.Rollback();
                        return new ReturnObject
                        {
                            Status = Status.STATUS_ERROR,
                            Message = "Cannot update email status"
                        };
                    }

                    transactionSend.Commit();
                    return updateResult;
                }
                catch (Exception e)
                {
                    // release lock
                    transactionSend.Rollback();
                    var releaseResult = sendEmailRepository.ReleaseLock(pendingEmail);
                    Console.WriteLine(JsonHelper.SerializeObject(releaseResult));
                    throw;
                }
            }
        }

        public async Task<ReturnObject> SendEmail(EmailQueue emailQueue, string apiUrl, string apiKey, string from,
            string fromName)
        {
            string emailBody = CreateEmailBody(emailQueue);
            if (emailBody == null)
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = "Cannot find template"
                };
            var values = new NameValueCollection
            {
                {"apikey", apiKey},
                {"from", from},
                {"fromName", fromName},
                {"to", emailQueue.ToEmail},
                {"subject", emailQueue.Subject},
                {"bodyHtml", emailBody},
                {"isTransactional", "true"}
            };

            using (var client = new WebClient())
            {
                try
                {
                    byte[] apiResponse = client.UploadValues(apiUrl, values);

                    var result = JsonHelper.DeserializeObject<JObject>(Encoding.UTF8.GetString(apiResponse));

                    var status = (bool)result["success"] ? Status.STATUS_SUCCESS : Status.STATUS_ERROR;

                    return new ReturnObject
                    {
                        Status = status,
                        Message = Encoding.UTF8.GetString(apiResponse)
                    };
                }
                catch (Exception ex)
                {
                    return new ReturnObject
                    {
                        Status = Status.STATUS_ERROR,
                        Message = ex.Message
                    };
                }
            }
        }

        public async Task<ReturnObject> SendEmailSendFile(EmailQueue emailQueue, string apiUrl, string apiKey, string from,
          string fromName)
        {
            string emailBody = CreateEmailBody(emailQueue);
            if (emailBody == null)
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = "Cannot find template"
                };
            var values = new NameValueCollection
            {
                {"apikey", apiKey},
                {"from", from},
                {"fromName", fromName},
                {"to", emailQueue.ToEmail},
                {"subject", emailQueue.Subject},
                {"bodyHtml", emailBody},
                {"isTransactional", "true"}
            };

            string _filesToSend = emailQueue.SendFileList;
            string[] filesToSend = _filesToSend.Split(",");
            List<Stream> paramFileStream = new List<Stream>();
            List<string> filenames = new List<string>();
            foreach (var fileToSend in filesToSend)
            {
                // AppSettingHelper.GetBitcoinNode
                var filepath = AppSettingHelper.GetReportStoreUrl() + fileToSend;
                if (!System.IO.File.Exists(filepath))
                    continue;
                var file = File.OpenRead(filepath);
                filenames.Add(fileToSend);
                paramFileStream.Add(file);
            }



            
            {
                try
                {

                    using (var client = new HttpClient())
                    using (var formData = new MultipartFormDataContent())
                    {
                        foreach (string key in values)
                        {
                            HttpContent stringContent = new StringContent(values[key]);
                            formData.Add(stringContent, key);
                        }

                        for (int i = 0; i < paramFileStream.Count; i++)
                        {
                            HttpContent fileStreamContent = new StreamContent(paramFileStream[i]);
                            formData.Add(fileStreamContent, "file" + i, filenames[i]);
                     
                        }

                        var response = await client.PostAsync(apiUrl, formData);
                        //   var response = postData.Result;
                        for (int i = 0; i < paramFileStream.Count; i++)
                        {
                            paramFileStream[i].Close();
                        }
                        foreach (var fileToSend in filesToSend)
                        {
                            var filepath = AppSettingHelper.GetReportStoreUrl() + fileToSend;
                            if (!System.IO.File.Exists(filepath))
                            {
                                Console.WriteLine(filepath + " not exist");
                                continue;
                            }
                            System.IO.File.Delete(filepath);
                        }
                        if (!response.IsSuccessStatusCode)
                        {
                            
                            return new ReturnObject
                            {
                                Status = Status.STATUS_ERROR,
                                Message = response.Content.ReadAsStringAsync().Result
                            };
                        }



                        return new ReturnObject
                        {
                            Status = Status.STATUS_SUCCESS,
                            Message = response.Content.ReadAsStringAsync().Result
                        };
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return new ReturnObject
                    {
                        Status = Status.STATUS_ERROR,
                        Message = ex.Message
                    };
                }
            }
        }




        private BlockchainTransaction GetTransaction(EmailQueue emailQueue)
        {
            switch (emailQueue.NetworkName)
            {
                case CryptoCurrency.BTC:
                    switch (emailQueue.Template)
                    {
                        case EmailTemplate.Sent:
                            /*using (var bitcoinWithdraw =
                                _smartcontractRepositoryFactory.GetBitcoinWithdrawTransactionRepository(_connectionDb))
                            {
                                return bitcoinWithdraw.FindById(emailQueue.TransactionId);
                            }*/
                        case EmailTemplate.Received:
                            /*using (var bitcoinDeposit =
                                _smartcontractRepositoryFactory.GetBitcoinDepositTransactionRepository(_connectionDb))
                            {
                                return bitcoinDeposit.FindById(emailQueue.TransactionId);
                            }*/
                        case EmailTemplate.NewDevice:
                            break;
                        case EmailTemplate.Verify:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case CryptoCurrency.ETH:
                    switch (emailQueue.Template)
                    {
                        case EmailTemplate.Sent:
                            //return
                            using (var _rep =
                                _smartcontractRepositoryFactory.GetEthereumWithdrawTransactionRepository(_connectionDb))
                            {
                                return _rep.FindById(emailQueue.TransactionId);
                            }
                        case EmailTemplate.Received:
                            using (var _rep =
                                _smartcontractRepositoryFactory.GetEthereumDepositeTransactionRepository(_connectionDb))
                            {
                                return _rep.FindById(emailQueue.TransactionId);
                            }

                        case EmailTemplate.NewDevice:
                            break;
                        case EmailTemplate.Verify:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case CryptoCurrency.VAKA:
                    switch (emailQueue.Template)
                    {
                        case EmailTemplate.Sent:
                            /*using (var rep =
                                _smartcontractRepositoryFactory.GetVakacoinWithdrawTransactionRepository(_connectionDb))
                            {
                                return rep.FindById(emailQueue.TransactionId);
                            }*/

                        case EmailTemplate.Received:
                            /*using (var rep =
                                _smartcontractRepositoryFactory.GetVakacoinDepositTransactionRepository(_connectionDb))
                            {
                                return rep.FindById(emailQueue.TransactionId);
                            }*/

                        case EmailTemplate.NewDevice:
                            break;
                        case EmailTemplate.Verify:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
            }

            return null;
        }

        private string GetReceivedAddress(EmailQueue emailQueue)
        {
            try
            {
                if (emailQueue.IsInnerTransaction == false) return GetTransaction(emailQueue).ToAddress;
                using (var internalTransactionsRepository = new InternalTransactionsRepository(_connectionDb))
                {
                    var transaction = internalTransactionsRepository.FindById(emailQueue.TransactionId);

                    using (var userRepository = new UserRepository(_connectionDb))
                    {
                        var user = userRepository.FindById(transaction.ReceiverUserId);
                        return user.Id;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string CreateEmailBody(EmailQueue emailQueue)
        {
            try
            {
                string body = string.Empty;
                string directory = Directory.GetCurrentDirectory() + "/MailTemplate/" +
                                   EmailConfig.TEMPLATE_FILES[emailQueue.Template];

                using (StreamReader reader = new StreamReader(directory))
                {
                    body = reader.ReadToEnd();
                }

                body = body.Replace("{vakapayUrl}", EmailConfig.VAKAPAY_URL);
                body = body.Replace("{logoImgUrl}", EmailConfig.LOGO_IMG_URL);
                body = body.Replace("{mailImgUrl}", EmailConfig.MAIL_IMG_URL);
                body = body.Replace("{checkImgUrl}", EmailConfig.CHECK_IMG_URL);
                body = body.Replace("{hrImgUrl}", EmailConfig.HR_IMG_URL);
                body = body.Replace("{deviceImgUrl}", EmailConfig.DEVICE_IMG_URL);

                switch (emailQueue.Template)
                {
                    case EmailTemplate.NewDevice:
                        body = body.Replace("{location}", emailQueue.DeviceLocation);
                        body = body.Replace("{ip}", emailQueue.DeviceIP);
                        body = body.Replace("{browser}", emailQueue.DeviceBrowser);
                        body = body.Replace("{authorizeUrl}", emailQueue.DeviceAuthorizeUrl);
                        break;

                    case EmailTemplate.Sent:
                        body = body.Replace("{signInUrl}", emailQueue.SignInUrl);
                        body = body.Replace("{toAddress}", GetReceivedAddress(emailQueue));
                        body = body.Replace("{amount}", emailQueue.GetAmount());
                        break;
                    case EmailTemplate.Received:
                        body = body.Replace("{signInUrl}", emailQueue.SignInUrl);
                        body = body.Replace("{networkName}", emailQueue.NetworkName);
                        body = body.Replace("{amount}", emailQueue.GetAmount());
                        body = body.Replace("{numberOfConfirmation}",
                            EmailConfig.GetNumberOfNeededConfirmation(emailQueue.NetworkName));
                        break;

                    case EmailTemplate.Verify:
                        body = body.Replace("{verifyEmailUrl}", emailQueue.VerifyUrl);
                        break;

                    case EmailTemplate.ReceivedInternal:
                        body = body.Replace("{signInUrl}", emailQueue.SignInUrl);
                        body = body.Replace("{amount}", emailQueue.GetAmount());
                        body = body.Replace("{sender}", GetSender(emailQueue));
                        break;
                    case EmailTemplate.Report:
                        //  body = body.Replace("{verifyEmailUrl}", emailQueue.VerifyUrl);
                        break;

                    default:
                        throw new Exception("Template not defined");
                }

                return body;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private string GetSender(EmailQueue emailQueue)
        {
            try
            {
                using (var internalTransactionsRepository = new InternalTransactionsRepository(_connectionDb))
                {
                    using (var userRepository = new UserRepository(_connectionDb))
                    {
                        var transaction = internalTransactionsRepository.FindById(emailQueue.TransactionId);

                        var user = userRepository.FindById(transaction.SenderUserId);
                        return user.Id;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// CreateDataEmail
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="email"></param>
        /// <param name="amount"></param>
        /// <param name="transactionId"></param>
        /// <param name="template"></param>
        /// <param name="networkName"></param>
        /// <param name="smartcontractRepositoryFactory"></param>
        /// <param name="isInnerTransaction"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task CreateDataEmail(string subject, string email, decimal amount, string transactionId,
            EmailTemplate template, string networkName, ISmartContractRepositoryFactory smartcontractRepositoryFactory,
            bool isInnerTransaction)
        {
            try
            {
                var currentTime = CommonHelper.GetUnixTimestamp();
                var sendMailBusiness = new SendMailBusiness(smartcontractRepositoryFactory, false);

                if (email == null) return;
                var emailQueue = new EmailQueue
                {
                    Id = CommonHelper.GenerateUuid(),
                    ToEmail = email,
                    Template = template,
                    Subject = subject,
                    NetworkName = networkName,
                    IsInnerTransaction = isInnerTransaction,
                    Amount = amount,
                    TransactionId = transactionId,
                    Status = Status.STATUS_PENDING,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime
                };
                await sendMailBusiness.CreateEmailQueueAsync(emailQueue);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}