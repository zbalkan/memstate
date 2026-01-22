using Memstate.Models.Redis;
using NUnit.Framework;

namespace Memstate.Test.Models.Redis
{
    [TestFixture]
    public class RangeTests
    {
        [Test]
        public void Flip()
        {
            var target = new Range(2, 6, 8).Flip(8);
            
            Assert.Equals(1, target.FirstIdx);
            Assert.Equals(5, target.LastIdx);
        }

        [Test]
        public void Positive()
        {
            var target = new Range(0, 99);
            
            Assert.Equals(0, target.FirstIdx);
            Assert.Equals(99, target.LastIdx);
            Assert.Equals(100, target.Length);
        }
        
        [Test]
        public void Negative()
        {
            var target = new Range(0, -2, 100);
            
            Assert.Equals(0, target.FirstIdx);
            Assert.Equals(98, target.LastIdx);
            Assert.Equals(99, target.Length);
        }
    }
}