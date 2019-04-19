using System;
using System.Collections.Generic;
using System.Text;
using MysqlCanalMq.Common.Models.canal;

namespace MysqlCanalMq.Common.Interface
{
    public interface IProduce
    {
        void Produce(DataChange message);
    }
}
