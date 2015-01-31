using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Nancy.AspNet.WebSockets.Tests.Integration
{
    // Based on http://www.reimers.dk/jacob-reimers-blog/testing-your-web-application-with-iis-express-and-unit-tests,
    // slightly modified.
    public class IisExpressBasedTest
    {
        private Process _iisProcess;
        private readonly string _webAppPath;

        private int _port;

        protected IisExpressBasedTest(string siteName)
        {
            // Find the path that we will point IIS Express to.
            _webAppPath = FindPath(siteName);
        }

        protected string GetHost()
        {
            return "localhost:" + _port;
        }

        private string FindPath(string siteName)
        {
            // Find the directory of our DLL
            var dir = new FileInfo(new Uri(GetType().Assembly.CodeBase).AbsolutePath).Directory;

            // Walk up the parent hierarchy until we find a csproj file. When we do, step up one
            // more level and look for a directory with the same name as our own assembly name
            // but with the site name as suffix.
            while (dir != null && !dir.EnumerateFiles("*.csproj").Any())
            {
                dir = dir.Parent;
            }

            // ReSharper shadow-copies DLLs by default, in case we won't find a project file.
            // Shadow-copying can be disabled in the ReSharper options.
            if (dir == null)
                throw new ArgumentException(
                    "Project file of the test assembly not found. Make sure shadow-copying is disabled.");

            // The test sites are in directories adjacent to the test project directory.
            var siteDir = SimpleName(GetType().Assembly) + "." + siteName;
            var sitePath = Path.Combine(dir.Parent.FullName, siteDir);
            if (!Directory.Exists(sitePath))
                throw new ArgumentException("No site found: " + siteName);
            return sitePath;
        }

        private string SimpleName(Assembly a)
        {
            return a.GetName().Name;
        }

        [TestFixtureSetUp]
        public void StartServer()
        {
            // Create a TaskCompletionSource that we will wait on. This way, tests won't run until
            // IIS Express has started, and on top of that we will get any startup exception that
            // occurs on the start thread. The generic type parameter used here is not important.
            var tcs = new TaskCompletionSource<bool>();

            // Start the thread, passing our TaskCompletionSource
            var thread = new Thread(StartIisExpress) { IsBackground = true };
            thread.Start(tcs);

            // Wait on the TaskCompletionSource. Throws if IIS Express couldn't be started, which
            // will prevent tests from running.
            tcs.Task.Wait();
        }

        [TestFixtureTearDown]
        public void StopServer()
        {
            _iisProcess.CloseMainWindow();
            _iisProcess.Dispose();
        }

        private void StartIisExpress(object tcsObj)
        {
            var tcs = tcsObj as TaskCompletionSource<bool>;
            _port = new Random().Next(60100, 60999);
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                ErrorDialog = true,
                LoadUserProfile = true,
                CreateNoWindow = false,
                UseShellExecute = false,
                Arguments = string.Format("/path:\"{0}\" /port:{1}", _webAppPath, _port)
            };

            var programfiles = string.IsNullOrEmpty(startInfo.EnvironmentVariables["programfiles"])
                                ? startInfo.EnvironmentVariables["programfiles(x86)"]
                                : startInfo.EnvironmentVariables["programfiles"];

            startInfo.FileName = programfiles + "\\IIS Express\\iisexpress.exe";

            try
            {
                _iisProcess = new Process { StartInfo = startInfo };
                _iisProcess.Start();

                // Indicate that IIS Express could be started successfully.
                tcs.SetResult(true);
                
                _iisProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                _iisProcess.CloseMainWindow();
                _iisProcess.Dispose();
                
                // Indicate the error to the TaskCompletionSource, which will cause the exception
                // to be re-thrown on the test runner thread that is waiting on the TCS.
                tcs.SetException(ex);
            }
        }
    }
}
