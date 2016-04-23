using System.Collections.Generic;
using WebApiSeed.Models;

namespace WebApiSeed.LogicUnit
{
    public class ThemeConfig
    {
        public string Title { get; set; }
        public string Background { get; set; }
        public string Logo { get; set; }
        public string ToolbarColour { get; set; }
    }

    public class MessageTemplateConfig
    {
        public string ContributionReceipt { get; set; }
        public string BirthdayMessage { get; set; }
        public string PledgeReminder { get; set; }
    }

    public class ChurchConfig
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }

    public class EmailConfig
    {
        public string AccountName { get; set; }
        public string ApiKey { get; set; }
        public string Sender { get; set; }
    }

    public class SmsConfig
    {
        public string ApiKey { get; set; }
        public string Sender { get; set; }
    }

    public class Config
    {
        public ThemeConfig Theme { get; set; }
        public MessageTemplateConfig Message { get; set; }
        public ChurchConfig Church { get; set; }
        public EmailConfig Email { get; set; }
        public SmsConfig Sms { get; set; }
    }

    public class Contact
    {
        public string Info { get; set; }
        public ContactType Type { get; set; }
        public long RecordId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class BulkMessage
    {
        public string Subject { get; set; }
        public string Text { get; set; }
        public MessageType Type { get; set; }
        public virtual List<Contact> Contacts { get; set; }
    }

    public enum ContactType
    {
        All,
        Group
    }
}