/* Copyright 2015 Per Rovegard
   Licensed under the MIT license. See LICENSE file in the root of the repo for the full license. */
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