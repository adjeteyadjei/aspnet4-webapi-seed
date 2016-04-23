using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebApiSeed.DataAccess.Repositories;
using WebApiSeed.Models;
using Newtonsoft.Json;

namespace WebApiSeed.LogicUnit
{
    public class NasaraSmsApi
    {
        public bool Success { get; private set; }
        public string Response { get; private set; }

        public async Task SendMessage(string phoneNumbers, string message)
        {
            var config = GetServerConfig();
            var client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "api_key", config.ApiKey },
                { "phone_numbers", phoneNumbers },
                { "message", message },
                { "sender_id", config.Sender }
            };

            var content = new FormUrlEncodedContent(values);
            var response = client.PostAsync("http://sms.nasaramobile.com/api/v2/sendsms", content).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeAnonymousType(responseString, new { code = 0, status = 0, message = "" });
            ResultHandler(result.message, (result.code == 1816) || (result.code == 1801));
        }

        public string CreditBalance()
        {
            var config = GetServerConfig();
            try
            {
                //Building SMS Request
                var client = new HttpClient();
                var resData = client.GetStringAsync($"http://sms.nasaramobile.com/api/v2/accounts/credit?api_key={config.ApiKey}").Result;
                var data = JsonConvert.DeserializeAnonymousType(resData,
                    new { status = "", data = "", code = "" });

                return string.IsNullOrWhiteSpace(data.data) ? "0" : data.data;
            }
            catch (Exception)
            {
                // _logger.ErrorException("Error getting credit balance", ex);
            }
            return "-";
        }

        private SmsConfig GetServerConfig()
        {
            var repo = new AppSettingRepository();
            return new SmsConfig
            {
                ApiKey = repo.Get(ConfigKeys.SmsApiKey).Value,
                Sender = repo.Get(ConfigKeys.SmsSender).Value,
            };
        }

        private void ResultHandler(string message, bool success = true)
        {
            Success = success;
            Response = message;
        }

    }

}