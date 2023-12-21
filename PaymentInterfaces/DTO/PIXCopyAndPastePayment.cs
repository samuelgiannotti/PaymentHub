using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentInterfaces.DTO
{
    public class PIXCopyAndPastePayment
    {
        public Customer customer {  get; set; }
        public double amount { get; set; }
        public int orderId { get; set; }
    }
}
