using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptPrinter
{
    class BookingReceipt
    {
        public string servedBy { get; set; }
        public string customerName { get; set; }
        public string date { get; set; }
        public string invoiceNumber { get; set; }
        public string booking { get; set; }
        public string amountDue { get; set; }
        public string discount { get; set; }
        public string totalDue { get; set; }
        public string amountPaid { get; set; }
        public string balance { get; set; }
    }
}
