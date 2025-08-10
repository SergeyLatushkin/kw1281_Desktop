using BitFab.KW1281Test.Blocks;
using BitFab.KW1281Test.Messengers;
using System.Linq;
using System.Text;

namespace BitFab.KW1281Test
{
    /// <summary>
    /// Manages a dialog with a VW controller using the KW1281 protocol.
    /// </summary>
    internal interface IKW1281Dialog
    {
        ControllerInfo Connect();

        void EndCommunication();

        void SetDisconnected();

        List<Block> Login(ushort code, int workshopCode);

        List<ControllerIdent> ReadIdent();

        /// <summary>
        /// Corresponds to VDS-Pro function 19
        /// </summary>
        List<byte>? ReadEeprom(ushort address, byte count);

        bool WriteEeprom(ushort address, List<byte> values);

        /// <summary>
        /// Corresponds to VDS-Pro functions 21 and 22
        /// </summary>
        List<byte> ReadRomEeprom(ushort address, byte count);

        /// <summary>
        /// Corresponds to VDS-Pro functions 20 and 25
        /// </summary>
        List<byte>? ReadRam(ushort address, byte count);

        bool AdaptationRead(byte channelNumber);

        bool AdaptationTest(byte channelNumber, ushort channelValue);

        bool AdaptationSave(byte channelNumber, ushort channelValue, int workshopCode);

        void SendBlock(List<byte> blockBytes);

        List<Block> ReceiveBlocks();

        List<byte>? ReadCcmRom(byte seg, byte msb, byte lsb, byte count);

        /// <summary>
        /// Keep the dialog alive by sending an ACK and receiving a response.
        /// </summary>
        void KeepAlive();

        ActuatorTestResponseBlock? ActuatorTest(byte value);

        List<FaultCode>? ReadFaultCodes();

        /// <summary>
        /// Clear all of the controllers fault codes.
        /// </summary>
        /// <param name="controllerAddress"></param>
        /// <returns>Any remaining fault codes.</returns>
        List<FaultCode>? ClearFaultCodes(int controllerAddress);

        /// <summary>
        /// Set the controller's software coding and workshop code.
        /// </summary>
        /// <param name="controllerAddress"></param>
        /// <param name="softwareCoding"></param>
        /// <param name="workshopCode"></param>
        /// <returns>True if successful.</returns>
        bool SetSoftwareCoding(int controllerAddress, int softwareCoding, int workshopCode);

        bool GroupRead(byte groupNumber, bool useBasicSetting = false, CancellationToken token = default);

        List<byte> ReadSecureImmoAccess(List<byte> blockBytes);

        public IKwpCommon KwpCommon { get; }

        Block ReceiveBlock();
    }

    internal class KW1281Dialog : IKW1281Dialog
    {
        Messenger Mc = Messenger.Instance;
        DataSender Ds = DataSender.Instance;

        public ControllerInfo Connect()
        {
            _isConnected = true;
            var blocks = ReceiveBlocks();
            return new ControllerInfo(blocks.Where(b => !b.IsAckNak));
        }

        public List<Block> Login(ushort code, int workshopCode)
        {
            Mc.Add("Sending Login block");
            SendBlock(
            [
                (byte)BlockTitle.Login,
                (byte)(code >> 8),
                (byte)(code & 0xFF),
                (byte)(workshopCode >> 16),
                (byte)((workshopCode >> 8) & 0xFF),
                (byte)(workshopCode & 0xFF)
            ]);

            return ReceiveBlocks();
        }

        public List<ControllerIdent> ReadIdent()
        {
            var idents = new List<ControllerIdent>();
            bool moreAvailable;
            do
            {
                Mc.AddLine("Sending ReadIdent block");

                SendBlock(new List<byte> { (byte)BlockTitle.ReadIdent });

                var blocks = ReceiveBlocks();
                var ident = new ControllerIdent(blocks.Where(b => !b.IsAckNak));
                idents.Add(ident);

                moreAvailable = blocks
                    .OfType<AsciiDataBlock>()
                    .Any(b => b.MoreDataAvailable);
            } while (moreAvailable);

            return idents;
        }

