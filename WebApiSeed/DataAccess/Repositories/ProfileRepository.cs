using WebApiSeed.Models;

namespace WebApiSeed.DataAccess.Repositories
{
    public class ProfileRepository : BaseRepository<Profile>
    {
        public override void Update(Profile entity)
        {
            var theProfile = DbSet.Find(entity.Id);
            theProfile.Name = entity.Name;
            theProfile.Privileges = entity.Privileges;
            theProfile.Notes = entity.Notes;
            SaveChanges();
        }
    }
}