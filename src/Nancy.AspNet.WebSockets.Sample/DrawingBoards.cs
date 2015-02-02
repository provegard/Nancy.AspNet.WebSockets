using System.Collections.Concurrent;

namespace Nancy.AspNet.WebSockets.Sample
{
    public interface IDrawingBoards
    {
        DrawingBoard Get(string name);
    }

    public class DrawingBoards : IDrawingBoards
    {
        private readonly ConcurrentDictionary<string, DrawingBoard> _boards = new ConcurrentDictionary<string, DrawingBoard>();

        public DrawingBoard Get(string name)
        {
            return _boards.GetOrAdd(name, n => new DrawingBoard(n));
        }
    }
}