        /// <summary>
        /// Reads a range of bytes from the EEPROM.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="count"></param>
        /// <returns>The bytes or null if the bytes could not be read</returns>
        public List<byte>? ReadEeprom(ushort address, byte count)
        {
            Mc.Add($"Sending ReadEeprom block (Address: ${address:X4}, Count: ${count:X2})");
            SendBlock(new List<byte>
            {
                (byte)BlockTitle.ReadEeprom,
                count,
                (byte)(address >> 8),
                (byte)(address & 0xFF)
            });
            var blocks = ReceiveBlocks();

            if (blocks.Count == 1 && blocks[0] is NakBlock)
            {
                // Permissions issue
                return null;
            }

            blocks = blocks.Where(b => !b.IsAckNak).ToList();
            if (blocks.Count != 1)
            {
                throw new InvalidOperationException($"ReadEeprom returned {blocks.Count} blocks instead of 1");
            }
            return blocks[0].Body.ToList();
        }

        /// <summary>
        /// Reads a range of bytes from the RAM.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="count"></param>
        /// <returns>The bytes or null if the bytes could not be read</returns>
        public List<byte>? ReadRam(ushort address, byte count)
        {
            Mc.AddLine($"Sending ReadRam block (Address: ${address:X4}, Count: ${count:X2})");
            SendBlock(new List<byte>
            {
                (byte)BlockTitle.ReadRam,
                count,
                (byte)(address >> 8),
                (byte)(address & 0xFF)
            });
            var blocks = ReceiveBlocks();

            if (blocks.Count == 1 && blocks[0] is NakBlock)
            {
                // Permissions issue
                return null;
            }

            blocks = blocks.Where(b => !b.IsAckNak).ToList();
            if (blocks.Count != 1)
            {
                throw new InvalidOperationException($"ReadEeprom returned {blocks.Count} blocks instead of 1");
            }
            return blocks[0].Body.ToList();
        }

        /// <summary>
        /// Reads a range of bytes from the CCM ROM.
        /// </summary>
        /// <param name="seg">0-15</param>
        /// <param name="msb">0-15</param>
        /// <param name="lsb">0-255</param>
        /// <param name="count">8(-12?)</param>
        /// <returns>The bytes or null if the bytes could not be read</returns>
        public List<byte>? ReadCcmRom(byte seg, byte msb, byte lsb, byte count)
        {
            Mc.Add($"Sending ReadEeprom block (Address: ${seg:X2}{msb:X2}{lsb:X2}, Count: ${count:X2})");
            var block = new List<byte>
            {
                (byte)BlockTitle.ReadEeprom,
                count,
                msb,
                lsb,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                (byte)(seg << 4)
            };
            Mc.AddLine($"SEND {Utils.Dump(block)}");
            SendBlock(block);
            var blocks = ReceiveBlocks();
#if true
            foreach (var b in blocks)
            {
                Mc.AddLine($"Received:{Utils.Dump(b.Bytes)}");
            }
#endif

            if (blocks.Count == 1 && blocks[0] is NakBlock)
            {
                // Log.WriteLine($"RECV {Utils.Dump(blocks.First().Bytes)}");
                // Permissions issue
                return null;
            }

            blocks = blocks.Where(b => !b.IsAckNak).ToList();
            if (blocks.Count != 1)
            {
                throw new InvalidOperationException($"ReadEeprom returned {blocks.Count} blocks instead of 1");
            }
            return blocks[0].Body.ToList();
        }

