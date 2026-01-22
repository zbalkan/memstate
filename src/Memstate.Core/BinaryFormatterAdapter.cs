using MessagePack;
using System.IO;

namespace Memstate
{
	public class BinaryFormatterAdapter : BinarySerializer
	{

        public BinaryFormatterAdapter()
        {
            MessagePackSerializer.DefaultOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options;
        }
        public override object ReadObject(Stream stream)
			=> MessagePackSerializer.Deserialize<object>(stream);

		public override void WriteObject(Stream stream, object @object)
			=> MessagePackSerializer.Serialize(stream, @object);
	}
}