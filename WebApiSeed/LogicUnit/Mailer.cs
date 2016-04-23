using System;
using System.Collections.Generic;
using System.Net.Http;
using WebApiSeed.DataAccess.Repositories;
using WebApiSeed.Models;
using Newtonsoft.Json;

namespace WebApiSeed.LogicUnit
{
    public class Mailer
    {
        public bool Success { get; private set; }
        public string Response { get; private set; }

        private void ResultHandler(string message, bool success = true)
        {
            Success = success;
            Response = message;
        }

        public void Send(string receipient, string subject, string message)
        {
            var config = GetServerConfig();
            var client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "username", config.AccountName },
                { "apikey", config.ApiKey },
                { "from", "messager@masafo.com" },
                { "from_name", config.Sender },
                { "sender", "messager@masafo.com" },
                { "sender_name", config.Sender },
                { "to",  receipient.Replace(',', ';')},
                { "subject", subject },
                { "body_text", message }
            };


            var content = new FormUrlEncodedContent(values);
            var response = client.PostAsync("https://api.elasticemail.com/v2/email/send", content).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            if (responseString == "") throw new Exception("No response from mail server.");
            var result = JsonConvert.DeserializeAnonymousType(responseString, new { success = false, error = "", data = new { transactionId = "" } });
            ResultHandler(result.error, result.success);
        }

        private EmailConfig GetServerConfig()
        {
            var repo = new AppSettingRepository();
            return new EmailConfig
            {
                ApiKey = repo.Get(ConfigKeys.EmailApiKey).Value,
                Sender = repo.Get(ConfigKeys.EmailSender).Value,
                AccountName = repo.Get(ConfigKeys.EmailAccountName).Value,
            };
        }
    }
}