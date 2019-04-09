using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeO.Models;


namespace HeO.Service
{
    public class AdminLimsService : BaseService<AdminLims>
    {
        public AdminLimsService()
        {
            repository = new GenericRepository<AdminLims>();
        }

        public AdminLimsService(HeOEntities context)
        {
            repository = new GenericRepository<AdminLims>(context);
        }
    }
}