// Sending Hart Command 1 to the device can be used for getting the values from the device
private void SendHartCommand_1(Device device)
{
    byte[] sendBuf = new byte[16];
    int index = 0;

    // number of preambles for the device from Hart Command 0
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

    sendBuf[index++] = Convert.ToByte(1); //  command

    sendBuf[index++] = 0x00; // data

    sendBuf[index++] = CalCheckSum(sendBuf, sendBuf.Length, 6); // xor check

    Console.WriteLine("command 1....");
    SerialPort.Write(sendBuf, 0, sendBuf.Length);
}


// Process Hart Command 1 Response
private double ProcessHartResponse_1()
{
    List<byte> bytelist = Queue.ToList();
    int startInt = bytelist.IndexOf(Convert.ToByte(StartByte));

    // IEEE574 conversion
    List<byte> ieee574ByteList = new List<byte>
    {
        bytelist[startInt + 10],
        bytelist[startInt + 11],
        bytelist[startInt + 12],
        bytelist[startInt + 13]
    };

    ieee574ByteList.Reverse();

    return BitConverter.ToSingle(ieee574ByteList.ToArray(), 0);
}