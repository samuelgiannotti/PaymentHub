using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaymentHub.PaymentInterfaces.Services;
using PaymentHub.PIXItau.DTO;
using PaymentInterfaces.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Security.Cryptography;

namespace PIXItau.Services
{
    public class PIXItauService : IPIXInterface
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static string accessToken;
        private static DateTime accessTokenExpiration;
        private readonly ILogger<PIXItauService> _logger;
        private readonly IConfiguration _configuration;
        private string ClientID, ClientSecret, CaminhoCertificado, SenhaCertificado, urlAuth, urlCob, ChavePIXCobranca;
        private int AcquirerId;
        private static X509Certificate2 certificate = null;

        public static IDbConnection GetOpenConnection(string connectionString) => new SqlConnection(connectionString);


        public PIXItauService(IConfiguration configuration, 
            ILogger<PIXItauService> logger)
        {
            _logger = logger;
            _configuration = configuration;
            ClientID = _configuration.GetValue<string>("PIXItau:ClientID");
            ClientSecret = _configuration.GetValue<string>("PIXItau:ClientSecret");
            CaminhoCertificado = _configuration.GetValue<string>("PIXItau:CertificatePath");
            _logger.LogInformation("CaminhoCertificado:{0}", CaminhoCertificado);
            SenhaCertificado = _configuration.GetValue<string>("PIXItau:CertificatePass");
            urlAuth = _configuration.GetValue<string>("PIXItau:TokenAuthURL");
            urlCob = _configuration.GetValue<string>("PIXItau:CobrancaImediataURL");
            ChavePIXCobranca = _configuration.GetValue<string>("PIXItau:ChavePIXCobranca");
            AcquirerId = _configuration.GetValue<int>("PIXItau:AcquirerId");
            if (certificate is null)
            {
                certificate = new X509Certificate2(File.ReadAllBytes(CaminhoCertificado), SenhaCertificado, X509KeyStorageFlags.DefaultKeySet);
            }
        }

