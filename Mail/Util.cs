using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Mail;

public static class Util
{

    public static async Task WaitWindow()
    {
        var semaphore = new Semaphore(1, 1);
        semaphore.WaitOne();
        while (Application.Current.Windows.Count < 0)
        {
            await Task.Delay(100);
        }
        semaphore.Release();
    }
}