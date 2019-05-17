using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeO.Models;

namespace HeO.Service
{
    public interface IMembersService
    {
        IResult Create(Members entity);
        IResult Update(Members entity);
        IResult SpecificUpdate(Members entity, string[] Includeproperties);
        IResult Delete(object id);
        IResult Delete(Members entity);
        Members GetByID(object id);
        IEnumerable<Members> Get();
        void SaveChanges();
    }
}