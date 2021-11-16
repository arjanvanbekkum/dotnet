using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
 
namespace Data.Repositories
{
    public class AsciiRepository
    {
        private bool _serialPortInitialised { get; set; }
        private readonly ISerialPortContext _serialPortContext;
        private Queue<byte> _queue { get; set; }
        private bool _communicationRunning { get; set; }
        private int _queueLength { get; set; }
        public AsciiRepository(ISerialPortContext serialPortContext)
        {
            _serialPortInitialised = false;
            _communicationRunning = false;
            _queue = new Queue<byte>();
            _serialPortContext = serialPortContext;
        }

        // address is something like 10003, 10011
        // this is multiplied with the portNumber because there can be multiple devices
        // each with their own set of register settings
        public double GetValue(Entity entity, int address) 
        {
            if (!_serialPortInitialised)
            {
                DetectAndOpenSerialPort();
            }
            _queue.Clear();
            int StartAdres = (address * entity.Portnumber);
            SendModbusCommand(StartAdres, 1);
            _queueLength = 27; // determine the lenght for the answer could be something else then 27
            FetchDatafromComPort();
            return Math.Round(GetValueFromQueue(), 2);
        }

        private void DetectAndOpenSerialPort()
        {
            try
            {
                string ObjectQuery = "SELECT * FROM Win32_PnPEntity WHERE ClassGuid =\"{4d36e978-e325-11ce-bfc1-08002be10318}\"";
                string Scope = "root\\CIMV2";
                string PortName = "USB Serial";
                serialPortContext.DetectAndOpenSerialPort(Scope, ObjectQuery, PortName, 9600, Parity.None, 4096, Handshake.None);
                _serialPortInitialised = true;
            }
            catch (Exception ex)
            {
                throw new SerialPortNotFoundException();
            }
        }

        private void FetchDatafromComPort()
        {
            DateTime currentDateTime = DateTime.Now;
 
            do
            {
                // wait for 10 seconds, then start processing
                // then read the bytes from the serialport context
                // put them in the queue and check if the answer is done
                if (DateTime.Now <= currentDateTime.AddSeconds(10))
                {
                    byte[] buf = new byte[serialPortContext.BytesToRead()];
                    serialPortContext.Read(buf, 0, buf.Length);
                    buf.ToList().ForEach(b => _queue.Enqueue(b));
                    ProcessData();
                }
                else
                {
                    throw new SerialCommunicationTimeOutException();
                }
            }
            while (_communicationRunning);
        }

        private void ProcessData()
        {
            List<byte> byteList = _queue.ToList();
            for (int i = 0; i < byteList.Count - 1; i++)
            {
                LoggerDebug(this, string.Format("0x{0:x2} ", byteList[i]));
            }
 
            if (_queue.Count == _queueLength)
            {
                _communicationRunning = false;
            }
        }
 
