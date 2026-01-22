using Memstate.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Memstate.Test.Proxy
{

    [TestFixture]
    public class ProxyTest
    {
        private ITestModel _proxy;
        private Engine<ITestModel> _engine;

        [SetUp]
        public async Task Setup()
        {
            var cfg = Config.Reset();
            cfg.UseInMemoryFileSystem();
            ITestModel model = new TestModel();
            _engine = await new EngineBuilder().Build<ITestModel>();
            _proxy = new LocalClient<ITestModel>(_engine).GetDispatchProxy();
        }

        [Test]
        public void CanSetProperty()
        {
            int expected = _proxy.CommandsExecuted + 1;
            _proxy.MyProperty = 42;
            Assert.Equals(expected, _proxy.CommandsExecuted);
        }

        [Test]
        public void CanExecuteCommandMethod()
        {
            _proxy.IncreaseNumber();
            Assert.Equals(1, _proxy.CommandsExecuted);
        }

        [Test]
        public void CanExecuteCommandWithResultMethod()
        {
            Assert.Equals("MEMSTATE", _proxy.Uppercase("memstate"));
            Assert.Equals(1, _proxy.CommandsExecuted);
        }

        [Test]
        [Ignore("Isolation is yet to be implemented")]
        public void ThrowsExceptionOnYieldQuery()
        {
            Assert.Throws<Exception>(() => _proxy.GetNames().Count());
        }

        [Test]
        public void CanExecuteQueryMethod()
        {
            var number = _proxy.GetCommandsExecuted();
            Assert.Equals(0, number);
        }

        [Test]
        [Ignore("Isolation is yet to be implemented")]
        public void QueryResultsAreCloned()
        {
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomers().First();
            Customer robert2 = _proxy.GetCustomers().First();
            Assert.That(robert != robert2);
        }

        [Test]
        public void SafeQueryResultsAreNotCloned()
        {
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomersCloned().First();
            Customer robert2 = _proxy.GetCustomersCloned().First();
            Assert.Equals(robert, robert2);
        }

        [Test]
        [Ignore("Isolation is yet to be designed")]
        public void ResultIsIsolated_attribute_is_recognized()
        {
            var map = MethodMap.MapFor<MethodMapTests.TestModel>();
            var signature = typeof(MethodMapTests.TestModel).GetMethod("GetCustomersCloned").ToString();
            var operationInfo = map.GetOperationInfo(signature);
            Assert.That(operationInfo.OperationAttribute.Isolation.HasFlag(IsolationLevel.Output));
        }

        [Test]
        [Ignore("Isolation is yet to be designed and implemented")]
        public void Query_result_is_cloned()
        {
            var customer = new Customer();
            var clone = _proxy.GenericQuery(customer);
            Assert.That(clone != customer);
            Assert.That(clone is Customer);
        }

        [Test]
        public void GenericCommand()
        {
            _proxy.GenericCommand(DateTime.Now);
            Assert.Equals(1, _proxy.CommandsExecuted);
        }

        [Test]
        public void ComplexGeneric()
        {
            double result = _proxy.ComplexGeneric(new KeyValuePair<string, double>("dog", 42.0));
            Assert.Equals(42.0, result);
            Assert.Equals(1, _proxy.CommandsExecuted);
        }

        [Test]
        public void Indexer()
        {
            _proxy.AddCustomer("Homer");
            Assert.Equals(1, _proxy.CommandsExecuted);

            var customer = _proxy[0];
            Assert.Equals("Homer", customer.Name);

            customer.Name = "Bart";
            _proxy[0] = customer;
            Assert.Equals(2, _proxy.CommandsExecuted);
            var customers = _proxy.GetCustomers();
            Assert.That(customers.Single().Name == "Bart");
        }

        [Test]
        public void DefaultArgs()
        {
            var result = _proxy.DefaultArgs(10, 10);
            Assert.Equals(62, result);

            result = _proxy.DefaultArgs(10, 10, 10);
            Assert.Equals(30, result);
        }

        [Test]
        public void NamedArgs()
        {
            var result = _proxy.DefaultArgs(b: 4, a: 2);
            Assert.Equals(48, result);
        }

        [Test]
        public void ExplicitGeneric()
        {
            var dt = _proxy.ExplicitGeneric<DateTime>();
            Assert.That(dt is DateTime);
            Assert.Equals(default(DateTime), dt);
        }

        [Test]
        public void Proxy_throws_InnerException()
        {
            Assert.Throws<CommandAbortedException>(() =>
            {
                _proxy.ThrowCommandAborted();
            });
        }
    }
}
