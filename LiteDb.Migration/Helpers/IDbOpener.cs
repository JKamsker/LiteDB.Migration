using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Migration.Helpers;

internal interface IDbOpener
{
    LiteDatabase Open();
}

internal class FileDbOpener : IDbOpener
{
    private readonly string _path;

    public FileDbOpener(string path)
    {
        _path = path;
    }

    public LiteDatabase Open()
    {
        return new LiteDatabase(_path);
    }

    public static FileDbOpener CreateNew(string name = "test.db")
    {
        // delete name .db and name-log.db
        if (System.IO.File.Exists(name))
        {
            System.IO.File.Delete(name);
        }

        var logName = Path.GetFileNameWithoutExtension(name) + "-log" + Path.GetExtension(name);
        if (File.Exists(logName))
        {
            File.Delete(logName);
        }

        return new FileDbOpener(name);
    }
}

// MemoryDbOpener is used for testing purposes
internal class MemoryDbOpener : IDbOpener
{
    private MemoryStream _ms;

    public MemoryDbOpener()
    {
        _ms = new MemoryStream();
    }

    public LiteDatabase Open()
    {
        return new LiteDatabase(_ms);
    }

    public static MemoryDbOpener CreateNew()
    {
        return new MemoryDbOpener();
    }
}

internal static class DbOpener
{
    public static IDbOpener File(string path)
    {
        return new FileDbOpener(path);
    }

    public static IDbOpener Memory()
    {
        return new MemoryDbOpener();
    }
}