        public bool WriteEeprom(ushort address, List<byte> values)
        {
            Mc.AddLine($"Sending WriteEeprom block (Address: ${address:X4}, Values: {Utils.DumpBytes(values)}");

            byte count = (byte)values.Count;
            var sendBody = new List<byte>
            {
                (byte)BlockTitle.WriteEeprom,
                count,
                (byte)(address >> 8),
                (byte)(address & 0xFF),
            };
            sendBody.AddRange(values);

            SendBlock(sendBody.ToList());
            var blocks = ReceiveBlocks();

            if (blocks.Count == 1 && blocks[0] is NakBlock)
            {
                // Permissions issue
                Mc.AddLine("WriteEeprom failed");
                return false;
            }

            blocks = blocks.Where(b => !b.IsAckNak).ToList();
            if (blocks.Count != 1)
            {
                Mc.AddLine($"WriteEeprom returned {blocks.Count} blocks instead of 1");
                return false;
            }

            var block = blocks[0];
            if (block is not WriteEepromResponseBlock)
            {
                Mc.AddLine($"Expected WriteEepromResponseBlock but got {block.GetType()}");
                return false;
            }

            if (!Enumerable.SequenceEqual(block.Body, sendBody.Skip(1).Take(4)))
            {
                Mc.AddLine("WriteEepromResponseBlock body does not match WriteEepromBlock");
                return false;
            }

            return true;
        }

        public List<byte> ReadRomEeprom(ushort address, byte count)
        {
            Mc.AddLine($"Sending ReadRomEeprom block (Address: ${address:X4}, Count: ${count:X2})");
            SendBlock(new List<byte>
            {
                (byte)BlockTitle.ReadRomEeprom,
                count,
                (byte)(address >> 8),
                (byte)(address & 0xFF)
            });
            var blocks = ReceiveBlocks();

            if (blocks.Count == 1 && blocks[0] is NakBlock)
            {
                return new List<byte>();
            }

            blocks = blocks.Where(b => !b.IsAckNak).ToList();
            if (blocks.Count != 1)
            {
                throw new InvalidOperationException($"ReadRomEeprom returned {blocks.Count} blocks instead of 1");
            }
            return blocks[0].Body.ToList();
        }

        public void EndCommunication()
        {
            if (_isConnected)
            {
                Mc.AddLine("Sending End Communication block");
                SendBlock(new List<byte> { (byte)BlockTitle.End });
                _isConnected = false;
            }
        }

        public void SetDisconnected()
        {
            _isConnected = false;
            _blockCounter = null;
        }

        public void SendBlock(List<byte> blockBytes)
        {
            Thread.Sleep(25); // For better support of 4D0919035AJ

            var blockLength = (byte)(blockBytes.Count + 2);

            blockBytes.Insert(0, _blockCounter!.Value);
            _blockCounter++;

            blockBytes.Insert(0, blockLength);

            Thread.Sleep(TimeInterval.R6);

            foreach (var b in blockBytes)
            {
                WriteByteAndReadAck(b);
                Thread.Sleep(TimeInterval.R6);
            }

            KwpCommon.WriteByte(0x03); // Block end, does not get ACK'd
        }

        public List<Block> ReceiveBlocks()
        {
            var blocks = new List<Block>();

            try
            {
                while (true)
                {
                    var block = ReceiveBlock();
                    blocks.Add(block); // TODO: Maybe don't add the block if it's an Ack
                    if (block is AckBlock || block is NakBlock)
                    {
                        break;
                    }
                    SendAckBlock();
                }
            }
            catch (Exception ex)
            {
                Mc.Add($"Error receiving blocks: {ex.Message}");
                if (blocks.Count > 0)
                {
                    Mc.Add("Blocks received:");
                    foreach (var block in blocks)
                    {
                        Mc.Add($"Block: {Utils.DumpBytes(block.Bytes)}");
                    }
                }
                throw;
            }

            return blocks;
        }

        private void WriteByteAndReadAck(byte b)
        {
            KwpCommon.WriteByte(b);
            KwpCommon.ReadComplement(b);
        }

