using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeO.Models;

namespace HeO.Service
{
    public class ReturnstatusService : BaseService<Returnstatus>
    {
        public ReturnstatusService()
        {
            repository = new GenericRepository<Returnstatus>();
        }

        public ReturnstatusService(HeOEntities context)
        {
            repository = new GenericRepository<Returnstatus>(context);
        }
    }
}