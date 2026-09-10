using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPartnerVerification
    {
        Task<bool> VerifyAPIAsync(string partnerId, CancellationToken cancellationToken = default);
    }
}
