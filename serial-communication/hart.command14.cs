// Sending Hart Command 14, for getting all the properties from a specific device
private void SendHartCommand_14(Device device)
{
    byte[] sendBuf = new byte[16];
    int index = 0;

    // number off preambles from Hart Command 0
    for (int i = 0; i < 6; ++i)
    {
        sendBuf[index++] = Convert.ToByte(255);
    }

    sendBuf[index++] = 0x82; //start-byte delimitter
    sendBuf[index++] = device.StartAdress; // address
    sendBuf[index++] = device.DeviceType; // address device type from command 0
    sendBuf[index++] = device.DeviceIdentificationNumber[0]; // address device identifier
    sendBuf[index++] = device.DeviceIdentificationNumber[1]; // address device identifier
    sendBuf[index++] = device.DeviceIdentificationNumber[2]; // address device identifier

    sendBuf[index++] = Convert.ToByte(14); //  command

    sendBuf[index++] = 0x00; // data

    sendBuf[index++] = CalCheckSum(sendBuf, sendBuf.Length, 6); // xor check

    logger.Debug("command 14....");
    SerialPort.Write(sendBuf, 0, sendBuf.Length);
}

// Process Hart Command 14 response
private void ProcessHartResponse_14(Device device)
{
    List<byte> byteList = Queue.ToList();
    // For Command 14 this is the first byte from the address in command 0;
    int startInt = byteList.IndexOf(Convert.ToByte(StartByte)); 

    List<byte> upper = new List<byte>();
    // Get the unit
    switch ( byteList[startInt + 7] )
    {
        case 7:
            device.UnitCode = "Bar";
            break;
        case 32:
            device.UnitCode = "Celcius";
            break; 
        default:
            break;
    }

    // IEEE574 conversion
    upper.Add(byteList[startInt + 8]);
    upper.Add(byteList[startInt + 9]);
    upper.Add(byteList[startInt + 10]);
    upper.Add(byteList[startInt + 11]);
    // reverse byte order 
    upper.Reverse();
    // convert            
    device.UpperLimit = BitConverter.ToSingle(upper.ToArray(), 0);
    
    // reverse byte order
    upper.Add(byteList[startInt + 12]);
    upper.Add(byteList[startInt + 13]);
    upper.Add(byteList[startInt + 14]);
    upper.Add(byteList[startInt + 15]);
    // reverse byte order 
    upper.Reverse();
    // convert
    device.LowerLimit = BitConverter.ToSingle(upper.ToArray(), 0);
}

