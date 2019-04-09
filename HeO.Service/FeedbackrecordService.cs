using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class FeedbackrecordService : BaseService<Feedbackrecord>
    {
        public FeedbackrecordService()
        {
            repository = new GenericRepository<Feedbackrecord>();
        }

        public FeedbackrecordService(HeOEntities context)
        {
            repository = new GenericRepository<Feedbackrecord>(context);
        }
    }
}