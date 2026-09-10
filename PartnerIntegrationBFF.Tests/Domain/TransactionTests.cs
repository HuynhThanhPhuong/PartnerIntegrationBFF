using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Exceptions;
using Xunit;

namespace PartnerIntegrationBFF.Tests.Domain
{
    public class TransactionTests
    {
        [Fact]
        public void Create_WithValidData_ReturnsPendingTransaction()
        {
            var transaction = Transaction.Create("P-1001", "TXN-99823", 250, "USD", DateTimeOffset.UtcNow);

            Assert.Equal("P-1001", transaction.PartnerId);
            Assert.Equal("TXN-99823", transaction.TransactionReference);
            Assert.Equal(VerificationStatus.Pending, transaction.Status);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithMissingPartnerId_ThrowsValidationException(string? partnerId)
        {
            Assert.Throws<ValidationException>(() => Transaction.Create(partnerId, "TXN-99823", 250, "USD", DateTimeOffset.UtcNow));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithMissingTransactionReference_ThrowsValidationException(string? reference)
        {
            Assert.Throws<ValidationException>(() => Transaction.Create("P-1001", reference, 250, "USD", DateTimeOffset.UtcNow));
        }

        [Fact]
        public void Create_WithInvalidAmount_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => Transaction.Create("P-1001", "TXN-99823", -10, "USD", DateTimeOffset.UtcNow));
        }

        [Fact]
        public void Verified_FromPendingStatus_ChangesStatusToVerified()
        {
            var transaction = Transaction.Create("P-1001", "TXN-99823", 250, "USD", DateTimeOffset.UtcNow);

            transaction.Verified();

            Assert.Equal(VerificationStatus.Verified, transaction.Status);
        }

        [Fact]
        public void Verified_WhenAlreadyVerified_ThrowsValidationException()
        {
            var transaction = Transaction.Create("P-1001", "TXN-99823", 250, "USD", DateTimeOffset.UtcNow);
            transaction.Verified();

            Assert.Throws<ValidationException>(() => transaction.Verified());
        }

        [Fact]
        public void Rejected_FromPendingStatus_ChangesStatusToRejected()
        {
            var transaction = Transaction.Create("P-1001", "TXN-99823", 250, "USD", DateTimeOffset.UtcNow);

            transaction.Rejected();

            Assert.Equal(VerificationStatus.Rejected, transaction.Status);
        }
    }
}
