namespace X
{
    using System;
    using NServiceBus;

    public class KickOffCommand : ICommand
    {
        public Guid Id { get; set; }
    }
    public class Cmd : ICommand
    {
    }
}