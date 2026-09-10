using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Exceptions;

namespace Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money Create(decimal amount, string? currency)
        {
            if (amount <= 0)
            {
                throw new ValidationException("Amount must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new ValidationException("Currency is required.");
            }
                
            var checkvalidcurrency = currency.Trim().ToUpperInvariant();
            if (checkvalidcurrency.Length != 3 || !checkvalidcurrency.All(char.IsLetter))
            {
                throw new ValidationException($"Currency '{currency}' is not a valid.");
            }
                
            return new Money(amount, checkvalidcurrency);
        }

        public override string ToString() => $"{Amount} {Currency}";
    }
}
