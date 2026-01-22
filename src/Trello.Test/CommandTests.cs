using System.Linq;
using Memstate.Examples.Trello.Core;
using NUnit.Framework;

namespace Trello.Test
{
    public class CommandTests
    {
        private TrelloModel _model;

        [SetUp]
        public void Setup()
        {
            _model = new TrelloModel();
        }

        [Test]
        public void CreateBoard()
        {
            var command = new CreateBoardCommand("A board");
            var id = command.Execute(_model);
            var board = _model.Boards.Values.Single();
            Assert.Equals(id, board.Id);
            Assert.Equals(command.BoardName, board.Name);
        }
    }
}