        /// <summary>
        ///  this communication is in Modbus ASCII, this means we have to send the command using ASCII characters
        ///  if we want to send the following command
        ///      0x01 0x03 0x27 0x10 0x00 0x0c
        ///  It must be translated to 
        ///      0x3a 0x30 0x31 0x30 0x33 0x32 0x37 0x31 0x30 0x30 0x30 0x30 0x43 0x42 0x39 0x0d 0x0a
        ///    
        ///  0x3a -> : => colon
        ///  
        ///  0x01
        ///  -> ASCII Char 0 = 0x30 
        ///  -> ASCII Char 1 = 0x31  
        ///  
        ///  0x03
        ///  -> ASCII Char 0 = 0x30
        ///  -> ASCII Char 3 = 0x33  
        ///  
        ///  0x27
        ///  -> ASCII Char 2 = 0x32 
        ///  -> ASCII Char 7 = 0x37 
        ///  
        ///  0x0c
        ///  -> ASCII Char 0 = 0x30
        ///  -> ASCII Char c (12) = 0x43 
        ///  
        ///  0xB9 (LRC Check)
        ///  -> ASCII Char B (11) = 0x42
        ///  -> ASCII Char 9 = 0x39 
        ///  
        ///  0x0d; -> CR => Carriage return
        ///  0x0a; -> LF => Line Feed
        /// </summary>
        /// <param name="StartAdres"></param>
        /// <param name="NrOfRegisters"></param>
        private void SendModbusCommand(int StartAdres, int NrOfRegisters)
        {
            try
            {
                _communicationRunning = true;
 
                byte[] buffer = new byte[6];
                
                // translation of the "real" command to a byte[]
                buffer[0] = Convert.ToByte(1); // device = 1
                buffer[1] = Convert.ToByte(3); // fetch holding registers function
                buffer[2] = Convert.ToByte(StartAdres / 256);
                buffer[3] = Convert.ToByte(StartAdres % 256);
                buffer[4] = Convert.ToByte(NrOfRegisters / 256);
                buffer[5] = Convert.ToByte(NrOfRegisters % 256);
 
                byte[] SendBuffer = new byte[17];
                char charFirst;
                char charLast;
                int Index = 0;
                // add a colon ":"
                SendBuffer[Index++] = 0x3A;
 
                // for each item in the buffer, split in two chars and make sure
                // the size is always two chars
                // then convert it to bytes again and add them to the sendbuffer
                for (int iIndex = 0; iIndex < 6; iIndex++)
                {
                    string upper = buffer[iIndex].ToString("X").PadLeft(2, '0');
                    charFirst = upper.First();
                    charLast = upper.Last();
 
                    SendBuffer[Index++] = Convert.ToByte(charFirst);
                    SendBuffer[Index++] = Convert.ToByte(charLast);
                }
 
                // LRC -> Longitudinal Redundancy Check.
                // complete for original buffer values 
                int intValue = 0;
                // sum of all decimal values
                for (int iIndex = 0; iIndex < 6; iIndex++)
                {
                    intValue += buffer[iIndex];
                }
                // make it negative 
                intValue = intValue * -1;
                // convert to hex and strip F values
                string hexValue = intValue.ToString("X").Replace("F", string.Empty).PadLeft(2, '0');
                // split into char values
                charFirst = hexValue.First();
                charLast = hexValue.Last();
                // then convert it to bytes again and add them to the sendbuffer
                SendBuffer[Index++] = Convert.ToByte(charFirst);
                SendBuffer[Index++] = Convert.ToByte(charLast);
                // add two more characters to the buffer
                SendBuffer[Index++] = 0x0d; // CR => Carriage return
                SendBuffer[Index++] = 0x0a; // LF => Line Feed
 
                serialPortContext.Write(SendBuffer, 0, SendBuffer.Length);
            }
            catch (Exception ex)
            {
                throw new SendingModbusCommandException();
            }
        }
 
        /// <summary>
        /// The return value is also in an ASCII format, so translation is required before calculation the real value
        /// In the response we can "ignore" the first 7 bytes (0x3a, 0x30, 0x31, 0x30, 0x33, 0x30, 0x38)
        /// telling us the colon (0x3a), the device (0x30, 0x31), the function (0x30, 0x33) and the lenght(0x30, 0x38)
        /// we can also ignore the last 4 containing the LRC, CR and LF
        /// So now we have to translate the 16 bytes into 8 bytes, converting them from ASCII to Hex using the sByte with base 16
        /// This translate to something like this
        ///    0x40, 0x21, 0x68, 0x75, 0x11, 0x64, 0x16, 0x53
        /// add the new byte to a new list and use Endian conversion (Reverse) and convert the list to a double
        /// 
        /// Example response
        ///   0x3a, 0x30, 0x31, 0x30, 0x33, 0x30, 0x38, 
        ///   0x34, 0x30, 0x32, 0x31, 0x36, 0x38, 0x37, 0x35, 0x31, 0x31, 0x36, 0x44, 0x41, 0x36, 0x45, 0x43, 
        ///   0x41, 0x36, 0x0d, 0x0a
        /// </summary>
        /// <returns></returns>
        private double GetValueFromQueue()
        {
            List<byte> byteResponse = new List<byte>();
 
            byte[] byteList = _queue.ToArray();
            char[] charArray = Encoding.ASCII.GetChars(byteList);
 
            for (int iIndex = 7; iIndex < byteList.Count() - 4; iIndex += 2)
            {
                string stringValue = charArray[iIndex].ToString() + charArray[iIndex + 1].ToString();
                byte resultByte = (byte)Convert.ToSByte(stringValue, 16);
                byteResponse.Add(resultByte);
            };
 
            byteResponse.Reverse();
 
            return BitConverter.ToDouble(byteResponse.ToArray(), 0);
        }
    }
}
