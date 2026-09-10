using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Messaging
{
    public class TransactionMessage
    {
        public Guid Id { get; set; }
        public string PartnerId { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
