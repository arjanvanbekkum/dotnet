using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Linq;
using System.Management;
 
namespace Data.Contexts
{
    public class SerialPortContext : ISerialPortContext
    {
        private SerialPort _serialPort { get; set; }
        
        public void DetectAndOpenSerialPort(string scope, string query, string portName, int baudRate, Parity parity, int readBufferSize, Handshake handshake)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            string SerialPortName = GetSerialPortName(searcher, portName);
            _serialPort = CreateSerialPort(SerialPortName, baudRate, parity, readBufferSize, handshake);
            _serialPort.Open();
        }
        
        public int BytesToRead()
        {
            return _serialPort.BytesToRead;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _serialPort.Read(buffer, offset, count);
        }
        
        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }

        private string GetSerialPortName(ManagementObjectSearcher searcher, string portName)
        {
            ManagementBaseObject port;
            List<ManagementBaseObject> ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
            port = ports.Find(p => p["Caption"].ToString().Contains(portName));
 
            // Get the index number where "(COM" starts in the string
            int indexOfCom = port["Caption"].ToString().IndexOf("(COM");
            string SerialPortName = port["Caption"].ToString().Substring(indexOfCom + 1, port["Caption"].ToString().Length - indexOfCom - 2);
 
            return SerialPortName;
        }

        private SerialPort CreateSerialPort(string SerialPortName, int baudRate, Parity parity, int readBufferSize, Handshake handshake)
        {
            // Create a new SerialPort obv het gevonden DeviceID
            _serialPort = new SerialPort
            {
                // Allow the user to set the appropriate properties.
                PortName = SerialPortName,
                BaudRate = baudRate,
                Parity = parity,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = handshake,
                ReadBufferSize = readBufferSize,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
 
            return _serialPort;
        }
    }
}
