using Application.DTOs;
using Application.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/v1/partner/transactions")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionsController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<ActionResult<TransactionResponse>> Post([FromBody] TransactionRequest request, CancellationToken cancellationToken)
        {
            var response = await _transactionService.SubmitAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
