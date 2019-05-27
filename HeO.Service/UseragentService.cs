using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class UseragentService : BaseService<Useragent>
    {
        public UseragentService()
        {
            repository = new GenericRepository<Useragent>();
        }

        public UseragentService(HeOEntities context)
        {
            repository = new GenericRepository<Useragent>(context);
        }
    }
}