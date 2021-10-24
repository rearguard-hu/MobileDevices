namespace MobileDevices.iOS.AfcServices
{
    public struct AfcPacketHeard
    {

        public ulong Magic;

        public ulong EntireLength;

        public ulong ThisLength;

        public ulong PacketNum;

        public AfcOperations Operation;

    }
}