// Checksum Calculation for the Hart protocol
private byte CalCheckSum(byte[] _PacketData, int PacketLength, int startBit)
{
    Byte _CheckSumByte = 0x00;
    for (int i = startBit; i < PacketLength; i++)
    {
        _CheckSumByte ^= _PacketData[i];
    }
    return _CheckSumByte;
}

/// <summary>
/// Reads the data from a serial port.
/// Creates a list off bytes (buffer) with the size of the amount of bytes "Bytes To Read"
/// Next the data is read from the port and added to the Queue
/// </summary>
private static void FetchDatafromComPort()
{
    DateTime currentDateTime = DateTime.Now;

    do
    {
        // Wait 10 seconds for the answer
        // Read the buffer until the expect answer is there
        if (DateTime.Now <= currentDateTime.AddSeconds(10))
        {
            // wait for the Hart to respond
            Thread.Sleep(25);
            byte[] buf = new byte[serialPort.BytesToRead];
            serialPort.Read(buf, 0, buf.Length);
            buf.ToList().ForEach(b => Queue.Enqueue(b));
            Thread.Sleep(10);
            ProcessData();
        }
        else
        {
            CommunicationRunning = false;
        }
    }
    while (CommunicationRunning);
}

/// <summary>
/// Because the bytes will not come in all at once, we need to check if we are ready to stop
/// if we got the last byte we are done
/// </summary>
private static void ProcessData()
{
    // when there is something on the Queue, then we can read
    // we need the first byte that is not 255 and ignore byte[0]
    // then we calculate the checksum based on the number of bytes and starting at the index
    // the checksum is needs to be equal to the last byte we receive (in the Queue), 
    // if that is equal we got all the bytes
    if (Queue.Count > 0)
    {
        List<byte> byteList = Queue.ToList();
        byte c = byteList.FirstOrDefault(x => x != byteList[0] && x != 255);
        int index = byteList.IndexOf(c);

        if (index > 2)
        {
            byte checksum = CalCheckSum(byteList.ToArray(), byteList.Count-1, index);
            if (checksum == Queue.ElementAt(Queue.Count - 1) && serialPort.BytesToRead == 0)
            {
                CommunicationRunning = false;
            }
        }
    }
}
