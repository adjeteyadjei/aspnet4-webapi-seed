using System;
using System.Linq;
using WebApiSeed.Models;

namespace WebApiSeed.DataAccess.Filters
{
    public class UserFilter : Filter<User>
    {
        public long ProfileId;

        public override IQueryable<User> BuildQuery(IQueryable<User> query)
        {
            if (ProfileId > 0) query = query.Where(q => q.Profile.Id == ProfileId);

            query = query.Where(q => !q.Hidden);
            return query;
        }
    }
    
}