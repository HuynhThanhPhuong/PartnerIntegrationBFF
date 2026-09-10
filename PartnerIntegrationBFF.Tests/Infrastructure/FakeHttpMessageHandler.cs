using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegrationBFF.Tests.Infrastructure
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _responses;

        public int CallCount { get; private set; }
            
        public FakeHttpMessageHandler(params HttpStatusCode[] scriptedResponses)
        {
            _responses = new Queue<HttpStatusCode>(scriptedResponses);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;

            var statusCode = _responses.Count > 0 ? _responses.Dequeue() : HttpStatusCode.InternalServerError;

            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
