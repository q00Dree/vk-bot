using chatbotvk.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace chatbotvk.Core.Services.External
{
    public interface IExchangeRateService
    {
        Task<BankResponse> GetCurrentExchangeRatesAsync();
    }
}
