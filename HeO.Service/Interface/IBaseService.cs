using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeO.Service
{
    public interface IBaseService<TEntity>
    {
        IResult Create(TEntity entity);
        IResult Update(TEntity entity);
        IResult Delete(object id);
        IResult Delete(TEntity entity);
        TEntity GetByID(object id);
        IQueryable<TEntity> Get();
        void SaveChanges();
    }
}
