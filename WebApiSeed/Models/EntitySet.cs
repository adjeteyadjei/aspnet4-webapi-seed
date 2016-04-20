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

    public class AppSetting : LookUp { }

}