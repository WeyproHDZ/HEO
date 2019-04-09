using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class GuideService : BaseService<Guide>
    {
        public GuideService()
        {
            repository = new GenericRepository<Guide>();
        }

        public GuideService(HeOEntities context)
        {
            repository = new GenericRepository<Guide>(context);
        }
    }
}