using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nancy.AspNet.WebSockets.Sample;
using NUnit.Framework;

namespace NancyWebsockets.Tests
{
    [TestFixture]
    public class The_DrawingBoards_service
    {
        private IDrawingBoards _service;

        [SetUp]
        public void Create()
        {
            _service = new DrawingBoards();
        }

        [Test]
        public void Should_return_a_new_drawing_board_the_first_time()
        {
            var board = _service.Get("board");
            Assert.IsNotNull(board);
        }

        [Test]
        public void Should_return_the_same_board_when_requested_with_the_same_name()
        {
            var board1 = _service.Get("board");
            var board2 = _service.Get("board");
            Assert.That(board2, Is.SameAs(board1));
        }

        [Test]
        public void Should_never_return_different_boards_for_same_name_when_used_from_multiple_threads()
        {
            const int threadCount = 5;
            const int getCount = 100;
            var boards = new BlockingCollection<DrawingBoard>();
            var tasks = new List<Task>();

            var allStarted = new SemaphoreSlim(threadCount);
            var startGetting = new ManualResetEventSlim();
            Action getBoard = () =>
            {
                allStarted.Release();
                startGetting.Wait();
                for (var j = 0; j < getCount; j++)
                {
                    boards.Add(_service.Get("test"));
                }
            };
            for (var i = 0; i < threadCount; i++)
            {
                var task = Task.Factory.StartNew(getBoard);
                tasks.Add(task);
            }
            allStarted.Wait();
            startGetting.Set();
            Task.WaitAll(tasks.ToArray());

            var distinctBoardCount = boards.Distinct().Count();
            Assert.That(distinctBoardCount, Is.EqualTo(1));
        }
    }
}