        public Block ReceiveBlock()
        {
            var blockBytes = new List<byte>();

            try
            {
                var blockLength = ReadAndAckByteFirst();
                blockBytes.Add(blockLength);

                var blockCounter = ReadBlockCounter();
                blockBytes.Add(blockCounter);

                var blockTitle = ReadAndAckByte();
                blockBytes.Add(blockTitle);

                for (var i = 0; i < blockLength - 3; i++)
                {
                    var b = ReadAndAckByte();
                    blockBytes.Add(b);
                }

                var blockEnd = KwpCommon.ReadByte();
                blockBytes.Add(blockEnd);
                if (blockEnd != 0x03)
                {
                    throw new InvalidOperationException(
                        $"Received block end ${blockEnd:X2} but expected $03. Block bytes: {Utils.Dump(blockBytes)}");
                }

                return (BlockTitle)blockTitle switch
                {
                    BlockTitle.ACK => new AckBlock(blockBytes),
                    BlockTitle.GroupReadResponseWithText => new GroupReadResponseWithTextBlock(blockBytes),
                    BlockTitle.ActuatorTestResponse => new ActuatorTestResponseBlock(blockBytes),
                    BlockTitle.AsciiData =>
                        blockBytes[3] == 0x00 ? new CodingWscBlock(blockBytes) : new AsciiDataBlock(blockBytes),
                    BlockTitle.Custom => new CustomBlock(blockBytes),
                    BlockTitle.NAK => new NakBlock(blockBytes),
                    BlockTitle.ReadEepromResponse => new ReadEepromResponseBlock(blockBytes),
                    BlockTitle.FaultCodesResponse => new FaultCodesBlock(blockBytes),
                    BlockTitle.ReadRomEepromResponse => new ReadRomEepromResponse(blockBytes),
                    BlockTitle.WriteEepromResponse => new WriteEepromResponseBlock(blockBytes),
                    BlockTitle.AdaptationResponse => new AdaptationResponseBlock(blockBytes),
                    BlockTitle.GroupReadResponse => new GroupReadResponseBlock(blockBytes),
                    BlockTitle.RawDataReadResponse => new RawDataReadResponseBlock(blockBytes),
                    BlockTitle.SecurityAccessMode2 => new SecurityAccessMode2Block(blockBytes),
                    _ => new UnknownBlock(blockBytes),
                };
            }
            catch (Exception ex)
            {
                Mc.AddLine($"Error receiving block: {ex.Message}");
                Mc.AddLine($"Partial block: {Utils.DumpBytes(blockBytes)}");
                if (ex is TimeoutException)
                {
                    Mc.AddLine($"Read timeout: {KwpCommon.Interface.ReadTimeout}");
                    Mc.AddLine($"Write timeout: {KwpCommon.Interface.WriteTimeout}");
                }
                throw;
            }
        }

        private void SendAckBlock()
        {
            var blockBytes = new List<byte> { (byte)BlockTitle.ACK };
            SendBlock(blockBytes);
        }

        private byte ReadBlockCounter()
        {
            var blockCounter = ReadAndAckByte();
            if (!_blockCounter.HasValue)
            {
                // First block
                _blockCounter = blockCounter;
            }
            else if (blockCounter != _blockCounter)
            {
                throw new InvalidOperationException(
                    $"Received block counter ${blockCounter:X2} but expected ${_blockCounter:X2}");
            }
            _blockCounter++;
            return blockCounter;
        }

        private byte ReadAndAckByte()
        {
            var b = KwpCommon.ReadByte();
            Thread.Sleep(TimeInterval.R6);
            var complement = (byte)~b;
            KwpCommon.WriteByte(complement);
            return b;
        }

