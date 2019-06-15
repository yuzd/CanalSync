using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Canal.SqlParse.StaticExt
{
    public static class TypeConvertUtils
    {

        public static T JsonToObject<T>(this string str)
        {
            try
            {
                var resultModel = JsonConvert.DeserializeObject<T>(str);
                return resultModel;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static object Parse(object value, Type propType)
        {
            try
            {
                if (null == value)
                {
                    return null;
                }
                else
                {
                    if (propType.Name.Contains("Boolean"))
                    {
                        if (value.ToString().Equals("0"))
                        {
                            return false;
                        }
                        else if (value.ToString().Equals("1"))
                        {
                            return true;
                        }
                        return Convert.ChangeType(value, propType);
                    }
                    else if (propType.Name.Contains("DateTime"))
                    {
                        var t = (DateTime)Convert.ChangeType(value, propType);
                        return t;
                    }
                    else if (!string.IsNullOrEmpty(value.ToString()))
                    {
                        return Convert.ChangeType(value, propType);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
