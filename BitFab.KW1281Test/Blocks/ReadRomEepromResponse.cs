using BitFab.KW1281Test.Messengers;
using System.Text;

namespace BitFab.KW1281Test.Blocks
{
    internal class ReadRomEepromResponse : Block
    {
        public ReadRomEepromResponse(List<byte> bytes) : base(bytes)
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
            Messenger.Instance.AddLine($"Received \"Read ROM/EEPROM Response\" block: {sb.ToString()}");
        }
    }
}
