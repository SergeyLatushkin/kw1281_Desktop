using BitFab.KW1281Test.Messengers;
using System.Text;

namespace BitFab.KW1281Test.Blocks
{
    internal class UnknownBlock : Block
    {
        public UnknownBlock(List<byte> bytes) : base(bytes)
        {
            Dump();
        }

        private void Dump()
        {
            StringBuilder sb = new();
            foreach (var b in Bytes)
            {
                sb.Append($" 0x{b:X2}");
            }
            Messenger.Instance.AddLine($"Received ${Title:X2} block: {sb.ToString()}");
        }
    }
}