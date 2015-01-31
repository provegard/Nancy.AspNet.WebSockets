using System;
using System.Threading.Tasks;

namespace Nancy.AspNet.WebSockets.Tests.Integration
{
    internal static class Extensions
    {
        internal static async Task<T> GetResultWithin<T>(this Task<T> task, TimeSpan timeout)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return await task;
            }
            throw new TimeoutException("Timed out waiting for task result");
        }


    }
}
