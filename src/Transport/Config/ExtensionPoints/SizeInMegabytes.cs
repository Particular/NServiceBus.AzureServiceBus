namespace NServiceBus
{
    /// <summary>
    /// Entity size (queues and topics)
    /// </summary>
    public class SizeInMegabytes
    {
        private readonly long sizeInMegabytes;

        public static SizeInMegabytes Size1024 = new SizeInMegabytes(1024);
        public static SizeInMegabytes Size2048 = new SizeInMegabytes(2048);
        public static SizeInMegabytes Size3072 = new SizeInMegabytes(3072);
        public static SizeInMegabytes Size4096 = new SizeInMegabytes(4096);
        public static SizeInMegabytes Size5120 = new SizeInMegabytes(5120);

        private SizeInMegabytes(long sizeInMegabytes)
        {
            this.sizeInMegabytes = sizeInMegabytes;
        }

        public static implicit operator long(SizeInMegabytes size)
        {
            return size.sizeInMegabytes;
        }

        public static SizeInMegabytes Create(long sizeInMegabytes)
        {
            return new SizeInMegabytes(sizeInMegabytes);
        }
    }
}