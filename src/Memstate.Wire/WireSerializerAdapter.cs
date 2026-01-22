using MessagePack;
using System.IO;
namespace Memstate.Wire
{
    public class WireSerializerAdapter : BinarySerializer
    {
        public WireSerializerAdapter()
        {

            MessagePackSerializer.DefaultOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options;
        }

        public override void WriteObject(Stream stream, object @object)
        {
            if (@object is JournalRecord[])
            {
                foreach (var record in (@object as JournalRecord[]))
                {
                    stream.Write(MessagePackSerializer.Serialize(record));
                }
            }
            else stream.Write(MessagePackSerializer.Serialize(@object));
        }

        public override object ReadObject(Stream stream)
        {
            return MessagePackSerializer.Deserialize<object>(stream);
        }
    }
}