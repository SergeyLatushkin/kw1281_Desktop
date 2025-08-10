using BitFab.KW1281Test.Messengers;
using System.Text;

namespace BitFab.KW1281Test.Blocks
{
    internal class WriteEepromResponseBlock : Block
    {
        public WriteEepromResponseBlock(List<byte> bytes) : base(bytes)
        {
            Dump();
        }

        private void Dump()
        {
            StringBuilder sb = new();
            foreach (var b in Body)
            {
                sb.Append($" {b:X2}");
            }
            Messenger.Instance.AddLine($"Received \"Write EEPROM Response\" block: {sb.ToString()}");
        }
    }
}