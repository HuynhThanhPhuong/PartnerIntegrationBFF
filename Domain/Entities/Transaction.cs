using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; }
        public string PartnerId { get; }
        public string TransactionReference { get; }
        public Money Amount { get; }
        public DateTimeOffset Timestamp { get; }
        public VerificationStatus Status { get; private set; }

        private Transaction(Guid id, string partnerId, string transactionReference, Money amount, DateTimeOffset timestamp, VerificationStatus status)
        {
            Id = id;
            PartnerId = partnerId;
            TransactionReference = transactionReference;
            Amount = amount;
            Timestamp = timestamp;
            Status = status;
        }

        public static Transaction Create(string? partnerId, string? transactionReference, decimal amount, string? currency, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(partnerId))
            {
                throw new ValidationException("PartnerId is required.");
            }

            if (string.IsNullOrWhiteSpace(transactionReference))
            {
                throw new ValidationException("TransactionReference is required.");
            }

            var money = Money.Create(amount, currency);

            return new Transaction(Guid.NewGuid(), partnerId.Trim(), transactionReference.Trim(), money, timestamp, VerificationStatus.Pending);
        }

        public void Verified()
        {
            if (Status != VerificationStatus.Pending)
            {
                throw new ValidationException($"Cannot verify a transaction in '{Status}' status.");
            }
            Status = VerificationStatus.Verified;
        }

        public void Rejected()
        {
            if (Status != VerificationStatus.Pending) 
            {
                throw new ValidationException($"Cannot reject a transaction in '{Status}' status.");
            } 
            Status = VerificationStatus.Rejected;
        }
    }
}
