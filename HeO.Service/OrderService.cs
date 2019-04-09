using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class OrderService : BaseService<Order>
    {
        public OrderService()
        {
            repository = new GenericRepository<Order>();
        }

        public OrderService(HeOEntities context)
        {
            repository = new GenericRepository<Order>(context);
        }
    }
}