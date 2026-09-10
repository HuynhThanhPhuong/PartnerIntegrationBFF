using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.ValueObjects;
using Xunit;

namespace PartnerIntegrationBFF.Tests.Domain
{
    public class MoneyTests
    {
        [Fact]
        public void Create_WithValidAmountAndCurrency_ReturnsMoney()
        {
            // Act
            var money = Money.Create(100, "USD");

            // Assert
            Assert.Equal(100, money.Amount);
            Assert.Equal("USD", money.Currency);
        }

        [Fact]
        public void Create_WithZeroAmount_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => Money.Create(0, "USD"));
        }

        [Fact]
        public void Create_WithNegativeAmount_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => Money.Create(-50, "USD"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("US")]
        [InlineData("USDD")]
        [InlineData("12D")]
        public void Create_WithInvalidCurrency_ThrowsValidationException(string invalidCurrency)
        {
            Assert.Throws<ValidationException>(() => Money.Create(100, invalidCurrency));
        }

        [Fact]
        public void Create_NormalizesCurrencyToUppercase()
        {
            var money = Money.Create(100, "usd");

            Assert.Equal("USD", money.Currency);
        }
    }
}
