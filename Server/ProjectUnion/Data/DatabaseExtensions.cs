using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace ProjectUnion.Data
{
    public static class DatabaseExtensions
    {

        public static float? FloatOrNull(this DbDataReader reader, int index)
        {
            if (string.IsNullOrEmpty(reader[index].ToString()) == false)
            {
                return float.Parse(reader[index].ToString());
            }
            return null;
        }
    }
}
