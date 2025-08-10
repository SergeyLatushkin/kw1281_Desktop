﻿using BitFab.KW1281Test.Blocks;
using BitFab.KW1281Test.Messengers;
using System.Text;

namespace BitFab.KW1281Test
{
    /// <summary>
    /// The info returned by the controller to a ReadIdent block.
    /// </summary>
    internal class ControllerIdent
    {
        public ControllerIdent(IEnumerable<Block> blocks)
        {
            var sb = new StringBuilder();
            foreach (var block in blocks)
            {
                if (block is AsciiDataBlock asciiBlock)
                {
                    sb.Append(asciiBlock);
                }
                else if (block is CodingWscBlock codingWscBlock)
                {
                    sb.AppendLine();
                    sb.Append(codingWscBlock);
                }
                else
                {
                    Messenger.Instance.AddLine($"ReadIdent returned block of type {block.GetType()}");
                }
            }
            Text = sb.ToString();
        }

        public string Text { get; }

        public override string ToString()
        {
            return Text;
        }
    }
}