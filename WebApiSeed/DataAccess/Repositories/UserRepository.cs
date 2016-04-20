using System;
using System.Linq;
using WebApiSeed.Models;

namespace WebApiSeed.DataAccess.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        public User Get(string username)
        {
            return DbSet.SingleOrDefault(x => x.UserName == username);
        }

        public override void Update(User entity)
        {
            var theUser = DbSet.Find(entity.Id);
            if (theUser == null) throw new Exception("User not found to update.");
            theUser.Name = entity.Name;
            theUser.Email = entity.Email;
            theUser.ProfileId = entity.ProfileId;
            theUser.PhoneNumber = entity.PhoneNumber;
            theUser.Email = entity.Email;

            SaveChanges();
        }

        public void Delete(string id)
        {
            var user = DbSet.Find(id);
            if(user.Locked) throw new Exception(ExceptionMessage.RecordLocked);
            DbSet.Remove(user);
            SaveChanges();
        }
    }
}