using Memstate.Examples.Trello.Core;
using NUnit.Framework;

namespace Trello.Test
{
    public class QueryTests
    {
        private TrelloModel _model;

        [SetUp]
        public void Setup()
        {
            _model = new TrelloModel();
        }

        [Test]
        public void GetBoards()
        {
            _model.CreateBoard("c");
            _model.CreateBoard("a");
            _model.CreateBoard("B");

            var query = new GetBoardsQuery();
            var boards = query.Execute(_model);
            Assert.Equals(3, boards.Count);
            Assert.Equals("a", boards[0].Name);
            Assert.Equals("B", boards[1].Name);
            Assert.Equals("c", boards[2].Name);
        }
    }
}