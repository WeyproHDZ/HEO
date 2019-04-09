﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeO.Models;

namespace HeO.Service
{
    public class AdminsService : BaseService<Admins>
    {
        public AdminsService()
        {
            repository = new GenericRepository<Admins>();
        }

        public AdminsService(HeOEntities context)
        {
            repository = new GenericRepository<Admins>(context);
        }
    }
}
