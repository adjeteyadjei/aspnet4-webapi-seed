using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using RestSharp;
using RestSharp.Authenticators;
using WebApiSeed.Models;

namespace WebApiSeed.AxHelpers
{
    public class MessageHelpers
    {
        public static IRestResponse SendEmailMessage(long id)
        {
            var db = new AppDbContext();
            var eoe = db.EmailOutboxEntries.First(x => x.Id == id && !x.IsSent);
            eoe.LastAttemptDate = DateTime.Now;
            var client = new RestClient
            {
                BaseUrl = new Uri("https://api.mailgun.net/v3"),
                Authenticator = new HttpBasicAuthenticator("api",
                    "key-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")
            };
            var request = new RestRequest();
            //request.
            request.AddParameter("domain",
                "sandboxxxxxxxxxxxxxxxxxxxxxxxxxxxx.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "App Name <mailgun@sandboxxxxxxxxxxxxxx.mailgun.org>");
            request.AddParameter("to", eoe.Receiver);
            request.AddParameter("subject", eoe.Subject);
            request.AddParameter("html", eoe.Message);
            request.AddParameter("text", "App Name");
            request.Method = Method.POST;
            var res = client.Execute(request);
            if (res.StatusCode == HttpStatusCode.OK)
            {
                eoe.IsSent = true;
                eoe.LastAttemptMessage = res.ResponseStatus.ToString();
            }
            else
            {
                eoe.LastAttemptMessage = res.ErrorMessage;
            }

            db.SaveChanges();
            return res;
        }
    }
}