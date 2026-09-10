using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;
using Domain.Entities;
using Application.Mappings;

namespace Application.Services
{
    public class TransactionService
    {
        private readonly IPartnerVerification _verification;
        private readonly ITransactionPublisher _publisher;

        public TransactionService(IPartnerVerification verification, ITransactionPublisher publisher)
        {
            _verification = verification;
            _publisher = publisher;
        }

        public async Task<TransactionResponse> SubmitAsync(TransactionRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ValidationException("TransactionRequest is null.");
            }
            if (!request.Amount.HasValue)
            {
                throw new ValidationException("Amount is required.");
            }

            if (!request.Timestamp.HasValue)
            {
                throw new ValidationException("Timestamp is required.");
            }

            var objTransaction = Transaction.Create(request.PartnerId, request.TransactionReference, request.Amount.Value, request.Currency, request.Timestamp.Value);
            bool VerifyTransaction = await _verification.VerifyAPIAsync(objTransaction.PartnerId);
            if (VerifyTransaction)
            {
                objTransaction.Verified();
                await _publisher.Publish(objTransaction);
            }
            else
            {
                objTransaction.Rejected();
                throw new ValidationException("Partner verification failed.");
            }
            var response = TransactionMapping.ToDto(objTransaction);
            return response;
        }
    }
}
