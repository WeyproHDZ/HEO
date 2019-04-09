using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class GuidelangService : BaseService<Guidelang>
    {
        public GuidelangService()
        {
            repository = new GenericRepository<Guidelang>();
        }

        public GuidelangService(HeOEntities context)
        {
            repository = new GenericRepository<Guidelang>(context);
        }
    }
}