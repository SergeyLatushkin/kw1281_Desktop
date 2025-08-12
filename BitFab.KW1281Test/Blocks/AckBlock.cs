using BitFab.KW1281Test.Actions;

namespace BitFab.KW1281Test.Blocks
{
    internal class AckBlock : Block
    {
        public AckBlock(List<byte> bytes) : base(bytes)
        {
            // Dump();
        }

        private void Dump()
        {
            Messenger.Instance.AddLine("Received ACK block");
        }
    }
}
