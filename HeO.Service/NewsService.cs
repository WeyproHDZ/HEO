using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeO.Models;

namespace HeO.Service
{
    public class NewsService : BaseService<News>
    {
        public NewsService()
        {
            repository = new GenericRepository<News>();
        }

        public NewsService(HeOEntities context)
        {
            repository = new GenericRepository<News>(context);
        }
    }
}