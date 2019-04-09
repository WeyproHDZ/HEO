using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class SettingService : BaseService<Setting>
    {

        public SettingService()
        {
            repository = new GenericRepository<Setting>();
        }

        public SettingService(HeOEntities context)
        {
            repository = new GenericRepository<Setting>(context);
        }
    }
}