        /// <summary>
        /// https://github.com/gmenounos/kw1281test/issues/93
        /// </summary>
        private byte ReadAndAckByteFirst(int count = 0)
        {
            if (count > 5)
            {
                throw new InvalidOperationException(
                    $"Cannot sync with {count} repeated attempts.");
            }
            var b = KwpCommon.ReadByte();
            if (b == 0x55)
            {
                var keywordLsb = KwpCommon.ReadByte();
                var keywordMsb = KwpCommon.ReadByte();
                var complement = (byte)~keywordMsb;
                BusyWait.Delay(25);
                KwpCommon.WriteByte(complement);
                Mc.AddLine($"Warning. Sync repeated.");
                return ReadAndAckByteFirst(count);
            }
            else
            {
                Thread.Sleep(TimeInterval.R6);
                var complement = (byte)~b;
                KwpCommon.WriteByte(complement);
                return b;
            }
        }

        public void KeepAlive()
        {
            SendAckBlock();
            var block = ReceiveBlock();
            if (block is not AckBlock)
            {
                throw new InvalidOperationException(
                    $"Received 0x{block.Title:X2} block but expected ACK");
            }
        }

        public ActuatorTestResponseBlock? ActuatorTest(byte value)
        {
            Mc.AddLine($"Sending actuator test 0x{value:X2} block");
            SendBlock(new List<byte>
            {
                (byte)BlockTitle.ActuatorTest,
                value
            });

            var blocks = ReceiveBlocks();
            blocks = blocks.Where(b => !b.IsAckNak).ToList();
            if (blocks.Count != 1)
            {
                Mc.AddLine($"ActuatorTest returned {blocks.Count} blocks instead of 1");
                return null;
            }

            var block = blocks[0];
            if (block is not ActuatorTestResponseBlock)
            {
                Mc.AddLine($"Expected ActuatorTestResponseBlock but got {block.GetType()}");
                return null;
            }

            return (ActuatorTestResponseBlock)block;
        }

        public List<FaultCode>? ReadFaultCodes()
        {
            Mc.AddLine($"Sending ReadFaultCodes block");
            SendBlock(new List<byte>
            {
                (byte)BlockTitle.FaultCodesRead
            });

            var blocks = ReceiveBlocks();
            blocks = blocks.Where(b => !b.IsAckNak).ToList();

            var faultCodes = new List<FaultCode>();
            foreach (var block in blocks)
            {
                if (block is not FaultCodesBlock)
                {
                    Mc.AddLine($"Expected FaultCodesBlock but got {block.GetType()}");
                    return null;
                }

                var faultCodesBlock = (FaultCodesBlock)block;
                faultCodes.AddRange(faultCodesBlock.FaultCodes);
            }

            return faultCodes;
        }

        public List<FaultCode>? ClearFaultCodes(int controllerAddress)
        {
            Mc.AddLine($"Sending ClearFaultCodes block");
            SendBlock([ (byte)BlockTitle.FaultCodesDelete ]);

            var blocks = ReceiveBlocks();
            blocks = blocks.Where(b => !b.IsAckNak).ToList();

            var faultCodes = new List<FaultCode>();
            foreach (var block in blocks)
            {
                if (block is not FaultCodesBlock)
                {
                    Mc.AddLine($"Expected FaultCodesBlock but got {block.GetType()}");
                    return null;
                }

                var faultCodesBlock = (FaultCodesBlock)block;
                faultCodes.AddRange(faultCodesBlock.FaultCodes);
            }

            return faultCodes;
        }

        public bool SetSoftwareCoding(int controllerAddress, int softwareCoding, int workshopCode)
        {
            // Workshop codes > 65535 overflow into the low bit of the software coding
            var bytes = new List<byte>
            {
                (byte)BlockTitle.SoftwareCoding,
                (byte)(softwareCoding * 2 / 256),
                (byte)(softwareCoding * 2 % 256),
                (byte)((workshopCode & 65535) / 256),
                (byte)(workshopCode % 256)
            };

            if (workshopCode > 65535)
            {
                bytes[2]++;
            }

            Mc.AddLine($"Sending SoftwareCoding block");
            SendBlock(bytes);

            var blocks = ReceiveBlocks();
            if (blocks.Count == 1 && blocks[0] is NakBlock)
            {
                return false;
            }

            var controllerInfo = new ControllerInfo(blocks.Where(b => !b.IsAckNak));
            return
                controllerInfo.SoftwareCoding == softwareCoding &&
                controllerInfo.WorkshopCode == workshopCode;
        }

