using Humanizer;
using System;
using System.Linq;
using System.Web.Http;
using WebApiSeed.AxHelpers;
using WebApiSeed.DataAccess.Repositories;
using WebApiSeed.Models;

namespace WebApiSeed.Controllers
{
    [Authorize]
    public class BaseApi<T> : ApiController where T : class
    {
        protected BaseRepository<T> Repository = new BaseRepository<T>();
        private readonly string _klassName = typeof(T).Name.Humanize(LetterCasing.Title);

        public virtual ResultObj Get(long id)
        {
            ResultObj results;
            try
            {
                var data = Repository.Get(id);
                results = WebHelpers.BuildResponse(data, "", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public virtual ResultObj Get()
        {
            ResultObj results;
            try
            {
                var data = Repository.Get();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, data.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public virtual ResultObj Post(T record)
        {
            ResultObj results;
            try
            {
                Repository.Insert(SetAudit(record, true));

                results = WebHelpers.BuildResponse(record, $"New {_klassName} Saved Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        public virtual ResultObj Put(T record)
        {
            ResultObj results;
            try
            {
                Repository.Update(SetAudit(record));

                results = WebHelpers.BuildResponse(record, $"{_klassName} Update Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        public virtual ResultObj Delete(long id)
        {
            ResultObj results;
            try
            {
                Repository.Delete(id);
                results = WebHelpers.BuildResponse(id, $"{_klassName} Deleted Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        protected T SetAudit(T record, bool isNew = false)
        {
            if (isNew)
            {
                if (typeof(T).GetProperty(GenericProperties.CreatedBy) != null)
                    typeof(T).GetProperty(GenericProperties.CreatedBy).SetValue(record, User.Identity.Name);
            }

            if (typeof(T).GetProperty(GenericProperties.ModifiedBy) != null)
                typeof(T).GetProperty(GenericProperties.ModifiedBy).SetValue(record, User.Identity.Name);

            return record;
        }
    }
}