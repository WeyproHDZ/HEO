using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;
namespace HeO.Service
{
    public class MemberloginrecordService : BaseService<Memberloginrecord>
    {
        public MemberloginrecordService()
        {
            repository = new GenericRepository<Memberloginrecord>();
        }

        public MemberloginrecordService(HeOEntities context)
        {
            repository = new GenericRepository<Memberloginrecord>(context);
        }
    }
}