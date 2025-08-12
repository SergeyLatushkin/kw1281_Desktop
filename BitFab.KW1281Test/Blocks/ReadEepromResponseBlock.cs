using BitFab.KW1281Test.Actions;
using System.Text;

namespace BitFab.KW1281Test.Blocks
{
    internal class ReadEepromResponseBlock : Block
    {
        public ReadEepromResponseBlock(List<byte> bytes) : base(bytes)
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
            Messenger.Instance.AddLine($"Received \"Read EEPROM Response\" block: {sb.ToString()}");
        }
    }
}