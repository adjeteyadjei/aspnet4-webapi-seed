using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WebApiSeed.Models
{
    public interface IHasId
    {
        [Key]
        long Id { get; set; }
    }

    public interface ISecured
    {
        bool Locked { get; set; }
        bool Hidden { get; set; }
    }

    public interface IAuditable : IHasId
    {
        [Required]
        string CreatedBy { get; set; }
        [Required]
        string ModifiedBy { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime ModifiedAt { get; set; }
    }

    public class HasId : IHasId
    {
        public long Id { get; set; }
    }

    public class AuditFields : HasId, IAuditable, ISecured
    {
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public string ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool Locked { get; set; }
        public bool Hidden { get; set; }
    }

    public class LookUp : AuditFields
    {
        [MaxLength(512), Required, Index(IsUnique = true)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Notes { get; set; }
    }

    public class User : IdentityUser
    {
        [MaxLength(128), Required]
        public string Name { get; set; }
        public virtual Profile Profile { get; set; }
        public long ProfileId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool Locked { get; set; }
        public bool Hidden { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager, string authenticationType)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            return userIdentity;
        }
    }

    public class Profile : HasId
    {
        [Required, MaxLength(512), Index(IsUnique = true)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Notes { get; set; }
        [MaxLength(500000)]
        public string Privileges { get; set; }
        public bool Locked { get; set; }
        public bool Hidden { get; set; }
        public List<User> Users { get; set; }
    }

    public class AppSetting : AuditFields
    {
        [MaxLength(512), Required, Index(IsUnique = true)]
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class ResetRequest : HasId
    {
        public string Ip { get; set; } = "127.0.0.1";
        public DateTime Date { get; set; } = DateTime.Now;
        public string Email { get; set; }
        public string Token { get; set; }
        public bool IsActive { get; set; } = false;
    }

    public class ResetModel
    {
        public string Token { get; set; }
        public string Password { get; set; }
    }

    public class EmailOutboxEntry
    {
        public DateTime Created { get; set; } = DateTime.Now;
        public long Id { get; set; }
        public bool IsSent { get; set; } = false;
        public DateTime LastAttemptDate { get; set; } = DateTime.Now.AddMinutes(-1);

        [MaxLength(256)]
        public string LastAttemptMessage { get; set; }

        [Required, MaxLength(5120)]
        public string Message { get; set; }

        [MaxLength(256)]
        public string Notes { get; set; }

        [Required, MaxLength(128)]
        public string Receiver { get; set; }

        [Required, MaxLength(128)]
        public string Sender { get; set; }

        [Required, MaxLength(256)]
        public string Subject { get; set; }

        [MaxLength(512)]
        public string FilePath { get; set; }
    }


    public class Message : HasId
    {
        [MaxLength(256), Required]
        public string Recipient { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(128)]
        public string Subject { get; set; }
        [Required]
        public string Text { get; set; }
        public MessageStatus Status { get; set; }
        public MessageType Type { get; set; }
        [MaxLength(5000)]
        public string Response { get; set; }
        public DateTime TimeStamp { get; set; }
        [NotMapped]
        public string Attachment { get; set; }
    }

    public enum MessageType
    {
        SMS,
        Email
    }

    public enum MessageStatus
    {
        Sent,
        Received,
        Failed
    }


}