using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class MembersService : BaseService<Members>
    {
        public MembersService()
        {
            repository = new GenericRepository<Members>();
        }

        public MembersService(HeOEntities context)
        {
            repository = new GenericRepository<Members>(context);
        }
    }
}