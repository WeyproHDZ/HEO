using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class ServicelogService : BaseService<Servicelog>
    {
        public ServicelogService()
        {
            repository = new GenericRepository<Servicelog>();
        }

        public ServicelogService(HeOEntities context)
        {
            repository = new GenericRepository<Servicelog>(context);
        }
    }
}