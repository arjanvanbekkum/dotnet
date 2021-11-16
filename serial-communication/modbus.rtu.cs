// Modbus uses registers for functions to read data from the device 
private void SendModbusCommand(int StartAdres, int NrOfRegisters)
{
    CommunicationRunning = true;

    byte[] buffer = new byte[8];

    buffer[0] = Convert.ToByte(1); // device = 1
    buffer[1] = Convert.ToByte(3); // fetch holding registers function
    buffer[2] = Convert.ToByte(StartAdres / 256);
    buffer[3] = Convert.ToByte(StartAdres % 256);
    buffer[4] = Convert.ToByte(NrOfRegisters / 256);
    buffer[5] = Convert.ToByte(NrOfRegisters % 256);
    byte[] olDcheckSum = CalcCRC16(buffer, 6); ;
    buffer[6] = olDcheckSum[0];
    buffer[7] = olDcheckSum[1];

    SerialPort.Write(buffer, 0, buffer.Length);
}

// modbus CRC calculation
private static byte[] CalcCRC16(byte[] data, int length)
{
    ushort CRCFull = 0xFFFF;
    byte CRCHigh = 0xFF, CRCLow = 0xFF;
    char CRCLSB;

    for (int i = 0; i < (length); i++)
    {
        CRCFull = (ushort)(CRCFull ^ data[i]);

        for (int j = 0; j < 8; j++)
        {
            CRCLSB = (char)(CRCFull & 0x0001);
            CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

            if (CRCLSB == 1)
                CRCFull = (ushort)(CRCFull ^ 0xA001);
        }
    }
    byte[] crcByte = new byte[2];
    crcByte[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
    crcByte[0] = CRCLow = (byte)(CRCFull & 0xFF);
    return crcByte;
}

// read the value from the queue that is filled by reading the serial port bytes
private double GetValueFromQueue(int Index)
{
    List<byte> byteResponse = Queue.ToList();
    List<byte> Response = new List<byte>();

    Response.Add(byteResponse[Index + 0]);
    Response.Add(byteResponse[Index + 1]);
    Response.Add(byteResponse[Index + 2]);
    Response.Add(byteResponse[Index + 3]);

    // convert from IEEE to double   
    Console.WriteLine("Value: " + BitConverter.ToSingle(Response.ToArray(), 0));

    return BitConverter.ToSingle(Response.ToArray(), 0);
}