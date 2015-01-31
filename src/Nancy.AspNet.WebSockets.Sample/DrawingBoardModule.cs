namespace Nancy.AspNet.WebSockets.Sample
{
    public class DrawingBoardModule : WebSocketNancyModule
    {
        public DrawingBoardModule(IDrawingBoards drawingBoards) : base("/drawing")
        {
            WebSocket["/{path}"] = _ =>
            {
                var board = drawingBoards.Get((string) _.path);
                var user = Request.Query.name ?? "Unknown";
                return board.Register(user);
            };
        }
    }
}