using System.Linq;

namespace WebApiSeed.DataAccess.Filters
{
    public abstract class Filter<T>
    {
        public abstract IQueryable<T> BuildQuery(IQueryable<T> query);
        public Pager Pager = new Pager();
    }

    public class Pager
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public int Skip(){return (Page - 1) * Size;}
    }
}