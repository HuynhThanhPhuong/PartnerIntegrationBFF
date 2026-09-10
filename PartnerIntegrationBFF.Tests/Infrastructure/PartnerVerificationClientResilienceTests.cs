using Application.Interfaces;
using Infrastructure.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegrationBFF.Tests.Infrastructure
{
    public class PartnerVerificationClientResilienceTests
    {
        private static IPartnerVerification BuildClient(FakeHttpMessageHandler fakeHandler)
        {
            var services = new ServiceCollection();

            services.AddHttpClient<IPartnerVerification, PartnerVerificationClient>(client =>
            {
                client.BaseAddress = new Uri("http://fake-partner-api.local");
            }).ConfigurePrimaryHttpMessageHandler(() => fakeHandler).AddStandardResilienceHandler();

            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IPartnerVerification>();
        }

        [Fact]
        public async Task VerifyAPI_TransientFailureThenSuccess_RetriesAndReturnsTrue()
        {
            var fakeHandler = new FakeHttpMessageHandler(
                HttpStatusCode.InternalServerError,
                HttpStatusCode.InternalServerError,
                HttpStatusCode.OK);

            var client = BuildClient(fakeHandler);

            var result = await client.VerifyAPIAsync("P-1001");

            Assert.True(result);
            Assert.Equal(3, fakeHandler.CallCount);
        }

        [Fact]
        public async Task VerifyAPI_AllAttemptsFail_ReturnsFalseWithoutThrowing()
        {
            var fakeHandler = new FakeHttpMessageHandler(
                HttpStatusCode.InternalServerError,
                HttpStatusCode.InternalServerError,
                HttpStatusCode.InternalServerError,
                HttpStatusCode.InternalServerError);

            var client = BuildClient(fakeHandler);

            var result = await client.VerifyAPIAsync("P-1001");

            Assert.False(result);
        }
    }
}
