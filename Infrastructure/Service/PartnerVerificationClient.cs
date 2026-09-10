using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;

namespace Infrastructure.Service
{
    public class PartnerVerificationClient : IPartnerVerification
    {
        private readonly HttpClient _httpClient;

        public PartnerVerificationClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> VerifyAPIAsync(string partnerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/partnerverification/{partnerId}", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
