using PaymentInterfaces.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentHub.PaymentInterfaces.Services
{
    public interface IPIXInterface
    {
        public Task<(int, string)> GeneratePIX(PIXCopyAndPastePayment payment);
        public Task<bool> IsPIXPayed(int orderId);
    }
}
