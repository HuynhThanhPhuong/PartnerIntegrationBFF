using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mappings
{
    public static class TransactionMapping
    {
        public static TransactionResponse ToDto(this Transaction entity)
        {
            TransactionResponse dto = new TransactionResponse();
            dto.Id = entity.Id;
            dto.PartnerId = entity.PartnerId;
            dto.TransactionReference = entity.TransactionReference;
            dto.Amount = entity.Amount.Amount;
            dto.Currency = entity.Amount.Currency;
            dto.Timestamp = entity.Timestamp;
            dto.Status = entity.Status.ToString();
            return dto;
        }
    }
}
