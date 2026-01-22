using NUnit.Framework;

namespace Memstate.Test.Proxy
{
    public class MethodMapTests
    {
        private readonly MethodMap<TestModel> _map = MethodMap.MapFor<TestModel>();

        public class TestModel
        {
            internal void InternalMethod() { }

            internal int InternalQuery()
            {
                return 12;
            }

            private void PrivateMethod()
            {
            }

            public void ImplicitCommand(){}
            
            [Command]
            public int ExplicitCommandWithResult()
            {
                return 42;
            }

            [NoProxy]
            public int NoProxyQuery()
            {
                return 4;
            }

            public int ImplicitQuery()
            {
                return 42;
            }

            public void GenericCommand<T>(T item)
            {
                
            }

            public T GenericQuery<T>(T item)
            {
                return item;
            }

            [Command]
            public TestModel MvccOperation()
            {
                return null;
            }
        }


        [Test]
        public void Maps_are_cached()
        {
            var map = MethodMap.MapFor<TestModel>();
            Assert.Equals(_map, map);
        }

        [Test]
        public void Implicit_command_IsCommand()
        {
            var target = _map. GetOperationInfo("ImplicitCommand");
            Assert.That(target is CommandInfo<TestModel>);
        }

        [Test]
        public void Explicit_command_IsCommand()
        {
            var target = _map.GetOperationInfo("ExplicitCommandWithResult");
            Assert.That(target is CommandInfo<TestModel>);
        }

        [Test]
        public void Implicit_query_IsQuery()
        {
            var target = _map.GetOperationInfo("ImplicitQuery");
            Assert.That(target is QueryInfo<TestModel>);
        }

        [Test]
        public void NoProxy_is_disallowed()
        {
            var target = _map.GetOperationInfo("NoProxyQuery");
            Assert.That(!target.IsAllowed);
        }

        [Test]
        public void Default_ResultIsIsolated_is_false_implicit()
        {
            var target = _map.GetOperationInfo("ImplicitQuery");
            Assert.That(!target.OperationAttribute.Isolation.HasFlag(IsolationLevel.Output));
        }

        [Test]
        public void Default_ResultIsIsolated_is_false_for_explicit()
        {
            var target = _map.GetOperationInfo("ExplicitCommandWithResult");
            Assert.That(!target.OperationAttribute.Isolation.HasFlag(IsolationLevel.Output));
        }

        [Test]
        public void Explicit_ResultIsIsolated_is_reported()
        {
            var target = _map.GetOperationInfo("MvccOperation");
            Assert.That(!target.OperationAttribute.Isolation.HasFlag(IsolationLevel.Output));
        }

        [Test]
        public void Implicit_internal_command_is_mapped()
        {
            //will throw unless exists
            _map.GetOperationInfo("InternalMethod");
        }

        [Test]
        public void Implicit_internal_query_is_mapped()
        {
            _map.GetOperationInfo("InternalQuery");
        }
    }
}