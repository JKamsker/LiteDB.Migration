using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Migration.Tests.Assertions;

public class ConversionAssertions
{
    // Try to convert a (object)int to a (int?)int
    [Fact]
    public void Convert_IntToObjectInt_To_IntToObjectInt()
    {
        object value = 1;
        var result = ConvertIntToObjectInt(value);
        Assert.Equal(value, result);
    }

    [Fact]
    public void Convert_StringToObjectInt_To_IntToObjectInt()
    {
        object value = "1";
        var result = ConvertIntToObjectInt(value);
        Assert.Equal(1, result);
    }

    // convert (obj)long to (int?)int
    [Fact]
    public void Convert_LongToObjectInt_To_IntToObjectInt()
    {
        object value = 1L;
        var result = ConvertIntToObjectInt(value);
        Assert.Equal(1, result);
    }

    // (object)null => (int?)null
    // (object)int => (int?)int
    // (object)string => (int?)int // convert
    private int? ConvertIntToObjectInt(object value)
    {
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