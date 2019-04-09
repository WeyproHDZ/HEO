using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class TermlangService : BaseService<Termlang>
    {
        public TermlangService()
        {
            repository = new GenericRepository<Termlang>();
        }

        public TermlangService(HeOEntities context)
        {
            repository = new GenericRepository<Termlang>(context);
        }
    }
}