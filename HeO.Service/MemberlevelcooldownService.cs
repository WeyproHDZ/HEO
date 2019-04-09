using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class MemberlevelcooldownService : BaseService<Memberlevelcooldown>
    {
        public MemberlevelcooldownService()
        {
            repository = new GenericRepository<Memberlevelcooldown>();
        }

        public MemberlevelcooldownService(HeOEntities context)
        {
            repository = new GenericRepository<Memberlevelcooldown>(context);
        }
    }
}