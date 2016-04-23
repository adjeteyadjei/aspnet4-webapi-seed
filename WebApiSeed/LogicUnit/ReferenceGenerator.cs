using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApiSeed.DataAccess.Filters;
using WebApiSeed.DataAccess.Repositories;
using WebApiSeed.Models;

namespace WebApiSeed.LogicUnit
{
    public class ReferenceGenerator
    {
        internal static string MemberId()
        {
            /*
             * Reference Number ::: [HOH/15/MY/001]
             */
            var num = 0;
            var term = $"HOH/{DateTime.Today.ToString("yy/MM")}/".ToUpper();
            //var app = new BaseRepository<Member>().Get(new MemberFilter{ Reference = term }).LastOrDefault();
            //if (app != null) num = int.Parse(app.Number.Split('/').LastOrDefault() ?? "0");

            return $"{term}{(num + 1).ToString("000").ToUpper()}";
        }

        
    }
}