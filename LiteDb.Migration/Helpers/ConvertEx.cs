using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Migration.Helpers;

internal class ConvertEx
{
    public static int? ToNullableInt32(object value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is int?)
        {
            return (int?)value;
        }
        else if (value is string && int.TryParse((string)value, out int result))
        {
            return result;
        }
        else
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return null;
            }
        }
    }
}