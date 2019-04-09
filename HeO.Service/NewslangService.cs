using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;


namespace HeO.Service
{
    public class NewslangService : BaseService<Newslang>
    {

        public NewslangService()
        {
            repository = new GenericRepository<Models.Newslang>();
        }

        public NewslangService(HeOEntities context)
        {
            repository = new GenericRepository<Newslang>(context);
        }
    }
}