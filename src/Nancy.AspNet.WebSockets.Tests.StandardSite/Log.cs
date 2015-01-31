using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Nancy.AspNet.WebSockets.Tests.StandardSite
{
    public interface ILog
    {
        void Log(string message);
        IEnumerable<string> GetMessages();
    }

    public class InMemoryLog : ILog
    {
        private readonly ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();
 
        public void Log(string message)
        {
            _messages.Enqueue(message);
        }

        public IEnumerable<string> GetMessages()
        {
            return _messages.ToList(); // create a copy
        }
    }
}