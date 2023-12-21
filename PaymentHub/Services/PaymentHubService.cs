using Grpc.Core;
using PaymentHub;
using PaymentHub.PaymentInterfaces.Services;
using PaymentInterfaces.DTO;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PaymentHub.Services
{
    public class PaymentHubService : PaymentHub.PaymentHubBase
    {
        private readonly ILogger<PaymentHubService> _logger;
        private readonly IPIXInterface _iPIXInterface;

        public PaymentHubService(IPIXInterface iPIXInterface,
            ILogger<PaymentHubService> logger)
        {
            _logger = logger;
            _iPIXInterface = iPIXInterface;
        }

        public async override Task<GeneratePIXReply> GeneratePIXCopiaECola(GeneratePIXRequest request, ServerCallContext context)
        {
            double valor = request.Valor;
            Console.WriteLine("GeneratePIXCopiaECola: valor:{0} document:{1} OrderId:{2} CustomerId:{3}", valor, request.Document, request.OrderId, request.CustomerId);

            PIXCopyAndPastePayment payment = new PIXCopyAndPastePayment();
            payment.amount = valor;
            payment.orderId = request.OrderId;
            payment.customer = new Customer();
            payment.customer.Id = request.CustomerId;
            payment.customer.name = request.Nome;
            payment.customer.document = request.Document;
            string pixCopiaECola = "";
            int paymentId = 0;
            (paymentId, pixCopiaECola) = await _iPIXInterface.GeneratePIX(payment);

            return new GeneratePIXReply
            {
                PixCopiaECola = pixCopiaECola,
                PaymentId = paymentId
            };
        }

        public async override Task<CheckPIXPaymentStatusReply> IsPIXPayed(CheckPIXPaymentStatusRequest request, ServerCallContext context)
        {
            Console.WriteLine("IsPIXPayed: OrderId:{0} ", request.OrderId);
            bool isPayed = await _iPIXInterface.IsPIXPayed(request.OrderId);
            return new CheckPIXPaymentStatusReply
            {
                IsPayed = isPayed
            };

        }
    }
}