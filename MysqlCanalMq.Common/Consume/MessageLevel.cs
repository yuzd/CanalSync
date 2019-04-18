﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MysqlCanalMq.Common.Consume
{
    public enum MessageLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6
    }
}
