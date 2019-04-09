using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class TermService : BaseService<Term>
    {
        public TermService()
        {
            repository = new GenericRepository<Term>();
        }

        public TermService(HeOEntities context)
        {
            repository = new GenericRepository<Term>(context);
        }
    }
}