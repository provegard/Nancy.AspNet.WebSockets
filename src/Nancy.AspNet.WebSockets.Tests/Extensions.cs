using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Nancy.AspNet.WebSockets.Tests
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

        internal static Configuration OpenConfigurationFromResource(this Type type, string relativeResource)
        {
            var assembly = type.Assembly;

            var fakeDll = Path.Combine(new FileInfo(assembly.Location).DirectoryName, "fake.dll");
            File.WriteAllText(fakeDll, "");

            var fullResource = type.Namespace + "." + relativeResource;
            using (var stream = assembly.GetManifestResourceStream(fullResource))
            {
                if (stream == null)
                    throw new ArgumentException("Resource not found: " + fullResource);
                var configFile = fakeDll + ".config";
                if (File.Exists(configFile))
                {
                    File.Delete(configFile);
                }
                using (var fs = File.OpenWrite(fakeDll + ".config"))
                {
                    stream.CopyTo(fs);
                }
            }
            return ConfigurationManager.OpenExeConfiguration(fakeDll);
        }

    }
}
