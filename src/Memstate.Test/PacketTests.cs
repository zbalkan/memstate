using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Tcp;
using NUnit.Framework;

namespace Memstate.Test
{
    public class PacketTests
    {
        private static IList<int> _payloadSizes = new List<int>
        {
            0,
            127,
            4523
        };

        [Test]
        public void IsTerminal_is_default_when_calling_constructor()
        {
            var packet = new Packet();
            Assert.That(packet.IsTerminal);
        }

        [Test]
        public void IsTerminal_is_default_when_creating_from_factory_method()
        {
            Packet packet = Packet.Create(new byte[42], 42);
            Assert.That(packet.IsTerminal);
        }

        [TestCaseSource(nameof(_payloadSizes))]
        public async Task When_writing_to_stream_Size_property_corresponds_to_bytes_written(int payloadSize)
        {
            Packet packet = Packet.Create(new byte[payloadSize], 1);

            var memoryStream = new MemoryStream();
            await packet.WriteTo(memoryStream);
            Assert.Equals(memoryStream.Length, packet.Size);
        }

        [TestCaseSource(nameof(_payloadSizes))]
        public async Task ReadAsync_returns_identical_packet(int payloadSize)
        {
            //Arrange
            Packet packet = Packet.Create(new byte[payloadSize], 1);
            var stream = new MemoryStream();
            await packet.WriteTo(stream);
            stream.Position = 0;
            var token = new CancellationToken();

            //Act
            Packet copy = await Packet.Read(stream, token);

            //Assert
            Assert.Equals(packet.Size, copy.Size);
            Assert.Equals(packet.MessageId, copy.MessageId);
            Assert.Equals(packet.Info, copy.Info);
            Assert.Equals(packet.Payload.Length, copy.Payload.Length);
        }

        [Ignore("Doesn't terminate")]
        [TestCaseSource(nameof(_payloadSizes))]
        public async Task When_stream_is_blocked_cancelling_throws_a_TaskCancelledException(int payloadSize)
        {
            //Arrange
            Packet packet = Packet.Create(new byte[payloadSize], 1);
            using (var stream = new MemoryStream())
            {
                await packet.WriteTo(stream);
                stream.SetLength(stream.Length - 3);
                var cancellationSource = new CancellationTokenSource();
                stream.Position = 0;

                //Act
                var task = Packet.Read(stream, cancellationSource.Token);
                // await Task.Delay(TimeSpan.FromMilliseconds(5));

                //Assert
                Assert.That(!task.IsCompleted);
                Assert.ThrowsAsync<TaskCanceledException>(async () =>
                {
                    cancellationSource.Cancel();
                    await task;
                });

                Assert.That(task.IsCompleted);
                Assert.That(task.IsFaulted);
            }
        }
    }
}