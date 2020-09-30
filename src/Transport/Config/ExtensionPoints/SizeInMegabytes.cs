namespace NServiceBus
{
    /// <summary>
    /// Entity size (queues and topics)
    /// </summary>
    public class SizeInMegabytes
    {
        SizeInMegabytes(long sizeInMegabytes)
        {
            this.sizeInMegabytes = sizeInMegabytes;
        }

        /// <summary></summary>
        public static implicit operator long(SizeInMegabytes size)
        {
            return size.sizeInMegabytes;
        }

        /// <summary></summary>
        public static SizeInMegabytes Create(long sizeInMegabytes)
        {
            return new SizeInMegabytes(sizeInMegabytes);
        }

        readonly long sizeInMegabytes;

        /// <summary></summary>
        public static SizeInMegabytes Size1024 = new SizeInMegabytes(1024);

        /// <summary></summary>
        public static SizeInMegabytes Size2048 = new SizeInMegabytes(2048);

        /// <summary></summary>
        public static SizeInMegabytes Size3072 = new SizeInMegabytes(3072);

        /// <summary></summary>
        public static SizeInMegabytes Size4096 = new SizeInMegabytes(4096);

        /// <summary></summary>
        public static SizeInMegabytes Size5120 = new SizeInMegabytes(5120);
    }
}