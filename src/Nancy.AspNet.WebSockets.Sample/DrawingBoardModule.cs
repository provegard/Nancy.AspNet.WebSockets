/* Copyright 2015 Per Rovegard
   Licensed under the MIT license. See LICENSE file in the root of the repo for the full license. */
namespace Nancy.AspNet.WebSockets.Sample
{
    public class DrawingBoardModule : WebSocketNancyModule
    {
        public DrawingBoardModule(IDrawingBoards drawingBoards) : base("/drawing")
        {
            WebSocket["/{path}"] = _ =>
            {
                var board = drawingBoards.Get((string) _.path);
                var user = (string) Request.Query.name ?? "Unknown";
                return board.Register(user);
            };
        }
    }
}