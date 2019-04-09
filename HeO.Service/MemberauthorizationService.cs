using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class MemberauthorizationService : BaseService<Memberauthorization>
    {
        public MemberauthorizationService()
        {
            repository = new GenericRepository<Memberauthorization>();
        }

        public MemberauthorizationService(HeOEntities context)
        {
            repository = new GenericRepository<Memberauthorization>(context);
        }
    }
}