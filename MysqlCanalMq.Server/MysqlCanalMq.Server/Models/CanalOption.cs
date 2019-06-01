//-----------------------------------------------------------------------
// <copyright file="CanalOption .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

namespace MysqlCanalMq.Server.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// canl param
    /// </summary>
    public class CanalOption
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Destination { get; set; }
        public string MysqlName { get; set; }
        public string MysqlPwd { get; set; }
        public int Timer { get; set; }
        public int GetCountsPerTimes { get; set; }
        public List<string> OutType { get; set; }

    }
}