using System;
using System.Threading.Tasks;
using WebApiSeed.AxHelpers;
using WebApiSeed.DataAccess.Repositories;
using WebApiSeed.Models;

namespace WebApiSeed.LogicUnit
{
    public class Messenger
    {
        readonly NasaraSmsApi _smsApi = new NasaraSmsApi();
        readonly Mailer _mailer = new Mailer();
        readonly BaseRepository<Message> _messageRepo = new BaseRepository<Message>();

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public async Task<Message> Send(Message message)
        {
            switch (message.Type)
            {
                case MessageType.SMS:
                    await SendSMS(message);
                    return message;
                case MessageType.Email:
                    await SendEmail(message);
                    return message;
                default:
                    message.Status = MessageStatus.Failed;
                    message.Response = "No implementation for this message type.";
                    SaveMessage(message);
                    return message;
            }
        }

        public async void SendAsync(Message message)
        {
            switch (message.Type)
            {
                case MessageType.SMS:
                    await SendSMS(message);
                    break;
                case MessageType.Email:
                    await SendEmail(message);
                    break;
                default:
                    message.Status = MessageStatus.Failed;
                    message.Response = "No implementation for this message type.";
                    SaveMessage(message);
                    break;
            }
        }



        /// <summary>
        /// Saves the message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void SaveMessage(Message message)
        {
            message.TimeStamp = DateTime.UtcNow;
            if (message.Id > 0) { _messageRepo.Update(message); }
            else { _messageRepo.Insert(message); }
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="message">The message.</param>
        private async Task SendEmail(Message message)
        {
            try
            {
                _mailer.Send(message.Recipient, message.Subject, message.Text);
                message.Status = _mailer.Success ? MessageStatus.Sent : MessageStatus.Failed;
                message.Response = _mailer.Response;
            }
            catch (Exception ex)
            {
                message.Status = MessageStatus.Failed;
                var err = WebHelpers.ProcessException(ex).Message;
                if (err.StartsWith("The remote name could not be resolved")) err = "Unable to reach Email Server. Make sure application server is connected to the internet.";
                message.Response = err;
            }
            finally
            {
                SaveMessage(message);
            }
        }

        /// <summary>
        /// Sends the SMS.
        /// </summary>
        /// <param name="message">The message.</param>
        private async Task SendSMS(Message message)
        {
            try
            {
                await _smsApi.SendMessage(message.Recipient, message.Text);
                message.Status = _smsApi.Success ? MessageStatus.Sent : MessageStatus.Failed;
                message.Response = _smsApi.Response;
            }
            catch (Exception ex)
            {
                message.Status = MessageStatus.Failed;
                var err = WebHelpers.ProcessException(ex).Message;
                if (err.StartsWith("The remote name could not be resolved")) err = "Unable to reach SMS Server. Make sure application server is connected to the internet.";
                message.Response = err;
            }
            finally
            {
                SaveMessage(message);
            }
        }
    }
}