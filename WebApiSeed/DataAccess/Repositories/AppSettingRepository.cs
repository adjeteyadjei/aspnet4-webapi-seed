using System.Linq;
using WebApiSeed.Models;

namespace WebApiSeed.DataAccess.Repositories
{
    public class AppSettingRepository : BaseRepository<AppSetting>
    {
        public AppSetting Get(string name)
        {
            return DbSet.FirstOrDefault(q => q.Name == name);
        }
    }
}