using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class MemberlevelService : BaseService<Memberlevel>
    {
        public MemberlevelService()
        {
            repository = new GenericRepository<Memberlevel>();
        }

        public MemberlevelService(HeOEntities context)
        {
            repository = new GenericRepository<Memberlevel>(context);
        }
    }
}