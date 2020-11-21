using chatbotvk.Core.Models;
using chatbotvk.Core.Services.External;
using chatbotvk.Services.Bank.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace chatbotvk.Services.Bank
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly string _externalApiUrl;
        public ExchangeRateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _externalApiUrl = "https://www.cbr-xml-daily.ru/daily_json.js";
        }
        public async Task<BankResponse> GetCurrentExchangeRatesAsync()
        {
            #region old v1.0
            //HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(_externalEndpoint);
            //HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //string response;
            //using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            //{
            //    response = await streamReader.ReadToEndAsync();
            //}
            //ValuteResponse valuteResponse = JsonConvert.DeserializeObject<ValuteResponse>(response);
            #endregion

            #region new v2.0
            using (var client = new HttpClient())
            {
                // асинхронный запрос.
                HttpResponseMessage httpWebResponse = await client.GetAsync(_externalApiUrl);

                // считываем ответ и переводим в строчный формат.
                byte[] responseBody = await httpWebResponse.Content.ReadAsByteArrayAsync();

                var response = Encoding.Default.GetString(responseBody);

                BankResponse bankResponse = new BankResponse();

                ValuteResponse valuteResponse = JsonConvert.DeserializeObject<ValuteResponse>(response);

                bankResponse.LastRateUpdateDate = valuteResponse.PreviousDate;
                bankResponse.CurrentRateDate = valuteResponse.Date;

                foreach (var valute in valuteResponse.Valute)
                {
                    bankResponse.ActualValue.Add(valute.Key, valute.Value.Value);
                    bankResponse.PreviousValue.Add(valute.Key, valute.Value.Previous);
                }

                return await Task.FromResult(bankResponse);
            }
            #endregion
        }
    }
}
