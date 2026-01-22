using NUnit.Framework;

namespace Memstate.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Memstate.Configuration;
    using Memstate.Models;
    using Memstate.Models.KeyValue;
    using Memstate.Tcp;

    public class SessionTests
    {
        private  Session<KeyValueStore<int>> _session;
        private KeyValueStore<int> _testModel;
        private List<Message> _messagesEmitted;

        [SetUp]
        public async Task PerTestSetup()
        {
            var cfg = Config.Reset();
            cfg.UseInMemoryFileSystem();
            _testModel = new KeyValueStore<int>();
            var engine = await new EngineBuilder().Build(_testModel);
            _session = new Session<KeyValueStore<int>>(engine);
            _messagesEmitted = new List<Message>();
            _session.OnMessage += _messagesEmitted.Add;
        }

        [Test]
        public async Task Incompatible_command_emits_ExceptionResponse()
        {
            var command = new ProxyCommand<string>("dummy", null, null);
            var request = new CommandRequest(command);
            await _session.Handle(request);
            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.Equals(request.Id, response.ResponseTo);
            Assert.That(response.Exception is InvalidCastException);
        }

        [Test]
        public async Task Query_that_throws_exception_emits_ExceptionResponse()
        {
            var query = new FailingQuery();
            var request = new QueryRequest(query);

            await _session.Handle(request);

            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.Equals(request.Id, response.ResponseTo);
        }

        [Test]
        public async Task Command_that_throws_exception_emits_ExceptionResponse()
        {
            var command = new Remove<int>("NON_EXISTING_KEY");
            var request = new CommandRequest(command);

            await _session.Handle(request);

            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.Equals(request.Id, response.ResponseTo);
        }

        [Test]
        public async Task QueryRequest_happy_path()
        {
            _testModel.Set("KEY", 42);
            var query = new Get<int>("KEY");
            var queryRequest = new QueryRequest(query);

            await _session.Handle(queryRequest);

            Assert.That(_messagesEmitted.Count == 1);
            var response = (QueryResponse)_messagesEmitted.Single();
            Assert.Equals(queryRequest.Id, response.ResponseTo);
            var node = (KeyValueStore<int>.Node)response.Result;
            Assert.Equals(42, node.Value);
        }

        [Test]
        public async Task Command_with_result_happy_path()
        {
            var commandRequest = new CommandRequest(new Set<int>("KEY", 42));
            await _session.Handle(commandRequest);
            var response = AssertAndGetSingle<CommandResponse>();
            Assert.Equals(commandRequest.Id, response.ResponseTo);
            Assert.Equals(1, (int)response.Result);
        }

        [Test]
        public async Task Void_command_happy_path()
        {
            _testModel.Set("KEY", 100);
            var request = new CommandRequest(new Remove<int>("KEY"));

            await _session.Handle(request);

            Assert.Equals(0, _testModel.Count());

            var response = AssertAndGetSingle<CommandResponse>();
            Assert.Equals(request.Id, response.ResponseTo);
            Assert.That(null == response.Result);
        }

        [Test]
        public async Task PingPong()
        {
            var request  = new Ping();
            await _session.Handle(request);
            var pong = AssertAndGetSingle<Pong>();
            Assert.Equals(request.Id, pong.ResponseTo);
        }

        [Test]
        public async Task UnhandledMessageException()
        {
            var message = new UnknownMessage();
            await _session.Handle(message);

            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.That(response.Exception is Exception);
            Assert.Equals(message.Id, response.ResponseTo);
        }

        private T AssertAndGetSingle<T>() where T : Message
        {
            Assert.That(_messagesEmitted.Count == 1);
            var message = _messagesEmitted.Single();
            Assert.That(message is T);
            return (T)message;
        }

        internal class UnknownMessage : Message
        {
        }

        internal class FailingQuery : Query<KeyValueStore<int>, int>
        {
            public override int Execute(KeyValueStore<int> db)
            {
                throw new Exception("the innermost exception");
            }
        }
    }
}