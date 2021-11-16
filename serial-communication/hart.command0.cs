// Use this command to find all connected devices  
// sending the hart command 0 to the device

private void SendHartCommand_0(int adress)
{
    byte[] sendBuf = new byte[30];
    int index = 0;

    // always start with 20 preambles for a device, 
    // the answer will contain the number of preambles you need for the device
    for (int i = 0; i < 20; ++i)
    {
        sendBuf[index++] = Convert.ToByte(255); 
    }
    sendBuf[index++] = 0x02; // master number
    sendBuf[index++] = Convert.ToByte(adress); // adress start at 1 until n
    sendBuf[index++] = Convert.ToByte(0); //  command
    sendBuf[index++] = 0x00; // data
    sendBuf[index++] = CalCheckSum(sendBuf, sendBuf.Length, 20); // xor check

    Console.WriteLine("command 0....");
    SerialPort.Write(sendBuf, 0, sendBuf.Length);
}

// process the Hart Command 0 response.
private void ProcessHartResponse_0(Device device)
{
    List<byte> byteList = Queue.ToList();
    // for command 0 the starting bit is 254
    int startInt = byteList.IndexOf(Convert.ToByte(StartByte)); 

    // Next is the manufacturer
    switch (Convert.ToInt32(byteList[startInt + 1])) 
    {
        case 38:
            device.Manufacturer = "Rosemount";
            device.StartAdress = 0xa6;
            break;
        case 23:
            device.Manufacturer = "Honeywell";
            device.StartAdress = 0x97;
            break;
        default:
            device.Manufacturer = "<unknown>";
            break;
    }
    device.DeviceType = byteList[startInt + 2]; // Device typ
    device.Preambles = byteList[startInt + 3]; // The number of preambles

    // The serialnumber according to BigEndian (byte sort)
    device.DeviceIdentificationNumber[0] = byteList[startInt + 9];
    device.DeviceIdentificationNumber[1] = byteList[startInt + 10];
    device.DeviceIdentificationNumber[2] = byteList[startInt + 11];

    // BigEndian conver to serial number
    device.Serienummer = byteList[startInt + 9] << 16 | byteList[startInt + 10] << 8 | byteList[startInt + 11];
}

