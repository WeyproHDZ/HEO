using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class OrderfacebooklistService : BaseService<Orderfaceooklist>
    {
        public OrderfacebooklistService()
        {
            repository = new GenericRepository<Orderfaceooklist>();
        }

        public OrderfacebooklistService(HeOEntities context)
        {
            repository = new GenericRepository<Orderfaceooklist>(context);
        }
    }
}