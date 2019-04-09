using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public class FeedbackproductService : BaseService<Feedbackproduct>
    {
        public FeedbackproductService()
        {
            repository = new GenericRepository<Feedbackproduct>();
        }

        public FeedbackproductService(HeOEntities context)
        {
            repository = new GenericRepository<Feedbackproduct>(context);
        }
    }
}