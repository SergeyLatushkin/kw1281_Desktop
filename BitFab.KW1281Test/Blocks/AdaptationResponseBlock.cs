using BitFab.KW1281Test.Messengers;
using System.Text;

namespace BitFab.KW1281Test.Blocks
{
    internal class AdaptationResponseBlock : Block
    {
        public AdaptationResponseBlock(List<byte> bytes) : base(bytes)
        {
            Dump();
        }

        public byte ChannelNumber => Body[0];

        public ushort ChannelValue => (ushort)(Body[1] * 256 + Body[2]);

        private void Dump()
        {
            StringBuilder sb = new();
            foreach (var b in Body)
            {
                sb.Append($" {b:X2}");
            }
            Messenger.Instance.AddLine($"Received \"Adaptation Response\" block: {sb.ToString()}");
        }
    }
}