        public bool AdaptationRead(byte channelNumber)
        {
            Mc.AddLine($"Sending AdaptationRead block");
            SendBlock([ (byte)BlockTitle.AdaptationRead, channelNumber ]);

            return ReceiveAdaptationBlock();
        }

        public bool AdaptationTest(byte channelNumber, ushort channelValue)
        {
            var bytes = new List<byte>
            {
                (byte)BlockTitle.AdaptationTest,
                channelNumber,
                (byte)(channelValue / 256),
                (byte)(channelValue % 256)
            };

            Mc.AddLine($"Sending AdaptationTest block");
            SendBlock(bytes);

            return ReceiveAdaptationBlock();
        }

        public bool AdaptationSave(byte channelNumber, ushort channelValue, int workshopCode)
        {
            List<byte> bytes = [
                (byte)BlockTitle.AdaptationSave,
                channelNumber,
                (byte)(channelValue / 256),
                (byte)(channelValue % 256),
                (byte)(workshopCode >> 16),
                (byte)((workshopCode >> 8) & 0xFF),
                (byte)(workshopCode & 0xFF)
            ];

            Mc.AddLine($"Sending AdaptationSave block");
            SendBlock(bytes);

            return ReceiveAdaptationBlock();
        }

        private bool ReceiveAdaptationBlock()
        {
            var responseBlock = ReceiveBlock();
            if (responseBlock is NakBlock)
            {
                Mc.AddLine($"Received a NAK.");
                return false;
            }

            if (responseBlock is not AdaptationResponseBlock adaptationResponse)
            {
                Mc.AddLine($"Expected an Adaptation response block but received a ${responseBlock.Title:X2} block.");
                return false;
            }

            Mc.AddLine($"Adaptation value: {adaptationResponse.ChannelValue}");

            return true;
        }

        public bool GroupRead(byte groupNumber, bool useBasicSetting = false, CancellationToken token = default)
        {
            if (groupNumber == 0)
            {
                return RawDataRead(useBasicSetting, token); //TODO
            }

            if (useBasicSetting)
            {
                Mc.AddLine($"Sending Basic Setting Read blocks...");
            }
            else
            {
                Mc.AddLine($"Sending Group Read blocks...");
            }

            GroupReadResponseWithTextBlock? textBlock = null;

            List<byte> bytes = [
                (byte)(useBasicSetting ? BlockTitle.BasicSettingRead : BlockTitle.GroupRead),
                groupNumber ];
            SendBlock(bytes);

            Block responseBlock = ReceiveBlock();
            if (responseBlock is NakBlock)
            {
                List<KeyValuePair<byte, string>> result = [ new KeyValuePair<byte, string>(0,$"Not Available") ];
                Ds.Send(result);
            }
            else if (responseBlock is GroupReadResponseWithTextBlock groupReadResponseWithText)
            {
                Mc.AddLine($"{groupReadResponseWithText}");
                textBlock = groupReadResponseWithText;
            }
            else if (responseBlock is GroupReadResponseBlock groupReading)
            {
                IEnumerable<KeyValuePair<byte, string>> result = groupReading.SensorValues
                    .Select(group => new KeyValuePair<byte, string>(group.SensorID, group.ToString()));

                Ds.Send(result);
            }
            else if (responseBlock is RawDataReadResponseBlock rawData)
            {
                if (textBlock != null && rawData.Body.Count > 0)
                {
                    var sb = new StringBuilder(textBlock.GetText(rawData.Body[0]));
                    sb.Append(Utils.DumpDecimal(rawData.Body.Skip(1)));

                    List<KeyValuePair<byte, string>> result = [ new KeyValuePair<byte, string>(0,sb.ToString()) ];
                    Ds.Send(result);
                }
                else
                {
                    List<KeyValuePair<byte, string>> result = [ new KeyValuePair<byte, string>(0, rawData.ToString()) ];
                    Ds.Send(result);
                }
            }
            else
            {
                Mc.AddLine($"Expected a Group Reading response block but received a ${responseBlock.Title:X2} block.");

                return false;
            }

            Mc.AddLine();

            return true;
        }

