using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class FeedbackdetailService : BaseService<Feedbackdetail>
    {
        public FeedbackdetailService()
        {
            repository = new GenericRepository<Feedbackdetail>();
        }

        public FeedbackdetailService(HeOEntities context)
        {
            repository = new GenericRepository<Feedbackdetail>(context);
        }
    }
}