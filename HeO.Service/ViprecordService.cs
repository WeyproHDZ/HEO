using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class ViprecordService : BaseService<Viprecord>
    {
        public ViprecordService()
        {
            repository = new GenericRepository<Viprecord>();
        }

        public ViprecordService(HeOEntities context)
        {
            repository = new GenericRepository<Viprecord>(context);
        }
    }
}