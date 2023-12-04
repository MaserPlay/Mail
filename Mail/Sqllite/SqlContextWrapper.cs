using System;
using System.Threading.Tasks;

namespace Mail.Sqllite;

public static class SqlContextWrapper<R>
{
    public static R exec(Func<SqlContext, R> func)
    {
        SqlContext context = new SqlContext();
        return func(context);
    }
    public static async Task<R> execAsync(Func<SqlContext, Task<R>> func)
    {
        var context = new SqlContext();
        return await func(context);
    }
}
public static class SqlContextWrapper
{
    public static void exec(Action<SqlContext> func)
    {
        SqlContext context = new SqlContext();
        func(context);
    }
    public static async Task execAsync(Func<SqlContext, Task> func)
    {
        SqlContext context = new SqlContext();
        await func(context);
    }
}