        public async Task<(int, string)> GeneratePIX(PIXCopyAndPastePayment payment)
        {
            string accessToken = await GetPIXItauAccessToken();

            _logger.LogInformation("PIXItauService - GeneratePIX accesstoken:{0}", accessToken);
            HttpWebResponse requestResponse = null;
            var request = (HttpWebRequest)WebRequest.Create(urlCob);
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Headers.Add("x-itau-apikey", accessToken);
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            var dataBody = "{\"valor\":{\"original\": \""+payment.amount+"\" }, \"chave\":\""+ ChavePIXCobranca + "\",\"calendario\":{\"expiracao\": \"14400\"}}";
            _logger.LogInformation("PIXItauService - GeneratePIX: dataBody:{0}", dataBody);
            var body = Encoding.ASCII.GetBytes(dataBody);
            request.ContentLength = body.Length;
            string response = "";
            Stream myWriter = null;
            try
            {
                request.ClientCertificates.Add(certificate);
                myWriter = request.GetRequestStream();
                myWriter.Write(body, 0, body.Length);
                requestResponse = (HttpWebResponse)request.GetResponse();
                response = new StreamReader(requestResponse.GetResponseStream()).ReadToEnd();
                _logger.LogInformation("PIXItauService - GetPIXItau - response:{0}", response);
                var cobranca = JsonConvert.DeserializeObject<PIXCobrancaImeditaResponse>(response);
                if (cobranca != null)
                {
                    var paymentConnStr = _configuration.GetConnectionString("PaymentHubConnection");
                    var paymentHubConn = GetOpenConnection(paymentConnStr);

                    var insertPaymentCommand = "insert into [dbo].[PIXPayment]" +
                                                "([CustomerId],[CustomerName],[CustomerDocumentType],[CustomerDocument],[OrderId]," +
                                                "[QrCodeString],[Tid],[StatusStr],[ReturnMessage],[Amount],[Installments],[AcquirerId],[CNPJ]) " +
                                                "OUTPUT INSERTED.[Id] " +
                                                "values (@CustomerId,@CustomerName,@CustomerDocumentType,@CustomerDocument,@OrderId,@QrCodeString,@Tid,@StatusStr,@ReturnMessage,@Amount,1,@acquirerId,@CNPJ)";
                    var paymentId = paymentHubConn.QuerySingleOrDefaultAsync<Int32>(insertPaymentCommand, new
                    {
                        CustomerId = payment.customer.Id,
                        CustomerName = payment.customer.name,
                        CustomerDocumentType = payment.customer.document.Length > 11 ? "CNPJ" : "CPF",
                        CustomerDocument = payment.customer.document,
                        OrderId = payment.orderId,
                        QrCodeString = cobranca.pixCopiaECola,
                        Tid = cobranca.txid,
                        StatusStr = cobranca.status,
                        ReturnMessage = response,
                        Amount = payment.amount*100,
                        acquirerId = AcquirerId,
                        CNPJ = ChavePIXCobranca
                    }).Result;

                    return (paymentId, cobranca.pixCopiaECola);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                var headerResponse = requestResponse.GetResponseHeader("");
                throw new Exception("Erro ao gerar o pix cobranca imediata.\r\nDetalhe: " + ex.Message);
            }
            finally
            {
                if (myWriter != null)
                    myWriter.Close();
            }


            return (0,null);
        }


        private async Task<string> GetPIXItauAccessToken()
        {
            await semaphore.WaitAsync();

            try
            {
                if (accessToken == null || DateTime.UtcNow >= accessTokenExpiration)
                {
                    _logger.LogInformation("PIXItauService - GetPIXItauAccessToken: accessToken is null");
                    HttpWebResponse requestResponse = null;
                    var request = (HttpWebRequest)WebRequest.Create(urlAuth);
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Method = "POST";
                    var dataBody = "grant_type=" + Uri.EscapeDataString("client_credentials");
                        dataBody += "&client_id=" + Uri.EscapeDataString(ClientID);
                        dataBody += "&client_secret=" + Uri.EscapeDataString(ClientSecret);
                    var body = Encoding.ASCII.GetBytes(dataBody);
                    request.ContentLength = body.Length;
                    string response = "";
                    Stream myWriter = null;
                    try
                    {
                        request.ClientCertificates.Add(certificate);
                        myWriter = request.GetRequestStream();
                        myWriter.Write(body, 0, body.Length);
                        requestResponse = (HttpWebResponse)request.GetResponse();
                        response = new StreamReader(requestResponse.GetResponseStream()).ReadToEnd();
                        var token = JsonConvert.DeserializeObject<Token>(response);
                        if (token != null)
                        {
                            accessTokenExpiration = DateTime.Now.AddSeconds(token.expires_in - 30);
                            accessToken = token.access_token;
                            _logger.LogInformation("PIXItauService - GetPIXItauAccessToken - accessTokenExpiration:{0} accessToken:{1}", accessTokenExpiration, accessToken);
                        }
                        _logger.LogInformation("PIXItauService - GetPIXItauAccessToken - response:{0}", response);
                        return accessToken;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message,ex);
                        var headerResponse = requestResponse.GetResponseHeader("");
                        throw new Exception("Erro ao solicitar o token.\r\nDetalhe: " + ex.Message);
                    }
                    finally
                    {
                        if (myWriter != null)
                            myWriter.Close();
                    }
                }
                return accessToken;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<bool> IsPIXPayed(int orderId)
        {
            string accessToken = await GetPIXItauAccessToken();

            _logger.LogInformation("PIXItauService - IsPIXPayed orderId:{0}", orderId);
            //busca id do pagamento
            var paymentConnStr = _configuration.GetConnectionString("PaymentHubConnection");
            var paymentHubConn = GetOpenConnection(paymentConnStr);
            string getPaymentIDByOrder = "select [Tid] FROM [dbo].[PIXPayment] where [OrderId] = " + orderId;
            var paymentId = await paymentHubConn.QuerySingleOrDefaultAsync<string>(getPaymentIDByOrder);

            if (paymentId is not null)
            {
                HttpWebResponse requestResponse = null;
                var request = (HttpWebRequest)WebRequest.Create(urlCob + "/" + paymentId);
                request.ContentType = "application/json";
                request.Method = "GET";
                request.Headers.Add("x-itau-apikey", accessToken);
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                string response = "";
                Stream myWriter = null;
                try
                {
                    request.ClientCertificates.Add(certificate);
                    //myWriter = request.GetRequestStream();
                    requestResponse = (HttpWebResponse)request.GetResponse();
                    response = new StreamReader(requestResponse.GetResponseStream()).ReadToEnd();
                    _logger.LogInformation("PIXItauService - IsPIXPayed - response:{0}", response);
                    var cobranca = JsonConvert.DeserializeObject<PIXCobrancaImeditaResponse>(response);
                    if (cobranca != null && cobranca.status.Equals("CONCLUIDA"))
                    {
                        var updatePaymentCommand = "update [dbo].[PIXPayment] set [StatusStr] = @StatusStr, [ReturnMessage] = @ReturnMessage where [OrderId] = " + orderId;
                        await paymentHubConn.QuerySingleOrDefaultAsync<Int32>(updatePaymentCommand, new
                        {
                            StatusStr = cobranca.status,
                            ReturnMessage = response
                        });

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    var headerResponse = requestResponse.GetResponseHeader("");
                    throw new Exception("Erro ao gerar o pix cobranca imediata.\r\nDetalhe: " + ex.Message);
                }
                finally
                {
                    if (myWriter != null)
                        myWriter.Close();
                }
            }

            return false;
        }
    }
}