        private bool RawDataRead(bool useBasicSetting, CancellationToken token = default)
        {
            if (useBasicSetting)
            {
                Mc.AddLine($"Sending Basic Setting Raw Data Read block");
            }
            else
            {
                Mc.AddLine($"Sending Raw Data Read block");
            }

            while (!token.IsCancellationRequested)
            {
                List<byte> bytes = [ (byte)(useBasicSetting ? BlockTitle.BasicSettingRawDataRead : BlockTitle.RawDataRead) ];
                SendBlock(bytes);

                var responseBlock = ReceiveBlock();

                if (responseBlock is not RawDataReadResponseBlock rawDataReadResponse)
                {
                    Mc.AddLine($"Expected a Raw Data Read response block but received a ${responseBlock.Title:X2} block.");

                    return false;
                }

                List<KeyValuePair<byte, string>> result = [
                    new KeyValuePair<byte, string>(0, rawDataReadResponse.ToString())
                ];

                Ds.Send(result);
            }

            Mc.AddLine();

            return true;
        }

        public List<byte> ReadSecureImmoAccess(List<byte> blockBytes)
        {
            blockBytes.Insert(0, (byte)BlockTitle.SecurityImmoAccess1);

            Mc.AddLine($"Sending ReadSecureImmoAccess block: {Utils.DumpBytes(blockBytes)}");

            SendBlock(blockBytes);
            var blocks = ReceiveBlocks();

            if (blocks.Count == 1 && blocks[0] is NakBlock)
            {
                return [];
            }

            blocks = blocks.Where(b => !b.IsAckNak).ToList();
            if (blocks.Count != 1)
            {
                throw new InvalidOperationException($"ReadRomEeprom returned {blocks.Count} blocks instead of 1");
            }
            return blocks[0].Body.ToList();
        }

        private static class TimeInterval
        {
            /// <summary>
            /// Time to wait in milliseconds after receiving a byte from the ECU before sending the next byte.
            /// Valid range: 1-50ms (according to SAE J2818)
            /// </summary>
            public const int R6 = 2;
        }

        public IKwpCommon KwpCommon { get; }

        private bool _isConnected;

        private byte? _blockCounter;

        public KW1281Dialog(IKwpCommon kwpCommon)
        {
            KwpCommon = kwpCommon;
            _isConnected = false;
            _blockCounter = null;
        }
    }

    /// <summary>
    /// Used for commands such as ActuatorTest which need to be kept alive with ACKs while waiting
    /// for user input.
    /// </summary>
    internal class KW1281KeepAlive : IDisposable
    {
        private readonly IKW1281Dialog _kw1281Dialog;
        private volatile bool _cancel = false;
        private Task? _keepAliveTask = null;

        public KW1281KeepAlive(IKW1281Dialog kw1281Dialog)
        {
            _kw1281Dialog = kw1281Dialog;
        }

        public ActuatorTestResponseBlock? ActuatorTest(byte value)
        {
            Pause();
            var result = _kw1281Dialog.ActuatorTest(value);
            Resume();
            return result;
        }

        public void Dispose()
        {
            Pause();
        }

        private void Pause()
        {
            _cancel = true;
            if (_keepAliveTask != null)
            {
                _keepAliveTask.Wait();
            }
        }

        private void Resume()
        {
            _keepAliveTask = Task.Run(KeepAlive);
        }

        private void KeepAlive()
        {
            _cancel = false;
            while (!_cancel)
            {
                _kw1281Dialog.KeepAlive();
                //TODO: Mc.Add(".");
            }
        }
    }
}
