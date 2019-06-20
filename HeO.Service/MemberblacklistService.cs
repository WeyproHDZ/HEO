using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class MemberblacklistService : BaseService<Memberblacklist>
    {
        public MemberblacklistService()
        {
            repository = new GenericRepository<Memberblacklist>();
        }

        public MemberblacklistService(HeOEntities context)
        {
            repository = new GenericRepository<Memberblacklist>(context);
        }
    }
}