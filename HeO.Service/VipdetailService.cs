using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class VipdetailService : BaseService<Vipdetail>
    {
        public VipdetailService()
        {
            repository = new GenericRepository<Vipdetail>();
        }

        public VipdetailService(HeOEntities context)
        {
            repository = new GenericRepository<Vipdetail>(context);
        }
    }
}