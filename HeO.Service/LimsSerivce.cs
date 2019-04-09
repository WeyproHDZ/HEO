using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeO.Models;

namespace HeO.Service
{
    public class LimsSerivce : BaseService<Lims>
    {

        public LimsSerivce()
        {
            repository = new GenericRepository<Lims>();
        }

        public LimsSerivce(HeOEntities context)
        {
            repository = new GenericRepository<Lims>(context);
        }
    }
}