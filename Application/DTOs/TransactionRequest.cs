using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class TransactionRequest
    {
        public string? PartnerId { get; set; }
        public string? TransactionReference { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
