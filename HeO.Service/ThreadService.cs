using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeO.Models;

namespace HeO.Service
{
    public class ThreadService : BaseService<Thread>
    {
        public ThreadService()
        {
            repository = new GenericRepository<Thread>();
        }

        public ThreadService(HeOEntities context)
        {
            repository = new GenericRepository<Thread>(context);
        }
    }
}