using BitFab.KW1281Test.Messengers;
using System.Text;

namespace BitFab.KW1281Test.Blocks
{
    internal class CustomBlock : Block
    {
        public CustomBlock(List<byte> bytes) : base(bytes)
        {
            // Dump();
        }

        private void Dump()
        {
            Messenger.Instance.AddLine("Received Custom block:");
            StringBuilder sb = new();
            for (var i = 3; i < Bytes.Count - 1; i++)
            {
                sb.Append($" {Bytes[i]:X2}");
            }
            Messenger.Instance.Add(sb.ToString());
        }
    }
}