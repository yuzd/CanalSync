using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntData.ORM.Linq;

namespace Canal.SqlParse.Models
{
    public partial class DB : IEntity
    {
        public IQueryable<T> Get<T>() where T : class
        {
            throw new NotImplementedException();
        }
    }
}
